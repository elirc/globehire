# MVP lifecycle and provenance

Activation is a business transition, not creation of an unrelated employee row. An employment can be created only from an offer with an issued contract, and it retains immutable offer and contract IDs.

The activation repository uses the offer as its idempotency boundary. A retried command returns the existing employment instead of creating a second worker relationship. Future persistence must enforce that promise with a unique constraint, not an application-only check.

The v0.5 MVP is deliberately a modular monolith. It proves one coherent lifecycle—plan, hire case, onboarding, approvals, contract, employment—before payroll and external integrations increase operational complexity.
