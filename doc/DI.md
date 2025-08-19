# Dependency Injection (DI) – EmailCampaign

This document explains the Dependency Injection structure and the main logic used in the EmailCampaign project.

---

## 1) Purpose
- Ensure loose coupling between layers
- Inject services through interfaces to improve testability
- Manage configurations with the Options Pattern
- Make the application easier to maintain and extend

---

## 2) Layers

- **Domain:** Entities and enums
- **Application:** Service interfaces, business rules, DTOs
- **Infrastructure:** EF Core, repositories, RabbitMQ publisher implementations
- **API/Worker:** Host applications, use only Application interfaces

---


## 3) DI Registration Examples in API

```csharp
// Repository
builder.Services.AddScoped(typeof(IGenericRepository<,>), typeof(EfRepositoryBase<,>));

// Servisler
builder.Services.AddScoped<ICampaignService, CampaignService>();
builder.Services.AddScoped<ICampaignSendService, CampaignSendService>();
builder.Services.AddScoped<IStatsService, StatsService>();

// Event Publisher
builder.Services.AddScoped<IEventPublisher, MassTransitPublisher>();

// DbContext
builder.Services.AddDbContext<AppDbContext>(opt =>
    opt.UseSqlServer(builder.Configuration.GetConnectionString("Default")));
builder.Services.AddScoped<DbContext>(sp => sp.GetRequiredService<AppDbContext>());
```

---

## 4) DI Registration Examples in Worker

```csharp
// Repository
services.AddScoped(typeof(IGenericRepository<,>), typeof(EfRepositoryBase<,>));

// Servisler
services.AddScoped<ICampaignSendService, CampaignSendService>();

// Consumer
services.AddMassTransit(x =>
{
    x.AddConsumer<SendEmailCommandConsumer>();
    x.UsingRabbitMq((context, cfg) =>
    {
        cfg.ReceiveEndpoint("send-email-queue", e =>
        {
            e.ConfigureConsumer<SendEmailCommandConsumer>(context);
        });
    });
});
```

---


## 5) Benefits of DI
- **Testability:** Unit tests are easier with mock/fake services
- **Maintainability:** Interfaces remain the same even if implementations change
- **Extensibility:** To add a new service, just create the interface and implementation, then register it in DI
- **Layer independence:** API and Worker do not need to know Infrastructure implementations

---

