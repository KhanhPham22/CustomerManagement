# Customer Management System

A simple Customer Management application built with .NET 8, Blazor, Entity Framework Core, and SQL Server.

## Features

- Admin login with JWT authentication
- Customer CRUD
- Search customers by name or phone number
- Pagination
- Input validation
- Duplicate validation for Customer Code, Phone Number, and Email
- Optional Date of Birth
- Swagger API documentation

## Tech Stack

- .NET 8
- ASP.NET Core Web API
- Blazor Server
- Entity Framework Core
- SQL Server
- JWT Authentication
- Swagger / OpenAPI

## Project Structure

```text
CustomerManagement
├── CustomerManagement.Api
│   ├── Controllers
│   ├── Data
│   ├── DTOs
│   ├── Models
│   └── Services
│
└── CustomerManagement.Blazor
    ├── Components
    ├── Models
    └── Services
```
## Architecture
```text
Blazor UI
    ↓ HTTP
CustomersController
    ↓
CustomerService
    ↓
Entity Framework Core
    ↓
SQL Server
```
## Getting Started
### 1. Prerequisites
.NET 8 SDK
SQL Server
Visual Studio 2022

### 2. Configure the database

Update the connection string in:

CustomerManagement.Api/appsettings.json

Example:

"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=CustomerManagementDb;Trusted_Connection=True;TrustServerCertificate=True;"
}

### 3. Apply database migration

Open Visual Studio Package Manager Console and run:

Update-Database

### 4. Run the application

Start both:

CustomerManagement.Api
CustomerManagement.Blazor

The API provides Swagger for testing the endpoints.

## Default Login
Username: admin
Password: Admin@123

This account is created automatically by the database seeder for demonstration purposes.

## API Endpoints
| Method | Endpoint | Description |
|---|---|---|
| POST | `/api/auth/login` | Admin login |
| GET | `/api/customers` | Get customers |
| GET | `/api/customers/{id}` | Get customer by ID |
| POST | `/api/customers` | Create customer |
| PUT | `/api/customers/{id}` | Update customer |
| DELETE | `/api/customers/{id}` | Delete customer |

Customer endpoints require JWT authentication.

## Validation
Customer Code: required, maximum 20 characters, unique
Full Name: required, maximum 100 characters
Email: optional, valid email format, unique when provided
Phone Number: required, valid Vietnamese mobile number, unique
Date of Birth: optional, cannot be in the future
Is Active: customer status
Notes

This project is intentionally kept simple for demonstration purposes. It focuses on the core customer management flow, validation, authentication, and API integration without introducing unnecessary architectural complexity.
