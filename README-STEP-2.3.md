# Step 2.3 — Controlled development bootstrap patch

This is an **overlay patch**, not a complete repository. Preserve your locally generated `InitialIdentity` migration, NuGet fixes, `.env`, and tests.

## Apply

1. Copy `DevelopmentBootstrap.cs` to `src/Services/Identity/EnterpriseIam.Identity.Api/Bootstrap/`.
2. In the API's `Program.cs`, add `using EnterpriseIam.Identity.Api.Bootstrap;` above the other usings.
3. Immediately after `var app = builder.Build();` insert:

```csharp
if (args.Length > 0 && args[0] == "--bootstrap")
{
    return await DevelopmentBootstrap.RunAsync(app, args);
}
```

**Important:** If you merge manually, add `return 0;` after `app.Run();` so the top-level entry point has a consistent integer return type. See the complete replacement `Program.cs` included in the patch.

4. The included `Program.cs` replaces the scaffold version if you have not added other API endpoints. If you did, merge only the `using` and guarded bootstrap branch.

## Run from an interactive PowerShell console

Use the same LocalDB connection currently used for EF Core. Avoid putting secrets in arguments or committed configuration.

```powershell
$env:ASPNETCORE_ENVIRONMENT = "Development"
$env:ENTERPRISEIAM_ENABLE_BOOTSTRAP = "true"
$env:ENTERPRISEIAM_SQL_CONNECTION = 'Server=(localdb)\MSSQLLocalDB;Database=EnterpriseIamDb;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True'

dotnet run --project src/Services/Identity/EnterpriseIam.Identity.Api --no-launch-profile -- --bootstrap "Demo Organization" "demo-org" "demo.owner" "owner@example.test"

Remove-Item Env:ENTERPRISEIAM_ENABLE_BOOTSTRAP
```

Enter the initial password when prompted. No login endpoint or externally callable bootstrap API is added.

**Precondition:** The `InitialIdentity` migration must already have been applied; the command does not invoke migrations. The existing DI registration must read the `ENTERPRISEIAM_SQL_CONNECTION` environment variable (via startup configuration or the current API setup). If it does not, use an effective `ConnectionStrings__IdentityDatabase` environment variable instead.

## Validate

```sql
SELECT Id, Name, Slug, Status FROM [identity].[Tenants];
SELECT Id, UserName, Email FROM [identity].[AspNetUsers];
SELECT TenantId, UserId, Status FROM [identity].[TenantMemberships];
```

Re-running with the same slug/name/email must reject the operation without writing new rows. This is **safe rejection**, not full idempotent reconciliation. The existing provisioner inserts all three records in one database transaction. Account creation failure rolls back that transaction.

## Security limits

- Local developer tooling only: do not ship this CLI bootstrap command as a production provisioning mechanism.
- `owner` is only an initial global user with a membership, **not** an administrator until the authorization model exists.
- Changing a tenant status, revoking memberships, and auditing privileged operations need more tests before admin endpoints are exposed.
- Current pre-checks are not concurrency controls; unique constraints and the existing transaction remain authoritative.
- Rotate any example passwords used earlier; do not log or paste real passwords.
- This patch has been statically inspected but not compiled here (no .NET SDK available).
