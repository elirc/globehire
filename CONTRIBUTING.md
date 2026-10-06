# Contributing to GlobeHire

## Working agreements

1. Work sequentially: one sprint branch and pull request at a time.
2. Begin from a stakeholder outcome and business invariant, not a framework feature.
3. Implement in the order domain rule → use case → adapter → HTTP contract.
4. Keep endpoints thin and persistence models private to infrastructure.
5. Prefer explicit code over reflection, convention-heavy mapping, or generic abstractions.
6. Add an interface only when at least one caller needs a replaceable boundary.
7. Every external write is idempotent or documents why it cannot be retried.
8. Every tenant-owned lookup includes tenant scope at the repository boundary.
9. Use UTC instants for events, `DateOnly` for local policy dates, and explicit ISO currency codes for money.
10. CI runs without Docker, a database server, network services, or provider credentials.

## Sprint definition of done

- stakeholder acceptance criteria work through the HTTP API;
- business invariants have fast domain tests;
- request, response, error, concurrency, and authorization contracts are explicit;
- adapter contract tests protect substitutability;
- OpenAPI reflects the shipped endpoint behavior;
- one ADR records a real decision with alternatives and consequences;
- one concept note teaches the core business/API design idea;
- review discussion includes a genuine trade-off, not a scripted agreement;
- build, format, analyzers, tests, package audit, and secret scan are green.

> **Status note (October 2026):** this definition of done describes the course's intent. The shipped repository has no adapter contract tests (there is no Infrastructure test project; adapters are exercised through Application and API tests) and no authorization contract beyond tenant-scoped lookups, because the API has no authentication. Treat those two bullets as targets for new work, not as existing coverage.

## Commit order

Use small, runnable commits that tell the implementation story. Defect harvests preserve a real red test commit immediately before the repair. Do not mix refactors with behavior changes when the separation helps review.
