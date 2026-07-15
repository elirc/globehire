# Fifteen-sprint retrospective

## What changed well

The architecture grew from one quote slice into hiring, offers, employment, payroll, people operations, contractors, lifecycle changes, integrations, bulk work, and compliance without moving business rules into controllers or binding the domain to storage. Typed tenant IDs, policy snapshots, idempotency identities, and state machines became stable integration concepts rather than framework conventions.

The five planted flaws were valuable because each counterexample changed a contract: exact money conservation, half-open effective dates, tenant-bound lookup, atomic versioned mutation, and stable retry identity. The fixes stayed local and produced tests that explain the risk.

## What was intentionally expensive

The issue, milestone, commit, PR, and review trail is much more ceremony than this codebase needs. It turns delivery history into course material. The source-project split also arrived earlier than many small products would need; [the over-engineering review](../../OEReadMe.md) identifies where to simplify without discarding important rules.

## What remains

A useful next exercise is a PostgreSQL adapter that passes the same use cases while atomically persisting aggregate state, audit evidence, and outbox messages. Follow it with authenticated tenant claims, a real webhook worker and contract test fixture, then load and restore drills. Do not implement country/payroll policy for real use without qualified legal, tax, payroll, and privacy owners.

## Questions for the learner

1. Which boundary would change first for a second HRIS provider, and which code should remain untouched?
2. Where must a transaction begin and end when state and an outbox message change together?
3. Which project boundaries still pay rent, and which would you merge on a three-person team?
4. What production evidence would turn each candidate SLO into a defensible target?
5. Which regression test would you keep if you could retain only one per harvested flaw?

