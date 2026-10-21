# Enterprise IAM — Step 2.4 security and integration tests

This package is a proposed updated source snapshot derived from the earlier Step 2.2 archive and Step 2.3 bootstrap patch. **Do not overwrite your locally generated `InitialIdentity` migration or dependency updates.** The safest approach is to cherry-pick the files listed below after comparing with your current Git branch.

## Files changed

- `.editorconfig`: disables conflicting StyleCop `SA1101`/`SA1309`, preserves mandatory documentation rules, and narrowly excludes generated migrations from selected diagnostics. Ensure your local `SA1633` entry uses `none` (not `file_scoped:warning`).
- `tests/Services/Identity/EnterpriseIam.Identity.IntegrationTests/SqlServerTenantIsolationTests.cs`: adds tests for bootstrap success/rollback, duplicate membership, tenant/member suspension, global account disablement, and optimistic concurrency. The test database name must contain `IntegrationTests`.
- `src/Services/Identity/EnterpriseIam.Identity.Infrastructure/Persistence/IamIdentityDbContext.cs`: expands constructor formatting.

This update does not introduce a new service or database schema. It does not create migrations.

## Important limitations

The .NET SDK is not available in the file-generation environment: no `dotnet build`, `dotnet format`, or NUnit execution has been run here. These tests remain implementation candidates until they run locally. The generated snapshot is **not** a replacement for your Git repository; keep your `Directory.Packages.props` vulnerability fix and current migration files.

The current tenant reader requires active membership but does not yet check tenant administrative permission; it must not be exposed as a general-purpose user-list API. Other threat-model cases (authorization policy evaluation, token/session revocation, raw SQL bypass, privileged admin operations) remain for later milestones.

## Run locally (PowerShell)

```powershell
$env:ENTERPRISEIAM_TEST_SQL_CONNECTION = 'Server=(localdb)\MSSQLLocalDB;Database=EnterpriseIam_IntegrationTests;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True'

dotnet restore EnterpriseIam.sln
dotnet format EnterpriseIam.sln
dotnet build EnterpriseIam.sln --configuration Release
dotnet test EnterpriseIam.sln --configuration Release --logger 'console;verbosity=normal'
dotnet format EnterpriseIam.sln --verify-no-changes --no-restore
# Security gate
dotnet list EnterpriseIam.sln package --vulnerable --include-transitive
```

The tests create persistent randomly named data within `EnterpriseIam_IntegrationTests`; never run them against the main `EnterpriseIamDb`. No test here calls `EnsureDeleted`. For repeatable CI, provision a fresh dedicated database and delete it via a separately reviewed job.

## Signoff conditions

1. All tests green, no database tests skipped, no `InvalidSeverityInAnalyzerConfig` warnings.
2. No NuGet vulnerabilities reported by NuGet's current advisory sources.
3. Review/resolve remaining StyleCop warnings rather than blanket-suppressing them.
4. Verify duplicate bootstrap CLI behavior with development gate disabled and enabled.
5. Capture logs and PR evidence; then proceed to Step 3 authentication, with tenant admin authorization still pending.
