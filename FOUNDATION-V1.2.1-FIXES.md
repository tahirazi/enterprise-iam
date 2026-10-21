# Enterprise IAM Foundation v1.2.1 — Build cleanup

- Fixed Visual Studio solution folder/project GUID collision for `Infrastructure`.
- Standardized solution folder project type GUID to the Visual Studio solution-folder type.
- Set `SA1200` and `SA1633` to `none`; file headers are not required and using directives belong outside file-scoped namespaces.
- Expanded documented property accessors to align with the `csharp_new_line_before_open_brace = all` policy.

## Verify

```powershell
dotnet restore EnterpriseIam.sln
dotnet format EnterpriseIam.sln --no-restore
dotnet build EnterpriseIam.sln
dotnet test EnterpriseIam.sln
dotnet format EnterpriseIam.sln --verify-no-changes --no-restore
```

Build and test execution were not possible in the packaging environment.
