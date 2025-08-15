using EmailCampaign.Application.Campaigns.Services;            // ICampaignSendService, CampaignSendService
using EmailCampaign.Application.Common.Options;                // DatabaseOptions, RabbitMqOptions
using EmailCampaign.Application.Common.Options.Validation;     // Validatorlar
using EmailCampaign.Application.Common.Repositories;           // IGenericRepository
using EmailCampaign.Infrastructure.Persistence;                // AppDbContext  (sen klas�r� 'Persistence' yapt�n)
using EmailCampaign.Infrastructure.Persistence.Repositories;   // EfRepositoryBase
using EmailCampaign.Worker.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        // --- Options (RabbitMQ + Database) ---
        services.AddOptions<RabbitMqOptions>()
            .Bind(ctx.Configuration.GetSection(RabbitMqOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<RabbitMqOptions>, RabbitMqOptionsValidator>();

        services.AddOptions<DatabaseOptions>()
            .Bind(ctx.Configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<DatabaseOptions>, DatabaseOptionsValidator>();

        // --- EF Core / DbContext ---
        var conn = ctx.Configuration.GetConnectionString("Default");
        services.AddDbContext<AppDbContext>(opt => opt.UseSqlServer(conn));
        // Repository base'i DbContext �zerinden �al��aca�� i�in DbContext'i de expose edelim
        services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());

        // --- Repository implementasyonu ---
        services.AddScoped(typeof(IGenericRepository<,>), typeof(EfRepositoryBase<,>));

        // --- Application servisleri (Worker'�n ihtiyac�) ---
        services.AddScoped<ICampaignSendService, CampaignSendService>();

        // --- MassTransit / Consumer ---
        services.AddMassTransit(x =>
        {
            x.AddConsumer<SendEmailCommandConsumer>();

            x.UsingRabbitMq((context, cfg) =>
            {
                var mq = context.GetRequiredService<IOptions<RabbitMqOptions>>().Value;

                cfg.Host(mq.Host, mq.VirtualHost, h =>
                {
                    h.Username(mq.Username);
                    h.Password(mq.Password);
                });

                cfg.ReceiveEndpoint("send-email-queue", e =>
                {
                    e.ConfigureConsumer<SendEmailCommandConsumer>(context);
                });
            });
        });
    });

await builder.Build().RunAsync();
