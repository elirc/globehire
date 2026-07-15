# Production-readiness assessment

`v1.0.0` means the fifteen-sprint teaching repository is complete. It does **not** mean this fictional workforce platform is ready to employ or pay people.

| Area | Evidence in this repository | Status for real production |
| --- | --- | --- |
| Business logic | Explicit state machines, money/date invariants, and regression tests | Teaching fixtures only; country, tax, payroll, and legal rules require qualified owners and validated source data |
| API design | Versioned routes, Problem Details, idempotency examples, ETags, tenant-bound lookups | Add an identity provider, tenant claims, role/scope authorization, quotas, and contract compatibility governance |
| Persistence | Replaceable repository ports and in-memory adapters | Blocking: durable database, migrations, transaction boundaries, atomic outbox, backup and restore |
| Integrations | Outbox/webhook models, stable delivery identity, bulk row outcomes | Add network delivery workers, secret rotation, HMAC verification, SSRF controls, replay tooling, and provider contract tests |
| Security/privacy | Threat model, tenant regression, secret scanning, dependency audit | Add formal threat review, PII classification, encryption, retention/deletion, audit access controls, and penetration testing |
| Operations | Liveness/readiness endpoints, trace response header, runbook, incident drill | Add telemetry backend, dashboards, alerts, SLO ownership, capacity/load tests, on-call and disaster-recovery rehearsal |

## Release gates used here

- locked restore, formatting, Release build, and all four test suites pass in CI;
- transitive packages are audited for known vulnerabilities;
- repository history is scanned for secrets;
- the five known flaws have counterexample tests and repairs;
- runbook statements distinguish executable evidence from proposed production design;
- no real PII, credentials, legal templates, or proprietary source material are included.

## Decision

The repository is ready as a reference implementation and junior-engineer curriculum. Production deployment remains blocked by durable storage, real authentication/authorization, validated legal and payroll policy, privacy controls, and operated infrastructure. See [ADR 0002](../adr/ADR-0002-honest-production-boundary.md) for why that boundary is intentional.

