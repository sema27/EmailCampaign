# Dependency Injection (DI) – EmailCampaign

Bu doküman EmailCampaign projesinde kullanılan **Dependency Injection** yapısını anlatır: temel kavramlar, yaşam döngüleri, projedeki kayıtlar ve katmanlar arası bağımlılık şeması.

## 1) Neden DI?
- **Gevşek bağlılık:** Sınıflar implementasyona değil **arayüzlere** bağımlı.
- **Test edilebilirlik:** Mock/fake ile kolay test.
- **Konfigürasyon ve yaşam döngüsü:** Servislerin ömrünü merkezi yönetme.
- **Genişleyebilirlik:** Yeni implementasyonlar eklemek basit (örn. farklı e‑posta sağlayıcısı).

## 2) .NET’te DI kısaca
```csharp
// Kayıt
builder.Services.AddScoped<IMyService, MyService>();

// Kullanım
public sealed class MyController(IMyService myService) : ControllerBase
{
    private readonly IMyService _myService = myService;
}

```

.NET Core, DI desteğini yerleşik olarak sağlar.
AddScoped, AddTransient, AddSingleton ile servis ömürleri belirlenir.

## 3) EmailCampaign Projesinde DI Kullanımı
Projemiz 2 ana uygulamadan oluşuyor:

- **EmailCampaign.Api** → Kampanya CRUD + RabbitMQ kuyruğuna gönderim
- **EmailCampaign.Worker** → Kuyruktan mesaj tüketip e-posta gönderimini simüle etme + veritabanına yazma

Her iki uygulamada da DI şu amaçlarla kullanıldı:

**API Katmanında**
- ICampaignService → CampaignService
- IGenericRepository<TEntity, TKey> → GenericRepository<TEntity, TKey>
- IEventPublisher → MassTransitPublisher
- Options Pattern ile RabbitMqOptions ve DatabaseOptions

**Worker Katmanında**
- IConsumer<SendEmailCommand> → SendEmailCommandConsumer
- EF Core AppDbContext → Kampanya gönderim durumlarını güncellemek için

## 4) Avantajlar
- **Katmanlar Arası Bağımsızlık:** Domain, Application, Infrastructure katmanları birbirinden kopuk çalışabilir.
- **Kolay Test:** Her servis kolayca mocklanabilir.
- **Kolay Konfigürasyon Değişikliği:** Örneğin RabbitMQ yerine başka bir kuyruk sistemi eklemek mümkün.
- **Bakım Kolaylığı:** Tüm bağımlılıkların tek yerde yönetilmesi sayesinde projeye yeni geliştiricilerin adapte olması kolay.

📄 Not:
Bu dokümanın amacı, projedeki DI yapısının anlaşılmasını sağlamaktır.
Kaynak kodun detayları için ilgili katmanların Program.cs dosyalarına bakabilirsiniz.
