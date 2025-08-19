# 📧 EmailCampaign Project

## 📌 Project Purpose
This project is a sample application built to demonstrate a simple email campaign delivery system.
The goals are:
- Manage campaign CRUD operations via API
- Add campaigns to a RabbitMQ queue for processing
- Consume the queue with a background Worker Service to simulate sending
- Provide campaign delivery statistics via API
  
---

## 🏗️ Architecture & Layers
The project follows **Clean Architecture** principles.

### Layers:
- **EmailCampaign.Domain**  
  - Core entity classes (Campaign, CampaignStatus, etc.)
- **EmailCampaign.Application**  
  - DTOs, service interfaces, and service implementations
  - Mapping profiles (AutoMapper)
  - Validations (FluentValidation)
  - Options classes (Options Pattern)
- **EmailCampaign.Infrastructure**  
  - EF Core AppDbContext
  - Repository implementations
- **EmailCampaign.Api**  
  - REST API controllers
  - Swagger documentation
- **EmailCampaign.Worker**  
  - MassTransit Consumers
  - Listens to RabbitMQ queue and updates campaigns to "Sent" state

---

## 🛠️ Technologies Used
- **.NET 8** – API & Worker Service
- **Entity Framework Core** – MSSQL database access
- **RabbitMQ** – Message queue system
- **MassTransit** – RabbitMQ client library
- **FluentValidation** – DTO validations
- **AutoMapper** – DTO ↔ Entity mapping
- **Swashbuckle** – Swagger documentation
- **Options Pattern** – Configuration management
- 📄 [Dependency Injection (DI) Documentation](doc/DI.md)

---

## ⚙️ Setup & Run

### Prerequisites
Make sure you have the following installed on your machine:
- .NET 8 SDK
- SQL Server or LocalDB
- RabbitMQ (either via Docker or CloudAMQP)
- Git

---

If you want to run RabbitMQ locally with Docker:
```bash
docker run -d --hostname rabbitmq \
  --name email-rabbit \
  -p 5672:5672 -p 15672:15672 \
  rabbitmq:3-management
```
This will expose RabbitMQ Management UI at: http://localhost:15672

(Default user/pass: guest / guest)

### Clone the Repository
```bash
git clone https://github.com/your-username/EmailCampaign.git
cd EmailCampaign
### Konfigürasyon
```

### Configure Settings
Both the API and Worker projects require proper configuration.

API (EmailCampaign.Api/appsettings.json)
Update your SQL Server connection string:
```bash
"ConnectionStrings": {
  "SqlServer": "Server=localhost;Database=EmailCampaignDb;User Id=sa;Password=your_password;"
}
```

Configure RabbitMQ connection:
```bash
"RabbitMq": {
  "Host": "localhost",
  "VirtualHost": "/",
  "Username": "guest",
  "Password": "guest"
}
```
Worker (EmailCampaign.Worker/appsettings.json)
Must use the same settings as API for ConnectionStrings and RabbitMq.

Note:
If using CloudAMQP, extract Host, VirtualHost, Username, and Password from the provided URL.

### Database Migration
Run the following commands from the project root (with API set as the startup project):
```bash
dotnet ef migrations add InitialCreate \
  --project EmailCampaign.Infrastructure \
  --startup-project EmailCampaign.Api

dotnet ef database update \
  --project EmailCampaign.Infrastructure \
  --startup-project EmailCampaign.Api
```
This will create and apply the initial database schema.

### Run the Application
Start both the API and Worker service:
API
```bash
cd EmailCampaign.Api
dotnet run
```
Swagger will be available at:
👉 http://localhost:5000/swagger

Worker
```bash
cd EmailCampaign.Worker
dotnet run
```
### Summary
Now you should have:
- API running at http://localhost:5000
- RabbitMQ running locally or via CloudAMQP
- Worker service consuming the RabbitMQ queue
- Database created and ready with migrations applied

You can now create campaigns via the API, which will be processed and marked as Sent by the Worker.
