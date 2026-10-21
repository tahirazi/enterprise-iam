# ADR-0001: Modular monolith first

Status: Accepted

## Context
IAM requires coherent security boundaries but does not initially justify distributed deployments.

## Decision
Use a modular monolith with separate domain, application, infrastructure, identity and API assemblies. Enforce dependencies with architecture tests.

## Consequences
Simpler local setup and transactions. Module boundaries must remain explicit to avoid coupling.
