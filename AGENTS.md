# AGENTS.md

## Project overview

ASP.NET Core 10.0 MVC + REST API — "EcoRut" / "EcoRoute". Ecological routes, forum, chat, and admin features. Single-project solution (no monorepo).

## Build and run

```bash
dotnet restore
dotnet build
dotnet run                        # dev server on http://localhost:5073
dotnet run --project Proyecto_Grupo_gris.csproj  # explicit project ref
```

There are **no test projects**, no linter config, no CI workflows, and no pre-commit hooks in this repo.

## Database

- **PostgreSQL** via EF Core + Npgsql.
- Connection string: `appsettings.json` → `ConnectionStrings:Default` (placeholder `#{DATABASE_URL}#`).
- `appsettings.Development.json` has a real Render.com connection string but is **gitignored** — never commit it.
- **Migrations auto-apply on startup** (`Program.cs:252-266`). If the DB is unreachable the app logs the error and continues.
- When adding/modifying models: `dotnet ef migrations add <Name> --project .`

## Identity & Auth

- ASP.NET Identity with roles (`Admin`, etc.). Roles + admin user are seeded on startup (`Data/IdentityDataSeeder.cs`).
- Two auth schemes: **JWT Bearer** (API) and **cookies** (MVC). Google OAuth is wired in.
- JWT settings live under `JwtSettings` in config. The `Secret` placeholder in `appsettings.json` is not real — use User Secrets or env vars for real keys.
- Password policy: no special chars required, min 8 chars.

## Architecture

| Layer | Location | Notes |
|-------|----------|-------|
| MVC Controllers | `Controllers/` | `Home`, `Account`, `Chat`, `Forum` |
| API Controllers | `Api/Controllers/` | REST endpoints (Auth, Users, EcoRoutes, ForumPosts, Comments, Weather) |
| ViewModels | `Models/` (files named `*ViewModel.cs`) | Separate from EF entities |
| DTOs | `Api/DTOs/` | Nested per domain (Auth/, Comments/, EcoRoutes/, Forum/, etc.) |
| Repositories | `Api/Repositories/` | Implementations + `Interfaces/` subfolder |
| Services | `Api/Services/` + `Services/` | `Services/ChatService.cs` is separate from `Api/Services/` |
| EF Context | `Data/ApplicationDbContext.cs` | Manual Fluent API config for all entities |
| Migrations | `Migrations/` | Auto-generated |
| Views | `Views/` | Razor views per controller area |
| Identity UI | `Areas/Identity/Pages/` | Scaffolded (only `_ViewStart.cshtml` present) |
| ML | `ML/SentimentAnalysis/` | ML.NET model — auto-trains on startup if `.mlnet` file missing |
| Static assets | `wwwroot/` | |

## External services

| Service | Config key | Notes |
|---------|-----------|-------|
| PostgreSQL | `ConnectionStrings:Default` | Render.com in dev |
| Redis | `ConnectionStrings:Redis` | Falls back to in-memory cache in Development |
| Ollama (AI chat) | `OllamaChat:*` | Semantic Kernel, defaults to `gemma3:4b` at `localhost:11434` |
| Google Maps | `GoogleMaps:ApiKey` | Key hardcoded in `appsettings.json` |
| OpenWeather | `OpenWeather:ApiKey` | Placeholder only |
| Cloudinary | `Cloudinary:*` | Placeholder only |
| Google OAuth | `Authentication:Google:*` | Use User Secrets |

## Conventions

- Namespace pattern: `Proyecto_Grupo_gris.{Area}.{SubArea}` (e.g., `Proyecto_Grupo_gris.Api.Repositories.Interfaces`).
- DI registration is all in `Program.cs` — add new services there.
- AutoMapper profile in `Api/Mappings/AutoMapperProfile.cs`.
- Repository pattern: each domain entity has its own repository + interface.
- API controllers return DTOs; MVC controllers use ViewModels.
- Swagger UI at `/swagger` in Development only.

## Gotchas

- `appsettings.Development.json` is **gitignored** — real credentials live there. Do not create or commit it.
- ML model (`ML/SentimentAnalysis/SentimentAnalysis.mlnet`) trains automatically at startup if missing. This can delay first boot.
- Redis connection string parsing in `Program.cs:188-222` handles `redis://` and `rediss://` URI formats — respect this if modifying cache config.
- The app applies EF migrations and seeds identity data on every startup. A broken migration will log an error but the app still starts.
- No test suite exists. Verify changes manually or via Swagger.
