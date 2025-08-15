# Dependency Injection (DI) – EmailCampaign

Bu doküman **EmailCampaign** projesinde kullanılan Dependency Injection yapısını ve temel mantığını açıklar.

---

## 1) Amaç
- Katmanlar arası **gevşek bağlılık** sağlamak
- Servisleri **arayüz** üzerinden enjekte ederek test edilebilirlik kazandırmak
- Konfigürasyonları **Options Pattern** ile yönetmek
- Uygulamanın **bakımını ve genişletilmesini** kolaylaştırmak

---

## 2) Katmanlar

- **Domain:** Entity ve enum’lar  
- **Application:** Servis arayüzleri, iş kuralları, DTO’lar  
- **Infrastructure:** EF Core, repository, RabbitMQ publisher implementasyonları  
- **API/Worker:** Host uygulamalar, sadece Application arayüzlerini kullanır

---


## 3) API’de DI Kayıt Örnekleri

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

## 4) Worker’da DI Kayıt Örnekleri

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


## 5) DI’nin Faydaları
- **Test edilebilirlik:** Mock/fake servisler ile birim testi kolaydır.
- **Bakım kolaylığı:** Implementasyon değişse bile arayüz sabit kalır.
- **Genişletilebilirlik:** Yeni servis eklemek için sadece arayüz ve implementasyonu yazıp DI’a eklemek yeterlidir.
- **Katman bağımsızlığı:** API ve Worker, Infrastructure implementasyonunu bilmez.

---

