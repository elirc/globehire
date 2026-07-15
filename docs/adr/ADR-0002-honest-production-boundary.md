# ADR 0002: Keep the production boundary explicit

- Status: Accepted
- Date: 2026-07-15

## Context

The course ends with production-readiness practices, but its executable adapters are in-memory and its policy data is fictional. Calling the service production-ready would teach learners to confuse a green build with operated, legally validated software.

## Decision

`v1.0.0` denotes curriculum completeness. Health endpoints expose the current process state and explicitly disclose non-durable storage. Operations documents label SLOs and recovery behavior as candidates. The release checklist blocks real use until durable transactions/outbox, identity and tenant authorization, validated policy ownership, privacy controls, telemetry, load evidence, and disaster recovery exist.

We keep the modular-monolith seams so those capabilities can be integrated without rewriting business rules. We do not add placeholder enterprise infrastructure merely to make the architecture diagram appear complete.

## Consequences

The repository is safe to evaluate and extend as teaching software, but cannot honestly demonstrate backup, restore, production availability, or legal correctness. A learner implementing a durable adapter must also define migration, atomicity, recovery, and operational ownership—not only make repository tests pass.

