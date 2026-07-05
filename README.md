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

## Module ownership

| Member | Module | Your files |
|--------|--------|-----------|
| Ching Linn Kee | Maintenance Request Management | `Controllers/RequestController.cs`, `Views/Request/` |
| Grace Go Ying Chee | Request Tracking & History | `Controllers/TrackingController.cs`, `Views/Tracking/` |
| Nigel Ng Kai Shuen | Request Modification & Activity Logs | `Controllers/ModificationController.cs`, `Views/Modification/` |
| Harenthran Chandrasegar | Reports & Analytics | `Controllers/ReportsController.cs`, `Views/Reports/` |
| Looi Zi Jian | Base project, authentication, database | already done |

## Team workflow

1. Pull the latest `dev`, then create your own branch: `feature/<your-module>`.
2. Build only inside your own controller and `Views/<Module>` folder.
3. Commit and push your branch, then open a pull request into `dev` (not `main`).
4. Tell the team before changing the database or models, so EF migrations don't conflict.
5. `main` is the stable branch — we merge `dev` into it for releases and the final submission.
