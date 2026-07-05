# Smart Facility Maintenance Management System

Web application for reporting and managing campus facility maintenance issues.
PRG3204E Web Application Development — Group 5.

- **Backend:** ASP.NET Core 8 (MVC, C#)
- **Auth:** ASP.NET Core Identity with role-based access control
- **Database:** Microsoft SQL Server (Entity Framework Core)
- **Frontend:** Razor views + Bootstrap 5

## Features

- Maintenance request management — submit, upload attachments, assign, close
- Request tracking and history
- Request modification and activity logs
- Reports and analytics
- Accounts with four roles: Admin, Staff, Maintenance, Student

## Prerequisites

- .NET 8 SDK
- SQL Server Express or LocalDB (LocalDB ships with Visual Studio 2022)
- Visual Studio 2022 or VS Code

## Getting started

1. Clone the repository and open `SmartFacilityMaintenance.sln`.
2. Check the connection string in `SmartFacilityMaintenance/appsettings.json`
   (defaults to LocalDB — no change needed on Windows with Visual Studio).
3. Run the project (press F5, or `dotnet run` from the project folder).

The database is created, migrated, and seeded automatically on first run, so
there is nothing else to configure. Sign in with a demo account below.

## Demo accounts

| Role | Email | Password |
|------|-------|----------|
| Administrator | admin@campus.edu | Admin@123 |
| Staff | staff@campus.edu | Staff@123 |
| Maintenance | tech@campus.edu | Tech@123 |
| Student | student@campus.edu | Student@123 |

## Project structure

```
SmartFacilityMaintenance/
  Controllers/   request, tracking, modification, reports, account, home
  Data/          EF Core context, seeder, migrations
  Models/        entities, enums, view models
  Services/      file upload service
  Views/         Razor views (one folder per controller)
  wwwroot/       css, js, uploads
```

## Team workflow

- Work on a feature branch, e.g. `feature/request-management`.
- Build inside your own module's controller and `Views/<Module>` folder.
- Open a pull request into `dev`; merge to `main` for releases.
- Coordinate database and model changes so migrations don't conflict.
