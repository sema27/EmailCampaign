using EmailCampaign.Application.Campaigns.Commands.StartSendCampaign;
using EmailCampaign.Application.Campaigns.Enums;
using EmailCampaign.Application.Campaigns.Handlers.StartSendCampaign;
using EmailCampaign.Application.Campaigns.Mapping;
using EmailCampaign.Application.Campaigns.Services;
using EmailCampaign.Application.Common.Abstractions;
using EmailCampaign.Application.Common.Options;
using EmailCampaign.Application.Common.Options.Validation;
using EmailCampaign.Domain.Repositories;
using EmailCampaign.Infrastructure.Persistance;
using FluentValidation;
using FluentValidation.AspNetCore;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.Filters;

var builder = WebApplication.CreateBuilder(args);

// ---- Controllers / Swagger / Validation ----
builder.Services.AddControllers()
   .AddJsonOptions(o => o.JsonSerializerOptions.PropertyNamingPolicy = null);
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(typeof(EmailCampaign.Application.Campaigns.Validators.CreateCampaignDtoValidator).Assembly);
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

    // XML yorumlarý (Controller + DTO)
    var xml = Path.Combine(AppContext.BaseDirectory, "EmailCampaign.Api.xml");
    if (File.Exists(xml)) c.IncludeXmlComments(xml, true);
    var appXml = Path.Combine(AppContext.BaseDirectory, "EmailCampaign.Application.xml");
    if (File.Exists(appXml)) c.IncludeXmlComments(appXml);

    // Örnekler ve schema id çakýþmalarýný önle
    c.ExampleFilters();
    c.CustomSchemaIds(t => t.FullName);
    c.CustomOperationIds(apiDesc => $"{apiDesc.ActionDescriptor.RouteValues["controller"]}_{apiDesc.HttpMethod}");
});

builder.Services.AddSwaggerExamplesFromAssemblies(AppDomain.CurrentDomain.GetAssemblies());

// ---- AutoMapper ----
builder.Services.AddAutoMapper(typeof(CampaignProfile));

// ---- Options binding + validation ----
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

// ---- EF Core ----
builder.Services.AddDbContext<AppDbContext>((sp, opt) =>
{
    var db = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
    opt.UseSqlServer(db.Default);
});
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());

// ---- DI ----
builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(GenericRepository<,>));
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<IEventPublisher, MassTransitPublisher>();
builder.Services.AddScoped<ICommandHandler<StartSendCampaignCommand, EnqueueResult>,
                          StartSendCampaignCommandHandler>();

// ---- MassTransit (Publisher) ----
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

// ---- Pipeline ----
if (app.Environment.IsDevelopment())
{
    // Swagger UI (görsel ayar)
    app.UseSwagger();
    app.UseSwaggerUI(o =>
    {
        o.DocumentTitle = "EmailCampaign API Docs";
        o.DisplayRequestDuration();
        o.DefaultModelsExpandDepth(-1);
        o.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.List);
    });
}
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
