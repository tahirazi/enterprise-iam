# Enterprise IAM — Step 2.2 implementation increment

Built on Foundation v1.2.1, preserving the seven project solution and .NET SDK 10.0.302.

- Read `docs/architecture/step-2.2-identity-persistence.md` before generating migrations.
- Restore, build, format and run NUnit tests using the commands in the base README.
- The SQL Server tests require `ENTERPRISEIAM_TEST_SQL_CONNECTION` pointing to an isolated database and an initial EF migration.
- No public provisioning endpoint has been added; do not interpret this as a complete authentication/authorization API.
- EF migration not generated: dotnet SDK/EF CLI unavailable in this build environment. Generate the migration locally and share results for review.
