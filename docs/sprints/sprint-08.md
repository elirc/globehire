# Sprint 08: Contractor lifecycle

## Goal

Assess classification risk, create a contractor engagement, approve invoices, manage disputes, and convert an engagement into a hire plan.

## Scope

Include versioned assessment questions, reviewer override with reason, invoice state machine, service periods, payment intent port, and conversion mapping. Exclude real payments and legal classification claims.

## Teaching focus

Decision evidence, explainable rules, state-machine composition, invoice idempotency, and translation between bounded contexts.

## Planned artifacts

Assessment/invoice tests, contractor API, payment fake, ADR, and explainable-policy note.

## Exercises

Add a new risk signal without changing historical assessments. Retry payment intent creation safely.
