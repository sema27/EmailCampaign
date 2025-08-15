# 📧 EmailCampaign Project

## 📌 Proje Amacı
Bu proje, basit bir e-posta gönderim sistemi kurmak için geliştirilmiş bir örnek uygulamadır.
Amacı:
- Kampanya CRUD işlemlerini API üzerinden yapmak
- Kampanyayı RabbitMQ kuyruğuna ekleyerek gönderime hazırlamak
- Arka planda çalışan Worker Service ile kuyruğu tüketmek ve gönderim simülasyonu yapmak
- Gönderim istatistiklerini API üzerinden almak
  
---

## 🏗️ Mimari & Katmanlar
Proje **Clean Architecture** prensipleri ile tasarlanmıştır.

### Katmanlar:
- **EmailCampaign.Domain**  
  - Temel entity sınıfları (`Campaign`, `CampaignStatus` vb.)
  - İş kuralları ve domain modelleri
- **EmailCampaign.Application**  
  - DTO'lar, servis arayüzleri, servis implementasyonları
  - Mapping profilleri (AutoMapper)
  - Validasyonlar (FluentValidation)
  - Options sınıfları (Options Pattern)
- **EmailCampaign.Infrastructure**  
  - EF Core DbContext (`AppDbContext`)
  - Repository implementasyonları
- **EmailCampaign.Api**  
  - REST API Controller’ları
  - Swagger dokümantasyonu
- **EmailCampaign.Worker**  
  - MassTransit Consumer’ları
  - RabbitMQ kuyruğunu dinleyerek kampanyaları "Sent" durumuna çeken servis

---

## 🛠️ Kullanılan Teknolojiler
- **.NET 8** – API ve Worker Service
- **Entity Framework Core** – MSSQL veri tabanı erişimi
- **RabbitMQ** – Mesaj kuyruğu sistemi
- **MassTransit** – RabbitMQ client kütüphanesi
- **FluentValidation** – DTO validasyonları
- **AutoMapper** – DTO ↔ Entity dönüşümleri
- **Swashbuckle** – Swagger dokümantasyonu
- **Options Pattern** – Konfigürasyon yönetimi
- 📄 [Dependency Injection (DI) Dokümanı](doc/DI.md)

---

## ⚙️ Kurulum & Çalıştırma

### Önkoşullar
- .NET 8 SDK
- SQL Server / LocalDB
- RabbitMQ (lokalde Docker ile veya CloudAMQP)

---

### Konfigürasyon

**API (`appsettings.json`)**
- `ConnectionStrings.SqlServer` değerini kendi SQL Server bağlantınıza göre düzenleyin.
- `RabbitMq` bölümünü lokal veya CloudAMQP bilgilerine göre doldurun.

**Worker (`appsettings.json`)**
- API ile **aynı** `ConnectionStrings` ve `RabbitMq` değerlerini kullanmalı.

> **Not:** CloudAMQP kullanıyorsanız, `Host`, `VirtualHost`, `Username`, `Password` alanlarını verilen URL’den ayıklayın.

---

### Veritabanı – Migration & Update

İlk kurulumda veya model değiştiğinde:

```bash
# Proje kökünde, API startup olarak seçiliyken:
dotnet ef migrations add InitialCreate --project EmailCampaign.Infrastructure --startup-project EmailCampaign.Api
dotnet ef database update --project EmailCampaign.Infrastructure --startup-project EmailCampaign.Api
