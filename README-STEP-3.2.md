# Enterprise IAM Step 3.2 — OIDC foundation patch (requires local verification)

This patch is deliberately incremental and **not signed off**. It is based on the previously uploaded EnterpriseIam.zip, which predates your latest Step 2.4 local formatting correction. Do not overwrite changed files blindly. Preserve your current migrations and repository settings.

## Included
- Duende 8.0.8 ASP.NET Identity and EF Core packages (net10.0)
- AspNet Identity SignInManager registration; no duplicate user model
- Explicit confidential development client (authorization code + PKCE S256, exact HTTPS redirect allowlist)
- IdentityServer host in existing API and operational SQL store in `idsrv` schema
- Secure Identity Provider cookie with distinct name
- Razor Pages sign-in and sign-out (antiforgery on POST)
- Fail-closed production startup until appropriate signing/key management and licensing are implemented

## Important gaps before Step 3.2 completion
1. .NET is unavailable here, so restore/build/format/test are **not executed**. Check Duende v8 API signatures and Razor compilation locally and report compilation errors.
2. Generate **separate Duende operational EF migrations** in the existing API project. The `PersistedGrantDbContext` is provided by Duende; DO NOT run `EnsureCreated` against the database and DO NOT replace the existing `identity` migration. Do not use the app until the `idsrv` tables are migrated.
3. Configure true SQL-persisted production signing keys and Data Protection; development signer here is NOT production ready. Review whether developer signing-credential persisted path remains constant across your local working directory.
4. Implement durable server-side sessions + administrative revocation and OIDC end-session behavior. POST sign-out above is local IdP sign-out only.
5. Add NUnit end-to-end tests for discovery, valid login, wrong password, disabled user, lockout, exact redirect validation, PKCE missing/mismatch, code replay, token issuer/audience, logout, and restart persistence. Do not call the milestone complete without these.
6. Check revalidation of disabled accounts on existing cookies and revoke existing active sessions according to policy. `SecurityStampValidatorOptions.ValidationInterval = Zero` does not by itself check `IsDisabled` in all cases.
7. Add CSP and cookie security review, explicit proxy forwarding restrictions, rate limiting, and test client secret rotation.
8. The OIDC test client is registered only when secret and exact HTTPS callback URLs are supplied. It is NOT a public API or Angular BFF client.

## Configure locally (development only)
PowerShell:
```powershell
$env:ASPNETCORE_ENVIRONMENT = 'Development'
$env:ENTERPRISEIAM_SQL_CONNECTION = 'Server=(localdb)\MSSQLLocalDB;Database=EnterpriseIamDb;Trusted_Connection=True;Encrypt=True;TrustServerCertificate=True'
$env:Oidc__DevelopmentClient__Secret = '<generate a strong random secret; do not commit>'
$env:Oidc__DevelopmentClient__RedirectUri = 'https://localhost:5443/signin-oidc'
$env:Oidc__DevelopmentClient__PostLogoutRedirectUri = 'https://localhost:5443/signout-callback-oidc'
```
Use a real HTTPS host/port for the IdP. Never send the secret from browser JavaScript.

## Migration procedure to review before running
After fixing compilation and confirming the Duende EF registration works, use `dotnet ef migrations add` with the Duende `PersistedGrantDbContext` and explicit output path under the API. Check the generated migration uses `idsrv` schema and the correct migrations history table; then apply it. Verify `identity` schema unchanged.

## Quality gate
```powershell
dotnet restore EnterpriseIam.sln
dotnet build EnterpriseIam.sln --configuration Release -warnaserror
dotnet test EnterpriseIam.sln --configuration Release
dotnet format EnterpriseIam.sln --verify-no-changes --no-restore --severity warn
dotnet list EnterpriseIam.sln package --vulnerable --include-transitive
```

## Security note
The current login uses PasswordSignInAsync. MFA is deferred, but **before MFA enforcement** this must be redesigned so no full sign-in occurs until MFA challenge completion. Development login is not production ready. Tenant context is deliberately absent: global login does not imply access to any tenant.
