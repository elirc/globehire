# Sprint 15: Production readiness and retrospective

## Goal

Publish an honestly scoped `v1.0.0` with deterministic demo data, observability boundaries, runbooks, a simulated incident, the revealed flaw ledger, and a complete course retrospective.

## Scope

Include health/readiness, structured events, trace propagation, SLO candidates, deploy/rollback, backup/restore design, retention, threat model, incident drill, release audit, and integration examples. Exclude claims requiring an actual production environment.

## Teaching focus

Operability, evidence scopes, release communication, recovery objectives, privacy-aware telemetry, and deciding what remains deployment-owned.

## Delivered artifacts

- liveness/readiness responses and trace propagation with API tests;
- an operational runbook and simulated duplicate-webhook incident;
- an integration guide and explicit production-readiness assessment;
- ADR 0002 recording why `v1.0.0` is curriculum-complete but not production-ready;
- the revealed five-flaw harvest with regression evidence;
- an over-engineering review, expanded junior testing guide, changelog, and course retrospective.

Deterministic test fixtures remain the seed data for this in-memory build. A mutable demo seed command was intentionally not added because it would imply durable environment lifecycle support that the executable system does not have.

## Evidence

- `/health/live` reports process liveness;
- `/health/ready` reports ready while disclosing `durable: false` and `storage: in-memory`;
- every HTTP response carries `X-Trace-Id` for correlation;
- CI performs locked restore, formatting, Release build, all test suites, dependency audit, and history secret scan;
- [the readiness assessment](../quality/PRODUCTION-READINESS.md) lists real-deployment blockers rather than hiding them.

## Exercises

Implement a durable adapter against the contract suite. Rehearse duplicate webhook delivery and a failed payroll close, then update the runbook from evidence.
