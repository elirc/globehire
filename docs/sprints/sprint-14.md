# Sprint 14: Hardening and flaw harvest

## Goal

Expose and repair all five planted flaws, then strengthen security, concurrency, compatibility, and performance evidence.

## Scope

Include real red commits, minimal green repairs, race/fault injection, authorization audit, OpenAPI compatibility check, dependency/secret audit, performance budgets, and resilience tests.

## Teaching focus

Counterexample-driven debugging, review blind spots, concurrency determinism, security boundaries, and claims backed by evidence.

## Planned artifacts

Five red/green arcs, security audit, compatibility report, performance evidence, ADR, and debugging note.

## Exercises

## Delivered evidence

- Largest-remainder money allocation reconciles every cent.
- Half-open policy dates and tenant-bound lookups retain their Sprint 13/10 regressions.
- Offer mutations execute atomically inside the repository adapter.
- Concurrent writers with the same version yield one success and one conflict.
- Webhook retries retain stable delivery and message identities while incrementing attempt.
- The public flaw-harvest ledger links every planted defect to its owning fix and test.

## Exercises

Revert each fix and predict the failing evidence. Add one generated counterexample without weakening the domain model.
