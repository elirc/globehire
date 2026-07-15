# GlobeHire

GlobeHire is an original C#/.NET teaching implementation of a global workforce and employer-of-record platform. It is inspired by publicly described EOR workflows—international hiring, compliant contracts, payroll, benefits, leave, expenses, contractor management, immigration cases, lifecycle changes, and integrations—without copying Pebl/Velocity Global code, private behavior, wording, assets, or branding.

The repository is the curriculum. A junior engineer studies fifteen sequential sprint pull requests, stakeholder issues, ordered commits, review discussions, ADRs, business-rule tests, API contracts, and five deliberately planted-then-repaired flaws.

## Engineering focus

- business rules modeled independently of ASP.NET Core, EF Core, and transport DTOs;
- thin HTTP endpoints with versioned contracts, OpenAPI, and RFC 9457-style Problem Details;
- explicit use-case services instead of framework-driven business logic;
- effective-dated country policies, money/currency invariants, approvals, and lifecycle state machines;
- tenant isolation, audit records, idempotency, transactional outbox, webhooks, and import/export boundaries;
- adapter contract tests so persistence and external providers can be replaced safely;
- modular-monolith deployment with seams for later extraction, not premature microservices.

## Architecture direction

```text
HTTP / OpenAPI contracts
          |
          v
Application use cases -----> ports for persistence, clock, identity, events
          |
          v
Domain aggregates, policies, value objects, and events
          ^
          |
Infrastructure adapters (in-memory first, EF/provider adapters later)
```

Dependencies point inward. The domain has no ASP.NET Core, EF Core, serialization, or provider dependency. The API maps contracts explicitly and does not expose persistence models.

## Deliberate non-choices

GlobeHire starts without microservices, MediatR, AutoMapper, a generic repository, an enterprise service bus, or a custom “framework.” Each would add indirection before a concrete change or integration requires it.

Read [the learner guide](LEARNER-GUIDE.md), [product and curriculum specification](docs/curriculum/SPEC.md), and [contribution workflow](CONTRIBUTING.md) before Sprint 1.

## Run Sprint 1

The first vertical slice requires the .NET 10 SDK pinned in `global.json`.

```powershell
dotnet restore GlobeHire.slnx --locked-mode
dotnet run --project src/GlobeHire.Api
```

Use [the HTTP examples](src/GlobeHire.Api/GlobeHire.Api.http) to create and replay a country-aware workforce plan or inspect `/openapi/v1.json`. Responses include effective policy identity, itemized employer cost, and assumptions. Then run the complete suite:

```powershell
dotnet test GlobeHire.slnx
```

See [the testing guide](TESTREADME.md), [Sprint 1 evidence](docs/sprints/sprint-01.md), and [ADR 0001](docs/adr/ADR-0001-modular-monolith-and-explicit-boundaries.md) for the reasoning behind the implementation.

## Attribution and limits

Pebl and Velocity Global are trademarks of their owners. GlobeHire is an independent educational project and is not affiliated with or endorsed by them. Country rules, payroll calculations, tax treatment, benefits, immigration, and legal templates in this repository are fictional teaching fixtures—not legal, tax, payroll, or employment advice.
