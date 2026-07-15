# Testing guide for a growing .NET system

GlobeHire uses tests to make business decisions and integration promises safe to change. The goal is not maximum line coverage. A useful test explains a risk, fails for the right reason, and gives a maintainer enough confidence to refactor.

## The test portfolio

| Suite | What it protects | Examples |
| --- | --- | --- |
| Domain unit tests | Dense rules with no framework or I/O | rounding, currency mismatch, quote totals, start-date invariants |
| Application tests | Use-case orchestration across ports | idempotent replay, tenant scoping, deterministic time and IDs |
| API integration tests | Behavior visible to another system | status codes, headers, Problem Details, JSON contracts, OpenAPI paths |
| Architecture tests | A few critical dependency constraints | domain independence, contract independence, inward dependencies |

Most business permutations belong in fast domain tests. Do not repeat every one through HTTP. API tests should sample the wiring and public contract risks that unit tests cannot see.

## Good testing habits

1. Name the behavior and outcome. `Handle_SameTenantAndKey_ReturnsOriginalPlanWithoutDuplicate` tells a future reader why the test exists.
2. Arrange only what matters. Fixed clocks and queued IDs expose nondeterminism instead of hiding it behind arbitrary waits.
3. Assert observable behavior. Avoid checking private methods or reproducing the implementation inside the test.
4. Test boundaries for failures as well as success. Stable error codes are safer integration contracts than matching prose.
5. Make tenant isolation explicit. Multi-tenant lookup tests should attempt the cross-tenant access that must fail.
6. Keep tests deterministic. Never depend on test order, the machine's local timezone, live external services, or random values whose value affects the assertion.

## Common traps

- **Mocking every class:** this freezes implementation structure and can let broken wiring pass. Prefer real domain objects and small in-memory adapters.
- **Only happy-path HTTP tests:** serialization, error mapping, idempotency, and authorization failures are where integrations often break.
- **Chasing 100% coverage:** trivial getters can inflate the number while important state transitions remain untested.
- **Huge end-to-end suites:** they are slow to diagnose. Use the lowest test level that can prove the risk.
- **Snapshotting everything:** snapshots can conceal meaningful contract changes in large diffs. Use focused assertions, and reserve snapshots for deliberately reviewed schemas.

## Run the tests

```powershell
dotnet restore GlobeHire.slnx --locked-mode
dotnet test GlobeHire.slnx --configuration Release
```

Before opening a pull request, also run formatting and dependency checks:

```powershell
dotnet format GlobeHire.slnx --verify-no-changes --no-restore
dotnet list GlobeHire.slnx package --vulnerable --include-transitive
```

When adding a feature, start by writing the smallest test that expresses the new rule or public promise. Then implement it, refactor while green, and add a higher-level test only if another boundary introduces a distinct risk.
