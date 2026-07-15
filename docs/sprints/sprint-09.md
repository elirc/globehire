# Sprint 09: Employment changes and offboarding

## Goal

Schedule effective-dated employment amendments and compute a policy-supported offboarding plan.

## Scope

Include compensation/location/title changes, overlapping-change rejection, notice calculations, end-date validation, checklist orchestration, and cancellation. Exclude real termination advice and provider filings.

## Teaching focus

Temporal aggregates, future commands, long-running orchestration without a saga framework, compensating actions, and audit reasons.

## Planned artifacts

Timeline tests, amendment/offboarding APIs, ADR, and temporal-workflow note.

## Exercises

## Delivered evidence

- Employment changes preserve type, reason, effective date, and transition status.
- Approved changes cannot apply before their effective date.
- Applied changes cannot be cancelled or rewritten.
- Offboarding readiness requires final pay, access revocation, and document evidence.
- An API validation contract exposes the temporal rules to integrators.

## Exercises

Resolve two future amendments that target the same property. Design cancellation after one external step completed.
