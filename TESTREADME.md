# Why GlobeHire has tests

Tests were added so a junior software engineer can change complicated business logic without relying on hope or manual clicking. In workforce software, a one-cent rounding error, a date boundary, a cross-tenant lookup, a duplicate retry, or a lost concurrent update can have serious consequences. A good test names that risk and proves the behavior another developer may safely depend on.

The goal is not 100% line coverage. The goal is useful evidence: a test should fail for the right reason, remain deterministic, and give a maintainer confidence to refactor.

## The test portfolio

| Suite | What it protects | Example risks |
| --- | --- | --- |
| Domain unit tests | Dense rules without framework or I/O | exact money allocation, effective dates, invalid transitions, stable retry identity |
| Application tests | Use-case orchestration across ports | idempotent replay, controlled time/IDs, atomic expected-version mutation |
| API integration tests | Behavior another system observes | routes, JSON, Problem Details, headers, OpenAPI, health, tenant information hiding |
| Architecture tests | A few expensive-to-break dependency rules | framework-free domain, isolated contracts, inward project references |

As of October 2026 the suite holds 62 test cases: Domain 43 (15 files), Application 8, API 8, Architecture 3. Count them yourself with `git grep -c "\[Fact\]\|\[InlineData" -- tests`; each `[InlineData]` row is one case.

Most permutations belong in fast domain tests. Do not repeat every rule through HTTP. Use an API test when routing, serialization, middleware, dependency wiring, or a public compatibility promise creates a distinct risk.

## A junior-friendly workflow

1. Describe the behavior in the test name: `Retry_PreservesDeliveryIdentityAndIncrementsAttempt` is more useful than `RetryTest`.
2. Arrange only relevant facts. Use fixed dates, fixed clocks, and known identifiers; avoid arbitrary delays and random values that affect assertions.
3. Act once when possible. A transition test is easiest to understand when one command causes one visible outcome.
4. Assert public behavior: returned value, state, emitted identity, error code, or persisted version. Do not test private methods.
5. Make the test fail before implementing a bug fix. A red counterexample proves that the test can detect the defect.
6. Implement the smallest correction, run the focused test, then the suite.
7. Refactor while green. If the test prevents an internal rename that does not change behavior, it is coupled too tightly.

This is Arrange–Act–Assert in practical terms, but clarity matters more than comments that label the sections.

## What the regressions teach

- Money allocation asserts that every cent is conserved, not merely that individual results look plausible.
- Effective policies use half-open ranges so adjacent versions cannot both own the boundary day.
- Tenant tests attempt access using a real foreign resource ID and expect `404`; a happy-path test cannot prove isolation.
- Concurrency starts two writers against one expected version and proves exactly one commits (`OfferConcurrencyTests`). That test covers offers only: hire-case handlers still mutate the aggregate after `FindAsync` releases its lock, and no test races two hire-case writers.
- Webhook retry retains stable delivery/message IDs, because a new ID defeats consumer deduplication.
- Health tests verify both operational status and the honest declaration that current storage is not durable.

## Common traps

- **Mock every class:** this freezes implementation structure and can let broken wiring pass. Prefer real domain objects and small in-memory ports.
- **Only test happy paths:** failures, boundaries, retries, and conflicting writers carry much of the business risk.
- **Copy implementation logic into the assertion:** two copies can share the same mistake. Assert invariants and independently known examples.
- **Depend on test order, local time, or a live service:** the suite becomes flaky and teaches the team to ignore failures.
- **Chase a coverage percentage:** trivial lines can inflate a score while an important transition has no counterexample.
- **Build one huge end-to-end suite:** failures become slow and hard to locate. Use the lowest layer that can prove the risk.
- **Snapshot everything:** large snapshots hide meaningful compatibility changes. Prefer focused assertions unless the whole schema is deliberately under review.

## Add a test for a new change

Ask: “What could be wrong even if the code compiles?” Put the first test at the layer that owns that answer. Add a higher-level test only for a new boundary—for example, a domain test proves a rule code and one API test proves that the code becomes Problem Details. Include at least one boundary or invalid example for non-trivial business logic.

When a test fails in CI, read the first meaningful failure, reproduce it with the focused project or class, and do not solve nondeterminism by adding retries. Fix the product, fixture, or test boundary that made the result unreliable.

## Commands

```powershell
dotnet restore GlobeHire.slnx --locked-mode
dotnet test GlobeHire.slnx --configuration Release
dotnet test tests/GlobeHire.Domain.Tests --filter FullyQualifiedName~MoneyAllocator
```

Before a pull request, CI also verifies formatting, a Release build, transitive dependency vulnerabilities, and secrets. Run these locally when the platform's line-ending behavior matches CI:

```powershell
dotnet format GlobeHire.slnx --verify-no-changes --no-restore
dotnet list GlobeHire.slnx package --vulnerable --include-transitive
```

Treat every test as maintained production code: readable names, little duplication, no hidden network, and a clear reason to exist.
