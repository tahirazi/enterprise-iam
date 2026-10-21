# Enterprise IAM — Solution Foundation v1.2

An intentionally small, capability-oriented .NET 10 modular monolith using SQL Server and NUnit.

## Tooling

- .NET SDK **10.0.302** (see `global.json`)
- `net10.0` backend, C# SDK default language version
- Visual Studio 2026 or compatible SDK-capable tooling
- Rancher Desktop configured for Docker-compatible CLI, for local SQL Server

## Solution layout

```text
src/Services/Identity/
  EnterpriseIam.Identity.Api/
  EnterpriseIam.Identity.Application/
  EnterpriseIam.Identity.Domain/
  EnterpriseIam.Identity.Infrastructure/
tests/Services/Identity/
  EnterpriseIam.Identity.UnitTests/
  EnterpriseIam.Identity.IntegrationTests/
  EnterpriseIam.Identity.ArchitectureTests/
infrastructure/sqlserver/
```

`Blocks` and `Web` remain conceptual folders until real shared code or an Angular portal is introduced. No separate Contracts or Init project is required yet.

## Run

```powershell
Copy-Item .env.example .env
docker compose up -d sqlserver
dotnet restore EnterpriseIam.sln
dotnet build EnterpriseIam.sln
dotnet test EnterpriseIam.sln
dotnet run --project src/Services/Identity/EnterpriseIam.Identity.Api
```

Set a strong `MSSQL_SA_PASSWORD` in `.env` before starting SQL Server. API URL is defined in the project's `Properties/launchSettings.json`.

## Code-quality gates

```powershell
dotnet format EnterpriseIam.sln --verify-no-changes --no-restore --severity warn
dotnet build EnterpriseIam.sln --configuration Release --no-restore
dotnet test EnterpriseIam.sln --configuration Release --no-build
```

Production code requires XML summaries and documented parameters, enforced through `.editorconfig`, C# compiler diagnostics and StyleCop.Analyzers. Test projects keep formatting rules but have relaxed documentation checks. Use `<inheritdoc />` for appropriate implementations. Migrations are treated as generated code.

## Validation status

Solution/project paths and XML were checked by generation-time static validation. The .NET SDK is unavailable in the generating environment: restore, compiler checks, StyleCop compatibility, NUnit and CI need to be run on a machine with SDK 10.0.302. The `StyleCop.Analyzers` prerelease version is deliberately provisional until validated.

Architecture and standards decisions: `docs/adr/`.
