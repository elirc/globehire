# Sprint 05: Onboarding and employment activation

## Goal

Complete required onboarding steps and activate an employment record exactly once. Publish the `v0.5.0` MVP.

## Scope

Include worker profile references, document metadata only, task completion, missing-data rules, activation idempotency, public API examples, and MVP demo. Exclude sensitive document contents and payroll.

## Teaching focus

PII minimization, completion invariants, retry-safe commands, immutable employment agreement references, and API evolution through the first release.

## Planned artifacts

Onboarding/activation tests, MVP walkthrough, threat-model start, ADR, and privacy-boundary note.

## Exercises

Make one task conditionally required by policy. Prove repeated activation returns the same employment identity.

## Delivered evidence

- Activation requires an issued contract and a non-past effective start date.
- Employment preserves immutable offer and contract provenance.
- Replaying activation for one offer returns the existing employment.
- The API publishes an additive v1 activation contract through OpenAPI.
- The MVP spans planning, onboarding, approvals, contract issuance, and active employment.
