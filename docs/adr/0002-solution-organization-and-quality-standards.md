# ADR 0002: Capability-oriented Solution and Coding Standards

Status: Accepted (Architecture v1.0)

## Decision

Deploy one modular-monolith API and group its four runtime projects under `Services/Identity`.
Keep three NUnit test projects physically under `tests/Services/Identity` but within the same Visual Studio capability folder. Defer standalone Contracts, Init, and shared Blocks packages. Separate platform infrastructure under `infrastructure/` from application adapter code in `EnterpriseIam.Identity.Infrastructure`.

Use SDK 10.0.302, `net10.0`, central package versions, XML documentation in production projects, built-in .NET analyzers, and StyleCop, with format checks in CI.

## Trade-offs

More projects than a simple web app, but explicit domain and application boundaries. A future move to microservices would require distinct data ownership and operational design; the folder structure does not ensure extractability.

The chosen StyleCop beta needs validation with the installed SDK before calling the toolchain verified.
