# Enterprise IAM — Architecture v1.0

- Modular monolith with explicit Domain, Application, Infrastructure, Identity and API project boundaries.
- SQL Server is the chosen store; EF Core schemas and migrations are not yet implemented.
- Duende IdentityServer and ASP.NET Core Identity are selected for the authentication milestone, not wired in this foundation.
- NUnit is the test framework for unit, integration and architecture checks.
- Angular will be scaffolded once authentication endpoints and routing boundaries are specified.
- Tenant isolation, authorization policies, auditing, OAuth/OIDC, and security hardening require dedicated implementation and threat review.

## Planned next milestone

SQL Server / EF Core integration, identity entities and schema ownership; then authentication / OIDC and Angular.
