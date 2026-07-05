# Smart Facility Maintenance Management System

Web application for reporting and managing campus facility maintenance issues.
PRG3204E Web Application Development — Group 5.

- **Backend:** ASP.NET Core 8 (MVC, C#)
- **Auth:** ASP.NET Core Identity with role-based access control
- **Database:** Microsoft SQL Server (Entity Framework Core, code-first)
- **Frontend:** Razor views + Bootstrap 5

## Features

- Maintenance request management — submit, upload attachments, assign, close
- Request tracking and history
- Request modification and activity logs
- Reports and analytics
- In-app notifications (no external email needed)
- Accounts with four roles: Admin, Staff, Maintenance, Student

## Prerequisites (Windows)

The easiest setup is **Visual Studio 2022**, which bundles everything you need.

1. Download **Visual Studio 2022 Community** (free): <https://visualstudio.microsoft.com/>
2. In the installer, tick the **"ASP.NET and web development"** workload. This installs:
   - .NET 8 SDK
   - SQL Server Express **LocalDB** (the database — no separate install needed)
   - Git tooling
3. That's it — there is no separate database server to set up.

> Prefer VS Code / command line? Install the [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
> and SQL Server Express or LocalDB yourself.

## Getting started (Visual Studio)

1. **Clone the repo**
   - In Visual Studio: *Git → Clone Repository…* and paste
     `https://github.com/LOOIZIJIAN/smart-facility-maintenance.git`
   - Or from a terminal: `git clone https://github.com/LOOIZIJIAN/smart-facility-maintenance.git`
2. **Open** `SmartFacilityMaintenance.sln`.
3. **Press F5** (or the green ▶ *SmartFacilityMaintenance* button).
   - NuGet packages restore automatically.
   - The database is created and seeded automatically (see the next section).
   - Your browser opens to the running app.
4. **Log in** with a demo account (listed below).

There are **no manual database steps** — no `Update-Database`, no SQL scripts to run.

## Database & seeding — is it automatic? Yes

On every startup, `Program.cs` runs `DbSeeder.SeedAsync`, which:

1. **Creates the database and applies migrations.** On first run it creates
   `SmartFacilityMaintenanceDb` in LocalDB with all the tables.
2. **Seeds starter data if it isn't already present:**
   - 4 roles — Admin, Staff, Maintenance, Student
   - 4 demo accounts (one per role)
   - 5 buildings, 6 categories
   - 5 sample maintenance requests + activity logs

The seeder is **idempotent** — it only inserts what's missing, so running the app
repeatedly never duplicates or overwrites data you've added yourself.

**To reset back to fresh demo data:** in Visual Studio open *View → SQL Server Object
Explorer*, expand `(localdb)\MSSQLLocalDB → Databases`, right-click
**SmartFacilityMaintenanceDb → Delete** (tick "Close existing connections"), then run
the app again — it rebuilds and reseeds.

## Demo accounts

| Role | Email | Password |
|------|-------|----------|
| Administrator | admin@campus.edu | Admin@123 |
| Staff | staff@campus.edu | Staff@123 |
| Maintenance | tech@campus.edu | Tech@123 |
| Student | student@campus.edu | Student@123 |

## Database connection

The connection string is in `SmartFacilityMaintenance/appsettings.json`:

```
Server=(localdb)\mssqllocaldb;Database=SmartFacilityMaintenanceDb;Trusted_Connection=True;MultipleActiveResultSets=true
```

- **LocalDB** (default) works out of the box with Visual Studio — leave it as is.
- Using **SQL Server Express** instead? Change `(localdb)\mssqllocaldb` to `.\SQLEXPRESS`.

## Run from the command line (optional)

```
cd SmartFacilityMaintenance
dotnet run
```

Then open the `https://localhost:...` URL it prints.

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
| Harenthran Chandrasegar | Reports & Analytics + notifications | `Controllers/ReportsController.cs`, `Views/Reports/` |
| Looi Zi Jian | Base project, authentication, database | already done |

## Team workflow

1. Pull the latest `dev`, then create your own branch: `feature/<your-module>`
   (in Visual Studio: *Git → New Branch…* based on `dev`).
2. Build only inside your own controller and `Views/<Module>` folder.
3. Commit and push your branch (*Git → Commit All* then *Push*), then open a pull
   request into `dev` on GitHub — **not** `main`.
4. Tell the team before changing the database or models, so EF migrations don't conflict.
5. `main` is the stable branch — we merge `dev` into it for releases and the final submission.

## Troubleshooting

- **"A network-related or instance-specific error… (localdb)"** — LocalDB isn't running.
  Open a terminal and run `sqllocaldb start mssqllocaldb`, or repair the *ASP.NET and web
  development* workload in the Visual Studio Installer.
- **Build errors about missing packages** — right-click the solution → *Restore NuGet
  Packages*, or run `dotnet restore`.
- **Want a clean database** — delete `SmartFacilityMaintenanceDb` (see the reset steps
  above) and run again.
- **Port already in use** — close the other running instance, or change the port in
  `SmartFacilityMaintenance/Properties/launchSettings.json` (Visual Studio creates this
  on first run).
