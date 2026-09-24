# Blind review: GlobeHire `src/` (60 minutes)

You are the reviewer before GlobeHire's first real customer pilot. The product owner's plan is "swap the in-memory repositories for SQL Server, put it behind the API gateway, and go".
Your job is to tell them what else would hurt real clients and real workers.

## Scope

- `src/GlobeHire.Api/` (endpoints, `Program.cs`, the error handler)
- `src/GlobeHire.Application/` (use cases, ports)
- `src/GlobeHire.Domain/` (aggregates, money, policies)
- `src/GlobeHire.Infrastructure/` (in-memory adapters, clock)
- `tests/` (what is actually proven)
- `docs/quality/FLAW-HARVEST.md`, `docs/security/THREAT-MODEL.md`: check their claims against the code.

## Rules

1. Timebox: 60 minutes. Stop even if you're not done.
2. Every finding needs **evidence** (`file:line`) and **what breaks in production** (who is hurt, how you'd notice).
3. Rank by production severity: Critical / High / Medium / Low.
4. Tag each finding with a category: correctness, security, data integrity, concurrency, failure handling, API design, performance, operability, tests, maintainability.
5. Mark anything you *suspect* but didn't confirm as `UNCONFIRMED`. Confirming one finding by writing a failing test earns more than listing three guesses.
6. The known gaps (no authentication, in-memory storage) count only if you say something *specific* about them that the docs don't already say.

## Prompts, in case you get stuck (not a checklist)

- Pick one aggregate and follow a write from the HTTP route to the object in memory. Is the version check atomic with the write?
- `FLAW-HARVEST.md` says one flaw was fixed in Sprint 14. Was it fixed everywhere the same pattern appears?
- What does each `POST` do when the client retries it? What if the retry has a different body?
- Which inputs does the server trust that the caller controls?
- Which endpoints look like features but persist nothing?

## Deliverable

Write `training/review/MY_FINDINGS.md` as a table:
`# | Severity | Category | Finding | Evidence | Production impact | Confirmed?`

Then fill in [COMPARE.md](COMPARE.md) against the sealed model review in `training/_answers/review.md`.
