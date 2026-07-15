# GlobeHire product and curriculum specification

## Product statement

GlobeHire helps a client organization plan, hire, onboard, employ, pay, support, and offboard workers in countries where the client does not operate a legal entity. The platform acts as a fictional employer-of-record operations system and integration hub.

This is an educational domain model, not a legal, tax, payroll, immigration, or compliance product.

## Personas

- **Global HR partner:** initiates hires, chooses compensation and benefits, tracks onboarding, and requests employment changes.
- **Payroll operator:** closes payroll periods, validates inputs, resolves exceptions, and produces statements and client invoices.
- **Compliance specialist:** maintains effective-dated country policies, reviews classification risk, and manages immigration cases.
- **Finance approver:** reviews cost estimates, invoices, expenses, and compensation changes.
- **Worker:** supplies onboarding data, requests leave/expenses, reviews statements, and raises disputes.
- **Integration engineer:** synchronizes HRIS, finance, identity, and provider systems using APIs, webhooks, and bulk files.
- **Support auditor:** reconstructs who decided what, under which policy version, and when.

## Core capabilities

1. Client organizations, legal-employment countries, users, roles, and tenant boundaries.
2. Effective-dated country policy catalogs for notice, probation, leave, employer costs, currencies, and required onboarding steps.
3. Workforce planning and transparent cost estimates with explicit assumptions and policy versions.
4. Hire cases, offers, approval chains, contracts, onboarding tasks, and document metadata.
5. Employment agreements, compensation, benefits, leave, expenses, amendments, and offboarding.
6. Payroll calendars, typed payroll inputs, validation, simplified gross-to-net fixtures, statements, and client invoices.
7. Contractor classification, contracts, invoices, approvals, disputes, and conversion to employment.
8. Immigration cases and compliance-review checkpoints.
9. Search, reporting, audit timelines, bulk import/export, and operational reconciliation.
10. Versioned REST APIs, OpenAPI, idempotency, optimistic concurrency, webhooks, and replaceable integration adapters.

## Domain model

### Tenant and identity

- `ClientOrganization`: tenant root, billing currency, status.
- `User`: external identity reference, tenant memberships.
- `RoleAssignment`: role plus optional worker/country scope.

### Country policy

- `CountryPolicy`: country code, version, effective date range, currency, notice/probation constraints, statutory leave, employer-cost rules, required steps.
- Policies are immutable once used by a quote, contract, or payroll result.

### Planning and employment

- `WorkforcePlan`: client, country, role, start date, compensation proposal.
- `CostEstimate`: immutable money lines, assumptions, policy version, expiry.
- `HireCase`: candidate identity reference, plan, status, approvals, selected benefits, contract, onboarding checklist.
- `Employment`: worker, client, country, agreement, status, compensation history, benefits, lifecycle changes.
- `EmploymentChange`: effective-dated compensation, location, hours, title, or manager amendment.
- `OffboardingCase`: reason, requested/validated end dates, notice calculation, checklist.

### Workforce operations

- `LeaveRequest`: local dates, policy snapshot, units, approval state.
- `ExpenseClaim`: currency-aware items, receipts metadata, approval/reimbursement state.
- `BenefitEnrollment`: plan, coverage dates, worker/employer cost.
- `ImmigrationCase`: case type, country, milestones, document metadata, status.

### Payroll and billing

- `PayrollCalendar`: country/pay group periods and cutoffs.
- `PayrollRun`: period, worker inputs, validation state, calculation version, close state.
- `PayrollInput`: typed earning/deduction/reimbursement with source and idempotency identity.
- `PayStatement`: immutable calculated lines and totals.
- `ClientInvoice`: grouped employment, payroll, benefit, expense, tax, and service-fee lines.

### Contractors

- `ContractorEngagement`: country, services, classification assessment, contract, status.
- `ContractorInvoice`: service period, lines, currency, approval, payment, dispute.
- `ClassificationAssessment`: answers, risk result, policy version, reviewer override.

### Integration and audit

- `AuditEntry`: tenant, actor, action, aggregate identity/version, timestamp, reason, correlation.
- `OutboxMessage`: domain event payload plus stable event type/version.
- `WebhookSubscription`: tenant endpoint, event filters, secret reference, status.
- `WebhookDelivery`: attempt history, response category, retry schedule, idempotency key.
- `ExternalReference`: system, external ID, internal resource, tenant.

## Business invariants

