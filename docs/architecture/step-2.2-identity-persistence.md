# Step 2.2 — Identity persistence

## Scope

This increment adds global ASP.NET Identity user records, canonical tenants, composite-key memberships, tenant-validated readers and an internal-only bootstrap provisioning service. It does not expose tenant administration, implement OIDC, or implement tenant RBAC.

## Important security boundaries

- Global users have no `TenantId`; memberships are the sole link to tenants.
- Do not treat access to `IamIdentityDbContext.Users` as tenant authorization.
- Tenant access checks require active tenant, active membership, and enabled user; `ITenantMemberReader` checks the requesting actor before reading.
- There are no unsafe unscoped tenant API routes. Later HTTP handlers must authenticate, resolve the tenant, and enforce authorization before accessing resources.
- No EF global filter is installed on global users. Explicit scope checks are used for the initial read path; do not interpret this as complete application-wide isolation.
- `ITenantBootstrapProvisioner` is intentionally not exposed by an HTTP endpoint. Provisioning requires a separately authorized internal runner in a later step.
- `UserManager` owns credential state. The account's email confirmation remains false until a verified workflow is introduced.

## Migration generation (run with SDK 10.0.302)

The SDK/EF CLI is not available in the generation environment; no unverified migration snapshot has been checked in. Generate and review the initial migration from the model:

```powershell
$env:ENTERPRISEIAM_SQL_CONNECTION='Server=localhost,14333;Database=EnterpriseIamDb;User Id=sa;Password=<strong-password>;Encrypt=True;TrustServerCertificate=True'
dotnet tool install --global dotnet-ef --version 10.0.0
dotnet ef migrations add InitialIdentity --project src/Services/Identity/EnterpriseIam.Identity.Infrastructure --startup-project src/Services/Identity/EnterpriseIam.Identity.Api --context IamIdentityDbContext --output-dir Persistence/Migrations
dotnet ef database update --project src/Services/Identity/EnterpriseIam.Identity.Infrastructure --startup-project src/Services/Identity/EnterpriseIam.Identity.Api --context IamIdentityDbContext
```

Review DDL for all expected identity user-store tables, `identity.Tenants`, composite membership PK/FKs, unique normalized-email and tenant-slug indexes, rowversions, check constraints, and `identity.__EFMigrationsHistory` before applying to shared databases.

## Real SQL Server tests

`ENTERPRISEIAM_TEST_SQL_CONNECTION` must reference a **dedicated disposable test database**. SQL integration tests run only when set. They intentionally use actual SQL Server relational constraints; EF InMemory is not a substitute. Always generate the migration before running these tests.

## Explicitly deferred

Custom tenant roles, permissions, MFA, Duende OIDC, sessions, full security audit events, SQL Server RLS, production tenant provisioning API and automated production migrations.
