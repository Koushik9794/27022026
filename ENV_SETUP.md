# 📄 Environment Variable Setup for gss‑backend

## 🎯 Goal
Move every **hard‑coded secret / configuration value** out of the source code and into a **`.env`** file that can be loaded as environment variables at runtime.

---

## 📦 What the code currently expects
| Where it’s read | Hard‑coded value (example) | Environment‑variable name that will satisfy the same call |
|-----------------|---------------------------|----------------------------------------------------------|
| `builder.Configuration.GetConnectionString("DefaultConnection")` (all services) | `Host=postgres;Database=admin_service;Username=postgres;Password=postgres;Port=5432;` | `ConnectionStrings__DefaultConnection` |
| `builder.Configuration.GetConnectionString("DefaultConnection")` (rule‑service tests) | `Host=127.0.0.1;Port=5433;Database=rule_service;Username=postgres;Password=postgres;SSL Mode=Disable;` | `TestConnectionString` (you can map it in the test project) |
| `appsettings.json` → `JwtSecret` (gss‑web‑api) | `"a_very_long_and_secure_secret_key_for_development_123456"` | `JwtSecret` |
| Docker‑compose files (`docker-compose.yml`, `gss‑web‑api/docker-compose.yml`) | `ConnectionStrings__DefaultConnection=Host=postgres;Database=gss_db;Username=gss_user;Password=gss_password` | `ConnectionStrings__DefaultConnection` (same as above) |
| Any other literal secrets you may have (e.g. AWS keys in docs) – **not used by code** yet, but you can add them later if needed. |

> **Why the double underscore?**
> In .NET, hierarchical keys (`ConnectionStrings:DefaultConnection`) are represented in environment variables as `ConnectionStrings__DefaultConnection`. The framework automatically maps them when you call `GetConnectionString` or `Configuration["JwtSecret"]`.

---

## 🗂️ Recommended `.env` template
Create a file **at the root of the repository** named `.env`. Add it to `.gitignore` (it already is in the repo) so that the secrets never get committed.

```dotenv
# -------------------------------------------------
# Database connection strings (used by all services)
# -------------------------------------------------
ConnectionStrings__DefaultConnection=Host=postgres;Database=admin_service;Username=postgres;Password=postgres;Port=5432

# If you run the rule‑service integration tests locally, you can
# override the test connection string with this variable.
TestConnectionString=Host=127.0.0.1;Port=5433;Database=rule_service;Username=postgres;Password=postgres;SSL Mode=Disable

# -------------------------------------------------
# JWT / authentication secrets
# -------------------------------------------------
JwtSecret=YOUR_JWT_SECRET_HERE   # 256‑bit base‑64 string is recommended

# -------------------------------------------------
# (Optional) AWS / other cloud secrets – add as needed
# -------------------------------------------------
# AWS_ACCESS_KEY_ID=...
# AWS_SECRET_ACCESS_KEY=...
# AWS_DEFAULT_REGION=us-east-1
```

> **Tip:** Use a password manager or secret‑manager (AWS Secrets Manager, Azure Key Vault, etc.) to generate the real values and inject them into the container at deploy time. The `.env` file is only for local development.

---

## 🚀 How the code picks up the values automatically
1. **ASP.NET Core already loads environment variables**
   `WebApplication.CreateBuilder(args)` internally calls `builder.Configuration.AddEnvironmentVariables()`. Therefore any variable present in the process environment is available through `builder.Configuration`.
2. **No code change required for the DB string** – the existing line already does:
   ```csharp
   var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
   ```
   When the environment contains `ConnectionStrings__DefaultConnection`, the call returns that value.
3. **JWT secret** – the same principle applies:
   ```csharp
   var secret = authConfig["JwtSecret"] ?? "fallback‑dev‑secret";
   ```
   If `JwtSecret` is defined in the environment, the fallback is never used.
4. **Loading a `.env` file** (optional but convenient for local dev)
   .NET does **not** read a `.env` file automatically. Add a tiny helper at the very top of `Program.cs` (or any entry point) to load it:
   ```csharp
   // Add this as the first line in every Program.cs
   DotNetEnv.Env.Load();   // <-- requires the DotNetEnv NuGet package
   ```
   Then run `dotnet add package DotNetEnv` once for the solution (or per‑project). The package reads the `.env` file and populates `Environment.GetEnvironmentVariable`, so the rest of the code works unchanged.
   *If you prefer not to add a package, you can simply export the variables in your shell before running the app (`export $(cat .env | xargs)` on Linux/macOS or `Set-Item -Path Env:$(Get-Content .env)` in PowerShell).* 

---

## 📋 Checklist for the migration
| ✅ | Action | Done? |
|----|--------|-------|
| 1 | Add **`.env`** file with the keys shown above. | |
| 2 | Add `.env` to **`.gitignore`** (already present). | |
| 3 | Install **DotNetEnv** (optional but recommended for local dev). `dotnet add package DotNetEnv` in each service project. | |
| 4 | Insert `DotNetEnv.Env.Load();` as the **first line** of each `Program.cs`. | |
| 5 | Verify that **no other hard‑coded secrets** remain (search for `Password=`, `JwtSecret`, `ConnectionString` in code). | |
| 6 | Run the application locally: `dotnet run` (or `docker compose up` after `docker compose --env-file .env up`). | |
| 7 | Confirm the app starts without the “Connection string not found” exception and that JWT generation works. | |
| 8 | Update CI/CD pipelines to inject the same environment variables from a secret manager instead of a `.env` file. | |

---

## 🛠️ Quick verification script (optional)
You can run a short PowerShell snippet to print the resolved values:
```powershell
# PowerShell (Windows)
$env:ConnectionStrings__DefaultConnection
$env:JwtSecret
```
If the values appear, the runtime is correctly picking them up.

---

**TL;DR** – All hard‑coded values are connection strings and the JWT secret. Create a **`.env`** file with `ConnectionStrings__DefaultConnection`, `TestConnectionString`, and `JwtSecret`. .NET automatically reads those variables; just load the `.env` file (via `DotNetEnv` or shell export) and the existing code will work without any further changes.
