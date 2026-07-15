# Sprint 01: Vertical slice and API foundations

## Goal

Create a client workforce plan and retrieve its quote through a real `/api/v1` HTTP path that crosses a contract, endpoint, application use case, domain model, and replaceable in-memory repository.

## Scope

Include .NET 10 solution conventions, money/country primitives, Problem Details, OpenAPI, correlation IDs, deterministic IDs/clocks, DB-free tests, format/analyzer/build CI, and ADR 0001. Exclude authentication, EF Core, payroll, and generic infrastructure.

## Teaching focus

Dependency direction, thin endpoints, explicit mapping, validation placement, and why a vertical slice is stronger than disconnected health checks.

## Planned artifacts

Domain and API tests, architecture guard, OpenAPI snapshot, integration example, ADR, concept note, and HTTP examples.

## Exercises

Add a new planning field without leaking the domain type into the contract. Replace the repository with a file adapter using the same contract suite.
