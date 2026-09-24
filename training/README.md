# GlobeHire: B07 apprenticeship drill

Tier: **DRILL** (Phase 3 review + incidents, plus a small interview kit). This folder is lab material only. It does not change production code on `main`.
Bug-seeded code lives only on the two `training/incidents/*` branches.

## Suggested order (about 6.5 hours)

| # | Exercise | Time | File |
|---|---|---|---|
| 1 | Blind code review of `src/` | 60 min + 20 min compare | [review/EXERCISE.md](review/EXERCISE.md), then [review/COMPARE.md](review/COMPARE.md) |
| 2 | Incident 001: approvals attributed to the wrong person | 75 min | [incidents/001-approver-identity.md](incidents/001-approver-identity.md) |
| 3 | Incident 002: US plans accepted with yesterday's start date | 60 min | [incidents/002-start-date-calendar.md](incidents/002-start-date-calendar.md) |
| 4 | Technical questions (answer in writing, then check) | 60 min | [interview/TECHNICAL_QUESTIONS.md](interview/TECHNICAL_QUESTIONS.md) |
| 5 | Write your STAR stories from what you actually did | 45 min | [interview/PROJECT_STORIES.md](interview/PROJECT_STORIES.md) |

Answers are sealed in `_answers/` (one file per exercise). Don't open them until you've written your own answer.

## How to run an incident

```powershell
git -C C:\Users\Owner\Desktop\globehire worktree add ..\gh-inc-001 training/incidents/001-approver-identity
cd ..\gh-inc-001
dotnet test GlobeHire.slnx        # green: the suite does not catch either incident
```

Both incident branches build, and every existing test passes on them. That is the point of the drill: CI was green when each bug shipped.

## Related central pages (read, don't duplicate)

- `C:\Users\Owner\Desktop\opusorganize\apprenticeship\curriculum\05-verify-before-trust.md`: both incidents are "the PR description is true, but the code is not".
- `C:\Users\Owner\Desktop\opusorganize\apprenticeship\curriculum\04-characterization-tests.md`: write the failing test before the fix.
- `C:\Users\Owner\Desktop\opusorganize\apprenticeship\curriculum\13-release-safety.md`: both bugs only show under production conditions (two tenants, a real wall clock).
- `C:\Users\Owner\Desktop\opusorganize\apprenticeship\curriculum\02-idempotency.md` and `03-concurrency-races.md`: background for review findings only. No new exercises here, because those skills are saturated.

## What this repo is good for (and not)

GlobeHire is a clean, small (about 2.9k lines of `src/`) .NET 10 modular monolith with honest docs (ADR-0002 admits in-memory storage and no auth).
Its distinct value is **.NET DI-lifetime depth** and **calendar-date-vs-instant correctness**. Tenant and IDOR work is already covered elsewhere in the archive.
