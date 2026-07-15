# GlobeHire

GlobeHire is a complete fifteen-sprint C#/.NET teaching repository for a fictional global workforce and employer-of-record platform. It focuses on changeable business logic, versioned API design, integration boundaries, and the engineering judgment a junior developer needs to grow.

It is an original educational project inspired by publicly described industry workflows. It does not copy Pebl/Velocity Global code, private behavior, wording, assets, or branding, and it is not affiliated with or endorsed by those companies.

## What you will learn

- model money, dates, policies, approvals, and lifecycle state machines independently of ASP.NET Core;
- keep HTTP contracts versioned and separate from domain and persistence shapes;
- design tenant-bound ports, idempotency, optimistic concurrency, outbox/webhook identities, and partial-failure results;
- test business risk at the lowest useful level, then sample public wiring through the API;
- recognize when architecture helps change and when ceremony becomes over-engineering;
- distinguish a green build from honest production readiness.

## Architecture

```text
HTTP / OpenAPI contracts
          |
          v
Application use cases -----> persistence, time, identity, and event ports
          |
          v
Domain aggregates, policies, value objects, and events
          ^
          |
Infrastructure adapters (in-memory teaching adapters today)
```

Dependencies point inward. The domain has no ASP.NET Core, EF Core, serialization, or provider dependency. The API maps contracts explicitly. This is a modular monolith with seams for proven integration needs, not premature microservices.

## Course map

| Sprint | Outcome | Evidence |
| --- | --- | --- |
| 1 | Workforce-plan vertical slice | [notes](docs/sprints/sprint-01.md) |
| 2 | Effective-dated country estimates | [notes](docs/sprints/sprint-02.md) |
| 3 | Hire-case state machine | [notes](docs/sprints/sprint-03.md) |
| 4 | Offers, approvals, contracts | [notes](docs/sprints/sprint-04.md) |
| 5 | Onboarding and activation (`v0.5.0`) | [notes](docs/sprints/sprint-05.md) |
| 6 | Payroll rules and statements | [notes](docs/sprints/sprint-06.md) |
| 7 | Leave, expenses, benefits | [notes](docs/sprints/sprint-07.md) |
| 8 | Contractors and invoices | [notes](docs/sprints/sprint-08.md) |
| 9 | Employment changes and offboarding | [notes](docs/sprints/sprint-09.md) |
| 10 | Tenant/IDOR hardening | [notes](docs/sprints/sprint-10.md) |
| 11 | Outbox and webhooks | [notes](docs/sprints/sprint-11.md) |
| 12 | Bulk validation and reporting | [notes](docs/sprints/sprint-12.md) |
| 13 | Compliance evidence | [notes](docs/sprints/sprint-13.md) |
| 14 | Five-flaw harvest | [notes](docs/sprints/sprint-14.md) |
| 15 | Operations and retrospective (`v1.0.0`) | [notes](docs/sprints/sprint-15.md) |

Study the milestones, issues, ordered commits, and review comments as part of the curriculum. Start with the [learner guide](LEARNER-GUIDE.md) and [product specification](docs/curriculum/SPEC.md).

## Run it

The repository pins the .NET 10 SDK in `global.json`.

```powershell
dotnet restore GlobeHire.slnx --locked-mode
dotnet run --project src/GlobeHire.Api
```

Use [the HTTP examples](src/GlobeHire.Api/GlobeHire.Api.http), inspect `/openapi/v1.json` in Development, and check `/health/live` and `/health/ready`. The readiness response intentionally discloses that storage is in-memory and non-durable.

Run all four test suites:

```powershell
dotnet test GlobeHire.slnx --configuration Release
```

Read [TESTREADME](TESTREADME.md) for the reasoning behind the tests, [OEReadMe](OEReadMe.md) for the requested over-engineering assessment, and the [production-readiness assessment](docs/quality/PRODUCTION-READINESS.md) before extending the service.

## Deliberate non-choices

GlobeHire does not add MediatR, AutoMapper, a generic repository, microservices, an enterprise bus, or a custom framework. Each adds indirection before this code has evidence that it would improve changeability.

## Attribution and limits

Country rules, payroll calculations, tax treatment, benefits, immigration, and legal templates are fictional teaching fixtures, not legal, tax, payroll, employment, or security advice. The service uses volatile in-memory storage and has no real authentication. `v1.0.0` means the curriculum is complete, not that the software is deployable for real workers.
