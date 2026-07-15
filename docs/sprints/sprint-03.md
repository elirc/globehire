# Sprint 03: Hire cases and onboarding workflow

## Goal

Turn an accepted estimate into a hire case with explicit lifecycle transitions and a country-derived onboarding checklist.

## Scope

Include aggregate commands, status history, expected-version concurrency, actor/reason audit entries, checklist generation, and idempotent creation. Exclude contract issuance and document bytes.

## Teaching focus

Aggregate boundaries, state machines, command versus CRUD semantics, optimistic concurrency, and atomic audit behavior.

## Planned artifacts

Transition table tests, hire-case endpoints, audit contract, ADR, and aggregate design note.

## Exercises

Add a cancelled-to-reopened policy. Explain which transitions belong in the aggregate and which belong in orchestration.
