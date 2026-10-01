# Verification record and limits

Verification is intentionally narrow and reproducible. The changed behavior is a new application regression file; production GlobeHire source was not modified because the audited tenant-aware repository and handler were already correct. The test project composes the actual InMemoryHireCaseRepository, ChangeHireCaseHandler, HireCase aggregate, ClientId and WorkforcePlanId value objects, plus a deterministic IClock.

The acceptance cases are: cross-client task completion returns null and preserves all checklist flags, timestamps, and Version; cross-client submission returns null and preserves Draft plus Version even when the checklist would permit submission; an owner completion records the exact UTC clock and increments once; and a stale owner mutation reports `concurrency.version_mismatch` while preserving the target task and current Version. These assertions cover result and state together.

The recorded command uses the repository’s .NET 10 target and the installed 10.0.302 SDK. Single-worker MSBuild and disabled build-server reuse reduce memory pressure. The command restores and builds the targeted Application.Tests dependency graph, then runs eight tests. Evidence files copied beside this chapter contain the exact argv, timestamps, exit code, and full log.

Limits are explicit. This proves in-process behavior of the singleton in-memory repository. It does not prove authentication, authorization membership, a durable transaction, row-level database isolation, multiple-node concurrency, or production policy correctness. ClientId still originates from request data in this teaching API. The fixed clock verifies propagation, not a real time provider. The broader API suite was not rerun because the accepted change only adds application tests and the targeted project already builds the involved source projects. Future durable implementations should reuse the same behavior cases and add storage-specific concurrency tests.

## Source excerpt

From [global.json](../global.json).

```json
{
  "sdk": {
    "version": "10.0.302",
    "rollForward": "latestPatch",
    "allowPrerelease": false
  }
}
```

## Course navigation

[README](README.md) / [01-CODEBASE-MAP](01-CODEBASE-MAP.md) / [02-CONCEPTS](02-CONCEPTS.md) / [03-WORKED-CHANGE](03-WORKED-CHANGE.md) / [04-TESTING-AND-DEBUGGING](04-TESTING-AND-DEBUGGING.md) / [05-PRACTICE](05-PRACTICE.md) / [06-SOLUTIONS-AND-REVIEW](06-SOLUTIONS-AND-REVIEW.md) / [07-TRACE-LAB](07-TRACE-LAB.md) / [VERIFICATION](VERIFICATION.md)

## Recorded command evidence

The recorded test runs passed **8 tests**. The commands below define the verified scope; counts refer to tests, not commands.

| Check | Exact command arguments | Exit | Evidence |
|---|---|---:|---|
| `sol15-globehire-app-tests-r1` | `["dotnet", "test", "tests\\GlobeHire.Application.Tests\\GlobeHire.Application.Tests.csproj", "--disable-build-servers", "-m:1"]` | 0 | [record](evidence/sol15-globehire-app-tests-r1.json), [log](evidence/sol15-globehire-app-tests-r1.log) |
