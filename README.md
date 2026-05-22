# Menaxhimi Shkollor (UM_Project)

ASP.NET Core 10 web application for school management: students, professors, parents, courses, grades, attendance, reports, and admin dashboards. Data is stored in **MySQL** using Entity Framework Core.

---

## Table of contents

1. [Prerequisites](#prerequisites)
2. [Run with Docker (recommended)](#run-with-docker-recommended)
3. [Run locally without Docker](#run-locally-without-docker)
4. [Configuration](#configuration)
5. [Demo accounts](#demo-accounts)
6. [Database behavior on startup](#database-behavior-on-startup)
7. [Email (SMTP)](#email-smtp)
8. [Useful commands](#useful-commands)
9. [Troubleshooting](#troubleshooting)
10. [Project overview](#project-overview)

---

## Prerequisites

### For Docker

| Tool | Version |
|------|---------|
| [Docker Desktop](https://www.docker.com/products/docker-desktop/) (or Docker Engine + Compose) | Recent stable |
| Git | Any |

### For local development (no Docker)

| Tool | Version |
|------|---------|
| [.NET SDK](https://dotnet.microsoft.com/download) | **10.0** (matches `net10.0` in the project) |
| [MySQL Server](https://dev.mysql.com/downloads/mysql/) | **8.x** recommended |
| Git | Any |

Optional: Visual Studio 2022+, VS Code, or Rider.

---

## Run with Docker (recommended)

Docker runs **two containers**: MySQL and the web app. On first start the app applies migrations, bootstraps schema, and seeds demo data automatically.

### 1. Clone and open the project

```powershell
git clone <your-repo-url>
cd UM_Project
```

### 2. Create environment file

Copy the example env file and edit secrets (especially MySQL and JWT):

```powershell
copy .env.example .env
```

Edit `.env` — at minimum set:

- `MYSQL_ROOT_PASSWORD`
- `MYSQL_PASSWORD`
- `JWT_SECRET` (long random string for anything beyond local demo)

**Do not commit `.env`** — it is ignored by `.dockerignore` and should stay local.

### 3. Build and start

```powershell
docker compose up --build
```

First run can take several minutes (NuGet restore, image build, MySQL init, migrations, seed).

### 4. Open the application

| URL | Notes |
|-----|--------|
| http://localhost:8080 | Default (`WEB_PORT` in `.env`) |

Login page: http://localhost:8080/Identity/Account/Login

### 5. Stop / remove

```powershell
# Stop containers (keep database volume)
docker compose down

# Stop and delete database volume (full reset)
docker compose down -v
```

### What Docker starts

| Service | Image / build | Port | Role |
|---------|----------------|------|------|
| `mysql` | `mysql:8.4` | `3306` | Database `UM_ProjectDB` |
| `web` | Built from `Dockerfile` | `8080` (configurable) | ASP.NET Core app |

MySQL data persists in the Docker volume `um_mysql_data`.

### Docker-specific settings

- Environment: `ASPNETCORE_ENVIRONMENT=Docker`
- Extra config: `appsettings.Docker.json` (migrations + seed enabled)
- Connection string is injected via `ConnectionStrings__DefaultConnection` in `docker-compose.yml` (host `mysql`, not `localhost`)

---

## Run locally without Docker

### 1. Install MySQL

Create a database and user (example):

```sql
CREATE DATABASE UM_ProjectDB CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
CREATE USER 'root'@'localhost' IDENTIFIED BY 'your_password';
GRANT ALL PRIVILEGES ON UM_ProjectDB.* TO 'root'@'localhost';
FLUSH PRIVILEGES;
```

### 2. Configure connection string

Edit `appsettings.json` (or `appsettings.Development.json`):

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=localhost;Database=UM_ProjectDB;User=root;Password=your_password;"
}
```

Ensure these flags match your intent (defaults in `appsettings.json` already enable auto-setup):

```json
"Database": {
  "ApplyMigrationsOnStartup": true,
  "ApplySchemaBootstrapOnStartup": true,
  "SeedOnStartup": true,
  "ReseedDemoData": false
}
```

### 3. Restore and run

```powershell
dotnet restore
dotnet run
```

### 4. Open in browser

From `Properties/launchSettings.json`:

| Profile | URLs |
|---------|------|
| `http` | http://localhost:5199 |
| `https` | https://localhost:7059 and http://localhost:5000 |

```powershell
# Explicit profile
dotnet run --launch-profile https
```

---

## Configuration

ASP.NET Core loads settings in this order (later wins):

1. `appsettings.json`
2. `appsettings.{Environment}.json` (e.g. `Development`, `Docker`)
3. Environment variables (use `__` for nested keys, e.g. `Jwt__Secret`)
4. User secrets (development only, if configured)

### Important settings

| Key | Description |
|-----|-------------|
| `ConnectionStrings:DefaultConnection` | MySQL connection |
| `Database:ApplyMigrationsOnStartup` | Run EF migrations on startup |
| `Database:ApplySchemaBootstrapOnStartup` | Apply extra SQL bootstrap |
| `Database:SeedOnStartup` | Seed roles, admin users, demo school data |
| `Database:ReseedDemoData` | **Destructive**: clear and re-seed demo tables |
| `Jwt:Secret` | Signing key for JWT (change in production) |
| `AccountProvisioning:DefaultPassword` | Default password for newly provisioned accounts (`Welcome@123`) |
| `EmailSettings:*` | SMTP for outbound mail (optional in Docker) |

### Security note

`appsettings.json` in the repo may contain placeholder or development values. For production:

- Use strong `Jwt:Secret`
- Use environment variables or a secrets manager — never commit real SMTP passwords
- Set `Database:ReseedDemoData` to `false` unless you intentionally reset demo data

---

## Demo accounts

After seeding (`SeedOnStartup: true`), you can sign in with these accounts.  
**Default password for seeded demo users:** `Admin@123`  
(New accounts created through provisioning use `Welcome@123` from `AccountProvisioning:DefaultPassword`.)

| Role | Email | Password |
|------|-------|----------|
| Super Admin | `superadmin@shkollademo.edu` | `Admin@123` |
| Admin | `admin@shkollademo.edu` | `Admin@123` |
| Professor (example) | `mesues.elena@shkollademo.edu` | `Admin@123` |
| Student (example) | `nx.ana.gashi@shkollademo.edu` | `Admin@123` |
| Parent (example) | `pr.artan.gashi@shkollademo.edu` | `Admin@123` |

More demo users are created under the `@shkollademo.edu` domain (professors `mesues.*`, students `nx.*`, parents `pr.*`).

### Roles in the system

- **SuperAdmin** — full system access  
- **Admin** — school administration panel  
- **Professor** — teaching, grades, courses  
- **Student** — schedules, grades, documents  
- **Parent** — linked child(ren), requests, documents  

---

## Database behavior on startup

On application start (`Program.cs`), the app:

1. Applies EF Core migrations (if `ApplyMigrationsOnStartup` is true)
2. Runs `DatabaseSchemaBootstrap` for supplementary schema (if enabled)
3. Runs `DemoDataSeeder` for roles, system admins, and demo school data (if `SeedOnStartup` is true)
4. Applies grade-scale settings via `ISystemSettingsService`

**First Docker start:** wait until logs show `Menaxhimi Shkollor — database startup step done.` before expecting all features to work.

**Reset demo data only** (keeps MySQL volume, clears/rebuilds demo tables):

```json
"Database": { "ReseedDemoData": true }
```

Or set environment variable: `Database__ReseedDemoData=true` (use once, then turn off).

**Full reset in Docker:**

```powershell
docker compose down -v
docker compose up --build
```

---

## Email (SMTP)

Email is used for notifications (MailKit). Configure in `appsettings.json` or environment:

| Variable | Maps to |
|----------|---------|
| `SMTP_SERVER` | `EmailSettings:SmtpServer` |
| `SMTP_PORT` | `EmailSettings:SmtpPort` |
| `SMTP_SENDER_EMAIL` | `EmailSettings:SenderEmail` |
| `SMTP_SENDER_PASSWORD` | `EmailSettings:SenderPassword` |

In Docker, leave SMTP variables empty in `.env` if you do not need outbound mail locally.

For Gmail, use an [App Password](https://support.google.com/accounts/answer/185833), not your main account password.

---

## Useful commands

### Docker

```powershell
# Rebuild after code changes
docker compose up --build

# Run in background
docker compose up -d --build

# View logs
docker compose logs -f web
docker compose logs -f mysql

# Shell into web container
docker compose exec web bash

# Shell into MySQL
docker compose exec mysql mysql -u umuser -p UM_ProjectDB
```

### .NET (local)

```powershell
dotnet build
dotnet run
dotnet ef database update   # if you prefer manual migrations
```

### EF Core migrations (developers)

```powershell
dotnet ef migrations add MigrationName
dotnet ef database update
```

Requires `Microsoft.EntityFrameworkCore.Design` (already in the project).

---

## Troubleshooting

### Docker: web container exits or cannot connect to MySQL

- Ensure MySQL healthcheck passes: `docker compose ps`
- Check logs: `docker compose logs mysql`
- Verify `.env` passwords match `MYSQL_PASSWORD` in the connection string (compose substitutes the same values)

### Port 8080 or 3306 already in use

Change in `.env`:

```env
WEB_PORT=8081
```

For MySQL host port, edit `docker-compose.yml` mapping (e.g. `"3307:3306"`).

### `dotnet run` fails: cannot connect to database

- MySQL service is running
- `DefaultConnection` host/user/password/database are correct
- Database exists and user has privileges

### Login works in Docker but not locally (or vice versa)

Each environment has its own MySQL data. Docker uses the `um_mysql_data` volume; local MySQL uses your server instance — seeds are independent.

### QuestPDF / PDF export errors in Docker

The `Dockerfile` installs font libraries required by QuestPDF on Linux. Rebuild the image after Dockerfile changes:

```powershell
docker compose build --no-cache web
```

### Migrations out of date

```powershell
dotnet ef database update
```

Or enable `Database:ApplyMigrationsOnStartup` and restart the app.

---

## Project overview

| Area | Location |
|------|----------|
| Entry point | `Program.cs` |
| Data / EF | `Data/ApplicationDbContext.cs`, `Migrations/` |
| Controllers | `Controllers/` |
| Identity UI | `Areas/Identity/` |
| Services | `Services/` |
| Views | `Views/` |
| Static files | `wwwroot/` |
| Localization | `Resources/` (EN + SQ) |

**Tech stack:** ASP.NET Core 10, Identity, EF Core + MySQL, JWT Bearer, QuestPDF, ClosedXML, MailKit.

---

## License

Add your license text here if applicable.
