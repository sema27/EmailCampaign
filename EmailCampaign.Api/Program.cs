using EmailCampaign.Api.Middlewares;
using EmailCampaign.Application.Campaigns.Commands.StartSendCampaign;
using EmailCampaign.Application.Campaigns.Enums;
using EmailCampaign.Application.Campaigns.Handlers.StartSendCampaign;
using EmailCampaign.Application.Campaigns.Mapping;
using EmailCampaign.Application.Campaigns.Services;
using EmailCampaign.Application.Common.Abstractions;
using EmailCampaign.Application.Common.Options;
using EmailCampaign.Application.Common.Options.Validation;
using EmailCampaign.Application.Common.Repositories;
using EmailCampaign.Application.Common.Services;
using EmailCampaign.Application.Stats.Services;
using EmailCampaign.Infrastructure.Persistence;
using EmailCampaign.Infrastructure.Persistence.Repositories;
using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Polly;
using Polly.Extensions.Http;
using Prometheus;
using Serilog;
using Swashbuckle.AspNetCore.Filters;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;

var builder = WebApplication.CreateBuilder(args);

// -------- Logging (Serilog) --------
builder.Host.UseSerilog((ctx, services, cfg) =>
{
    cfg.ReadFrom.Configuration(ctx.Configuration)
       .ReadFrom.Services(services)
       .Enrich.FromLogContext()
       .WriteTo.File("logs/api-.log", rollingInterval: RollingInterval.Day);
});

// -------- Rate Limiting --------
builder.Services.AddRateLimiter(options =>
{
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(ctx =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: ctx.User?.Identity?.Name
                ?? ctx.Connection.RemoteIpAddress?.ToString() ?? "anon",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 100,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0
            }));

    options.RejectionStatusCode = 429;
});

// -------- HttpClient + Polly Retry --------
builder.Services.AddHttpClient("default", c =>
{
    c.BaseAddress = new Uri("https://httpbin.org/");
    c.Timeout = TimeSpan.FromSeconds(10);
})
.AddPolicyHandler(HttpPolicyExtensions
    .HandleTransientHttpError()
    .WaitAndRetryAsync(3, a => TimeSpan.FromSeconds(Math.Pow(2, a))));


// -------- Controllers / JSON / Validation --------
builder.Services
    .AddControllers()
    .AddJsonOptions(o =>
    {
        o.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
    });

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(
    typeof(EmailCampaign.Application.Campaigns.Validators.CreateCampaignDtoValidator).Assembly);

// -------- Swagger --------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.EnableAnnotations();
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EmailCampaign API",
        Version = "v1",
        Description = "Basit kampanya CRUD + RabbitMQ kuyruða gönderim + istatistikler.",
        Contact = new OpenApiContact { Name = "Sema Nur Þakar", Email = "nur.sakar@euromsg.com" },
        License = new OpenApiLicense { Name = "MIT" }
    });

    var xml = Path.Combine(AppContext.BaseDirectory, "EmailCampaign.Api.xml");
    if (File.Exists(xml)) c.IncludeXmlComments(xml, true);
    var appXml = Path.Combine(AppContext.BaseDirectory, "EmailCampaign.Application.xml");
    if (File.Exists(appXml)) c.IncludeXmlComments(appXml);

    c.ExampleFilters();
    c.CustomSchemaIds(t => t.FullName);
    c.CustomOperationIds(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
});
builder.Services.AddSwaggerExamplesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

// -------- AutoMapper --------
builder.Services.AddAutoMapper(typeof(CampaignProfile));

// -------- Options binding + validation --------
builder.Services
    .AddOptions<DatabaseOptions>()
    .Bind(builder.Configuration.GetSection(DatabaseOptions.SectionName))
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<DatabaseOptions>, DatabaseOptionsValidator>();

builder.Services
    .AddOptions<RabbitMqOptions>()
    .Bind(builder.Configuration.GetSection(RabbitMqOptions.SectionName))
    .ValidateOnStart();
builder.Services.AddSingleton<IValidateOptions<RabbitMqOptions>, RabbitMqOptionsValidator>();

// -------- EF Core --------
builder.Services.AddDbContext<AppDbContext>((sp, opt) =>
{
    var db = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    opt.UseSqlServer(db.Default);
});
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());

// -------- DI --------
builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(EfRepositoryBase<,>));
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<IStatisticsService, StatisticsService>();
builder.Services.AddScoped<ICampaignSendService, CampaignSendService>();
builder.Services.AddScoped<IEventPublisher, MassTransitPublisher>();
builder.Services.AddScoped<ICommandHandler<StartSendCampaignCommand, EnqueueResult>,
                          StartSendCampaignCommandHandler>();

// (Opsiyonel) Application servislerini ekle
builder.Services.AddScoped<AnyService>();

// -------- MassTransit (Publisher) --------
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((ctx, cfg) =>
    {
        var mq = ctx.GetRequiredService<IOptions<RabbitMqOptions>>().Value;
        cfg.Host(mq.Host, mq.VirtualHost, h =>
        {
            h.Username(mq.Username);
            h.Password(mq.Password);
        });
    });
});

var app = builder.Build();

// -------- Pipeline --------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.DocumentTitle = "EmailCampaign API Docs";
        o.DisplayRequestDuration();
        o.DefaultModelsExpandDepth(-1);
        o.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    });
}

// Global hatalar + timing
app.UseMiddleware<GlobalExceptionMiddleware>();
app.UseMiddleware<RequestTimingMiddleware>();

// Prometheus metrikleri
app.UseHttpMetrics();
app.UseMetricServer();

// Rate limiting
app.UseRateLimiter();

app.UseHttpsRedirection();
app.MapControllers();
app.Run();
