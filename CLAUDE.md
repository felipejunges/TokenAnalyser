# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Commands

```bash
# Run the application
dotnet run --project TokenAnalyser/TokenAnalyser.csproj

# Build
dotnet build

# Add a new EF Core migration
dotnet ef migrations add <MigrationName> --project TokenAnalyser/TokenAnalyser.csproj

# Apply migrations manually
dotnet ef database update --project TokenAnalyser/TokenAnalyser.csproj
```

## Architecture

**TokenAnalyser** is a Blazor Server app (.NET 10) for tracking daily AI token usage. It uses SQLite via EF Core, Bootstrap for styling, ApexCharts for visualisation, and ASP.NET Core Identity for authentication.

### Request / data flow

1. All pages require authentication (`[Authorize]`). Unauthenticated requests are redirected to `/Account/Login` by `RedirectToLogin.razor`.
2. Pages inject `DailyUsageService` (scoped), which uses `IDbContextFactory<AppDbContext>` — it creates and disposes a fresh `AppDbContext` per operation, keeping Blazor's long-lived circuit safe from context lifetime issues.
3. `AppDbContext` extends `IdentityDbContext<IdentityUser>` and exposes a single `DailyUsages` `DbSet<DailyUsage>`.
4. On startup (`Program.cs`), `db.Database.Migrate()` runs automatically and a default admin user (`admin@tokenanalyser.com` / `Admin@123!`) is seeded if absent.
5. Logout is a plain GET to `/Account/Logout` — a minimal API endpoint that calls `SignOutAsync` and redirects.

### Key domain concept

`DailyUsage` records a token count (`Usage decimal`) for a given `Date` and `Model` string. The monthly budget is hard-coded at **1 500 tokens**. The Dashboard (`/`) plots cumulative actual vs. theoretical burn-up using ApexCharts, and shows four KPI cards (Total Used, Remaining, % Used, % Theoretical).

### Pages

| Route | File | Purpose |
|---|---|---|
| `/` | `Components/Pages/Home.razor` | Dashboard with burn-up chart and KPI cards |
| `/daily-usage` | `Components/Pages/DailyUsageCrud.razor` | CRUD table with add/edit/delete modals |
| `/Account/Login` | `Components/Pages/Account/Login.razor` | Login form (SSR, `method="post"`) |

### Database

SQLite file is stored at `TokenAnalyser/tokenanalyser.db` (excluded from git). EF Core migrations live in `TokenAnalyser/Migrations/`.