- Money always has an ISO currency; unlike currencies never add implicitly.
- Every decision records the effective policy version used.
- Aggregate transitions are explicit and reject invalid or repeated commands.
- Approved/issued financial and contract artifacts are immutable; changes create new versions.
- Tenant identity is required at every application and repository boundary.
- External commands and deliveries have stable idempotency identities.
- Audit and outbox records commit atomically with business state.
- Local employment dates use `DateOnly`; operational events use UTC `DateTimeOffset`.
- HTTP contracts do not expose domain or persistence types directly.

## API conventions

- Base path `/api/v1`; resource-oriented endpoints with commands only for meaningful state transitions.
- Problem Details for errors with stable machine codes and correlation IDs.
- `Idempotency-Key` for retryable creates and commands with financial/integration effects.
- ETags/expected versions for concurrent aggregate changes.
- Cursor pagination for mutable collections; explicit filter/sort allowlists.
- ISO 8601 dates, UTC timestamps, ISO 3166-1 alpha-2 countries, ISO 4217 currencies.
- OpenAPI generated and snapshot-checked; additive evolution within v1 and explicit versioning for breaking changes.
- Webhook envelopes include event ID, type, schema version, occurred time, tenant, subject, and data.

## Architecture

```text
src/
  GlobeHire.Domain/          framework-free business model
  GlobeHire.Application/     use cases and ports
  GlobeHire.Contracts/       versioned HTTP/integration DTOs
  GlobeHire.Infrastructure/  persistence and provider adapters
  GlobeHire.Api/             ASP.NET Core composition and endpoints
tests/
  GlobeHire.Domain.Tests/
  GlobeHire.Application.Tests/
  GlobeHire.Api.Tests/
  GlobeHire.Architecture.Tests/
  GlobeHire.ContractTests/
```

The solution begins with fewer projects if a boundary has no behavior yet. Projects are added only in the sprint that needs them.

## Testing strategy

- Example and property-oriented domain tests for money, dates, policies, and transitions.
- Application tests with in-memory ports and controlled clock/identity.
- API integration tests using `WebApplicationFactory` and no external server.
- Shared adapter contract suites for repository and integration substitutability.
- Architecture tests for inward dependency rules and contract isolation.
- OpenAPI snapshots and consumer-driven examples for stable integration behavior.
- Fault injection for timeouts, retries, duplicates, concurrency, and partial provider failure.

## Fifteen sprints

| Sprint | Outcome | Core teaching focus |
| --- | --- | --- |
| 01 | One workforce-plan quote through HTTP, domain, and repository | vertical slice, dependency direction, Problem Details |
| 02 | Effective-dated country policies and cost estimation | money, dates, policy versioning, strategy without factories |
| 03 | Hire case and onboarding state machine | aggregates, commands, optimistic concurrency, audit |
| 04 | Offers, approvals, and contract provider port | approval invariants, immutable versions, provider adapters |
| 05 | Worker onboarding and employment activation; `v0.5.0` | PII boundaries, checklists, idempotent activation |
| 06 | Payroll calendars, inputs, calculation, and statements | financial invariants, rounding, cutoff state machine |
| 07 | Leave, expenses, and benefits | effective policy snapshots, approvals, currency conversion boundaries |
| 08 | Contractor lifecycle and invoices | classification policy, disputes, employment conversion |
| 09 | Employment changes and offboarding | effective-dated amendments, notice rules, sagas without a framework |
| 10 | Tenant authorization and scoped access | IDOR prevention, policy-based authorization, repository scope |
| 11 | Webhooks, outbox, and external references | integration contracts, idempotency, retries, schema evolution |
| 12 | Bulk import/export, search, and reporting | reconciliation, cursors, partial success, stable projections |
| 13 | Immigration and compliance cases | long-running workflows, deadlines, document metadata, policy evidence |
| 14 | Hardening and five flaw harvests | counterexamples, security, concurrency, performance, API compatibility |
| 15 | Operations, simulated incident, retrospective, `v1.0.0` | observability, runbooks, restore, SLOs, honest readiness |

## Planned flaw curriculum

Exactly five plausible flaws are privately selected before Sprint 1. They involve money rounding, effective-date boundaries, tenant scoping, approval concurrency, and integration duplicate delivery. Each merges silently, then is exposed later by a real failing counterexample commit and repaired in the next commit. The private ledger is revealed in Sprint 15.

## Out of scope

- real legal/tax advice or production payroll formulas;
- storage of document bytes, bank details, passports, or government identifiers;
- real payments, signatures, emails, immigration submissions, or background checks;
- microservices, Kubernetes, distributed transactions, or multi-region deployment;
- copying any proprietary platform implementation or interface.
