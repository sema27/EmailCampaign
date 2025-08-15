using EmailCampaign.Application.Common.Options;
using EmailCampaign.Application.Common.Options.Validation;
using EmailCampaign.Infrastructure.Persistance;
using EmailCampaign.Worker.Consumers;
using MassTransit;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

var builder = Host.CreateDefaultBuilder(args)
    .ConfigureServices((ctx, services) =>
    {
        // Options
        services.AddOptions<RabbitMqOptions>()
            .Bind(ctx.Configuration.GetSection(RabbitMqOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<RabbitMqOptions>, RabbitMqOptionsValidator>();

        services.AddOptions<DatabaseOptions>()
            .Bind(ctx.Configuration.GetSection(DatabaseOptions.SectionName))
            .ValidateOnStart();
        services.AddSingleton<IValidateOptions<DatabaseOptions>, DatabaseOptionsValidator>();

        // DbContext
        services.AddDbContext<AppDbContext>((sp, opt) =>
        {
            var db = sp.GetRequiredService<IOptions<DatabaseOptions>>().Value;
            opt.UseSqlServer(db.Default);
        });

        // MassTransit + Consumer
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
