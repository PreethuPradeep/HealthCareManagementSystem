# Healthcare Management System API

A comprehensive RESTful API for managing healthcare operations including patient management, appointments, consultations, prescriptions, pharmacy operations, and billing. Built with ASP.NET Core 8.0 and Entity Framework Core.

## 📋 Table of Contents

- [Overview](#overview)
- [Features](#features)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Installation](#installation)
- [Configuration](#configuration)
- [API Documentation](#api-documentation)
- [Project Structure](#project-structure)
- [Authentication & Authorization](#authentication--authorization)
- [Database](#database)
- [Development](#development)

## 🏥 Overview

The Healthcare Management System API is a robust backend solution designed to streamline healthcare operations across multiple departments. It provides secure, role-based access control for different user types including administrators, doctors, receptionists, pharmacists, and lab technicians.

## ✨ Features

### 🔐 Admin Module
- User management (staff creation, updates, deactivation)
- Role management
- Specialization management
- System configuration

### 👨‍⚕️ Doctor Module
- Patient consultations with SOAP notes
- Prescription management
- Lab test requests
- Patient history tracking
- Follow-up scheduling

### 📋 Receptionist Module
- Patient registration and management
- Appointment scheduling
- Billing and payment processing
- Lab test request management

### 💊 Pharmacist Module
- Medicine inventory management
- Stock transactions tracking
- Prescription fulfillment
- Pharmacy billing
- Prescription item management

### 🔬 Lab Technician Module
- Lab test request management
- Test result tracking

## 🛠 Technology Stack

- **Framework**: ASP.NET Core 8.0
- **Database**: SQL Server (LocalDB/Express)
- **ORM**: Entity Framework Core 8.0
- **Authentication**: ASP.NET Core Identity + JWT Bearer Tokens
- **API Documentation**: Swagger/OpenAPI
- **PDF Generation**: QuestPDF
- **Language**: C# (.NET 8.0)

## 📦 Prerequisites

Before you begin, ensure you have the following installed:

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (LocalDB, Express, or full version)
- [Visual Studio 2022](https://visualstudio.microsoft.com/) or [Visual Studio Code](https://code.visualstudio.com/)
- Git (for cloning the repository)

## 🚀 Installation

### 1. Clone the Repository

```bash
git clone <repository-url>
cd HealthCareManagementSystem
```

### 2. Restore Dependencies

```bash
cd HealthCareManagementSystem
dotnet restore
```

### 3. Update Database Connection String

Edit `appsettings.json` or `appsettings.Development.json` and update the connection string:

```json
{
  "ConnectionStrings": {
    "default": "Server=(localdb)\\MSSQLLocalDB;Database=HealthCareDB;Trusted_Connection=True;TrustServerCertificate=True"
  }
}
```

### 4. Apply Database Migrations

```bash
dotnet ef database update --project HealthCareManagementSystem
```

### 5. Run the Application

```bash
dotnet run --project HealthCareManagementSystem
```

The API will be available at:
- **HTTP**: `http://localhost:5000`
- **HTTPS**: `https://localhost:5001`
- **Swagger UI**: `https://localhost:5001/swagger`

## ⚙️ Configuration

### JWT Configuration

Update the JWT settings in `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "your-256-bit-secret-key-here-change-in-production",
    "Issuer": "HealthCareSystem",
    "Audience": "HealthCareUsers",
    "DurationInMinutes": 60
  }
}
```

**⚠️ Important**: Change the JWT Key to a secure random string in production environments.

### CORS Configuration

The API is configured to accept requests from `http://localhost:4200` (Angular frontend). To modify this, update the CORS policy in `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("MyPolicy", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
```

## 📚 API Documentation

### Base URL
```
https://localhost:5001/api
```

### Main Endpoints

#### Authentication
- `POST /api/auth/login` - User login

#### Admin Endpoints
- `GET /api/users` - Get all active staff
- `POST /api/users` - Create new staff member
- `PUT /api/users/{id}` - Update staff member
- `DELETE /api/users/{id}` - Deactivate staff member
- `GET /api/roles` - Get all roles
- `POST /api/roles` - Create role
- `PUT /api/roles/{id}` - Update role
- `DELETE /api/roles/{id}` - Deactivate role
- `GET /api/specializations` - Get all specializations
- `POST /api/specializations` - Create specialization
- `PUT /api/specializations/{id}` - Update specialization
- `DELETE /api/specializations/{id}` - Deactivate specialization

#### Receptionist Endpoints
- `GET /api/patients` - Get all patients
- `POST /api/patients` - Register new patient
- `PUT /api/patients/{id}` - Update patient
- `GET /api/appointments` - Get all appointments
- `POST /api/appointments` - Create appointment
- `PUT /api/appointments/{id}` - Update appointment
- `GET /api/billings` - Get all bills
- `POST /api/billings` - Create bill

#### Doctor Endpoints
- `GET /api/consultations` - Get consultations
- `POST /api/consultations` - Create consultation
- `GET /api/prescriptions` - Get prescriptions
- `POST /api/prescriptions` - Create prescription

#### Pharmacist Endpoints
- `GET /api/medicines` - Get all medicines
- `POST /api/medicines` - Add medicine
- `PUT /api/medicines/{id}` - Update medicine
- `GET /api/prescriptions` - Get prescriptions
- `POST /api/pharmacy/bills` - Create pharmacy bill

#### Lab Technician Endpoints
- `GET /api/labtests` - Get lab test requests
- `PUT /api/labtests/{id}` - Update lab test status

### Interactive API Documentation

Swagger UI is available at `/swagger` when running in Development mode. It provides:
- Complete API endpoint documentation
- Interactive testing interface
- Request/response schemas
- Authentication testing

## 🔒 Authentication & Authorization

The API uses JWT Bearer Token authentication with role-based authorization.

### Authentication Flow

1. **Login**: Send credentials to `/api/auth/login`
2. **Receive Token**: API returns a JWT token
3. **Include Token**: Add token to subsequent requests in the Authorization header:
   ```
   Authorization: Bearer <your-token>
   ```

### Roles

- **Admin**: Full system access
- **Doctor**: Consultation, prescription, and patient history management
- **Receptionist**: Patient registration, appointments, and billing
- **Pharmacist**: Medicine inventory and prescription fulfillment
- **Lab Technician**: Lab test management

### Authorization Example

```csharp
[Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "Admin")]
```

## 🗄️ Database

### Database Schema

The system uses Entity Framework Core with Code-First migrations. Key entities include:

- **ApplicationUser**: Staff members (extends IdentityUser)
- **Patient**: Patient information
- **Appointment**: Appointment scheduling
- **Consultation**: Doctor consultations with SOAP notes
- **Prescription**: Prescription records
- **Medicine**: Medicine inventory
- **PharmacyBill**: Pharmacy billing
- **Billing**: General billing
- **LabTestRequest**: Lab test requests
- **Role**: Custom roles
- **Specialization**: Doctor specializations

### Database Migrations

Create a new migration:
```bash
dotnet ef migrations add MigrationName --project HealthCareManagementSystem
```

Apply migrations:
```bash
dotnet ef database update --project HealthCareManagementSystem
```

## 📁 Project Structure

```
HealthCareManagementSystem/
├── Controllers/          # API Controllers
│   ├── AuthController.cs
│   ├── UsersController.cs
│   ├── RolesController.cs
│   ├── PatientsController.cs
│   ├── AppointmentsController.cs
│   ├── ConsultationController.cs
│   ├── PrescriptionsController.cs
│   ├── MedicinesController.cs
│   └── ...
├── Models/              # Domain Models and DTOs
│   ├── ApplicationUser.cs
│   ├── Patient.cs
│   ├── Consultation.cs
│   ├── DTOs/
│   └── Pharm/
├── Repository/          # Data Access Layer
│   ├── IUserRepository.cs
│   ├── UserSqlServerRepositoryImpl.cs
│   └── ...
├── Services/            # Business Logic Services
│   ├── JwtTokenHelper.cs
│   └── PdfService.cs
├── Database/            # Database Context
│   └── HealthCareDbContext.cs
├── Migrations/          # EF Core Migrations
├── Program.cs           # Application Entry Point
└── appsettings.json     # Configuration
```

## 💻 Development

### Running in Development Mode

```bash
dotnet run --project HealthCareManagementSystem
```

### Building for Production

```bash
dotnet build --configuration Release --project HealthCareManagementSystem
```

### Running Tests

```bash
dotnet test
```

### Code Style

The project follows C# coding conventions and best practices:
- Repository pattern for data access
- Dependency Injection for loose coupling
- DTOs for data transfer
- Async/await for asynchronous operations

## 🔐 Security Considerations

- **JWT Tokens**: Use strong, randomly generated keys in production
- **Password Policy**: Configured via Identity options (minimum 6 characters)
- **CORS**: Configure appropriate origins for production
- **HTTPS**: Always use HTTPS in production environments
- **SQL Injection**: Protected by Entity Framework Core parameterized queries
- **Authentication**: All endpoints require proper authentication tokens

## 📝 Default User Accounts

The system seeds default user accounts on startup. Default passwords are set in `Program.cs`:

- `sreejith.menon@hospital.com` - Doctor@123
- `anjali.jose@hospital.com` - Doctor@234
- `meera.pillai@hospital.com` - Recep@123
- `jithin.mathew@hospital.com` - Pharma@123
- `sneha.tech@hospital.com` - Lab@123

**⚠️ Change these passwords immediately in production!**

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/AmazingFeature`)
3. Commit your changes (`git commit -m 'Add some AmazingFeature'`)
4. Push to the branch (`git push origin feature/AmazingFeature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 👥 Authors

- Healthcare Management System Development Team

## 🙏 Acknowledgments

- ASP.NET Core team for the excellent framework
- Entity Framework Core for robust ORM capabilities
- QuestPDF for PDF generation capabilities

---

**Note**: This is a development version. Ensure all security configurations are properly set before deploying to production.
