# Sprint 15: Production readiness and retrospective

## Goal

Publish an honestly scoped `v1.0.0` with deterministic demo data, observability boundaries, runbooks, a simulated incident, the revealed flaw ledger, and a complete course retrospective.

## Scope

Include health/readiness, structured events, trace propagation, SLO candidates, deploy/rollback, backup/restore design, retention, threat model, incident drill, release audit, and integration examples. Exclude claims requiring an actual production environment.

## Teaching focus

Operability, evidence scopes, release communication, recovery objectives, privacy-aware telemetry, and deciding what remains deployment-owned.

## Planned artifacts

Seed command, runbooks, postmortem, release checklist, flaw ledger, retrospective, ADR, and operations note.

## Exercises

Implement a durable adapter against the contract suite. Rehearse duplicate webhook delivery and a failed payroll close, then update the runbook from evidence.
