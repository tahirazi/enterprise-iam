# Step 2.4 quality-gate patch

This archive contains **only changed source and configuration files** relative to the uploaded `EnterpriseIam.zip`. It intentionally excludes `.env`, `.git`, binaries, local connection settings, and passwords.

## Applied changes

- Correct invalid `SA1633` analyzer severity (`none`), retaining the separately configured file-scoped namespace preference.
- Format EF Core LINQ `Join` calls in `TenantAccess.cs` according to `SA1117`.
- Format the multiline bootstrap test call according to `SA1117`.
- Add a null-forgiving assertion for an already initialized test service provider in the concurrency test (`provider!`), consistent with existing fixture checks.
- Normalize C# files from CRLF/mixed line endings to LF, matching `.editorconfig` and `.gitattributes`. This may produce large whitespace-only Git diffs initially.

## Applying

Extract the archive at the root of your repository. Review the diff before committing. In particular, preserve your already-applied EF Core migration content; this patch changes line endings only in migration files, not their SQL operations or identifiers.

## Validate locally

```powershell
dotnet restore EnterpriseIam.sln
dotnet build EnterpriseIam.sln --configuration Release
dotnet test EnterpriseIam.sln --configuration Release
dotnet format EnterpriseIam.sln --verify-no-changes --no-restore
dotnet list EnterpriseIam.sln package --vulnerable --include-transitive
```

If `dotnet format` still reports outstanding issues, inspect them and run `dotnet format EnterpriseIam.sln` only after reviewing its expected edits. The current environment cannot compile the C# solution, so passing local checks are required for formal sign-off.

## Safety

The original uploaded ZIP includes a `.env` and `.git` history. Do not commit or republish that full ZIP. Keep secrets and local state out of repository artifacts.
