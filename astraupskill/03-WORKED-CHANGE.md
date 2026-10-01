# Worked change: prove isolation without rewriting it

The worked change begins with a fact from current source: IHireCaseRepository.FindAsync requires ClientId and Guid, and the in-memory implementation stores the same pair. Because the production path is already correct, changing it would create needless risk. The useful improvement is to make that contract executable at the application seam.

Create an owned aggregate with deterministic IDs and add it to the real InMemoryHireCaseRepository. Construct ChangeHireCaseHandler with that repository and a fixed clock. First call CompleteTaskAsync with `client-other`, the owned Guid, task `identity`, and version 1. The expected return is null. Then inspect the original owner aggregate: Version remains 1, every IsComplete flag remains false, and every CompletedAt remains null. This checks absence of mutation rather than merely checking the outward response.

For submission, prepare an owner case whose three tasks are complete but whose Status is still Draft. Capture its current version, call SubmitAsync as the other client, and verify null, Draft, and the unchanged version. This fixture is valuable because submission would otherwise be a legal transition; failure therefore comes from tenant lookup rather than an incomplete checklist.

The success case calls completion as `client-acme`. Assert that the returned object is the stored aggregate, only `identity` is complete, its CompletedAt equals the fixed UTC instant, and Version is 2. Finally, complete identity and attempt work-eligibility with stale version 1. Assert the named business error and unchanged second task. Together the four cases form a small matrix: wrong identity with valid version, wrong identity with otherwise valid submit, correct identity and version, and correct identity with stale version. That matrix is the worked artifact.

## Source excerpt

From [tests/GlobeHire.Application.Tests/HireCases/HireCaseTenantIsolationTests.cs](../tests/GlobeHire.Application.Tests/HireCases/HireCaseTenantIsolationTests.cs).

```cs
public sealed class HireCaseTenantIsolationTests
{
    private static readonly DateTimeOffset CompletionTime =
        new(2026, 9, 19, 17, 30, 0, TimeSpan.Zero);

    [Fact]
    public async Task CompleteTask_DifferentClient_ReturnsNotFoundAndPreservesOwnerState()
    {
        var (handler, hireCase) = await CreateHandlerAsync();

        var result = await handler.CompleteTaskAsync(
            "client-other",
            hireCase.Id,
            "identity",
            hireCase.Version,
            CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(1, hireCase.Version);
        Assert.All(hireCase.Tasks, task =>
        {
            Assert.False(task.IsComplete);
            Assert.Null(task.CompletedAt);
        });
    }

    [Fact]
    public async Task Submit_DifferentClient_ReturnsNotFoundAndPreservesOwnerState()
    {
        var (handler, hireCase) = await CreateHandlerAsync(completedChecklist: true);
        var versionBeforeAttempt = hireCase.Version;

        var result = await handler.SubmitAsync(
            "client-other",
            hireCase.Id,
            versionBeforeAttempt,
            CancellationToken.None);

        Assert.Null(result);
        Assert.Equal(HireCaseStatus.Draft, hireCase.Status);
        Assert.Equal(versionBeforeAttempt, hireCase.Version);
    }

    [Fact]
    public async Task CompleteTask_OwningClient_RecordsClockAndIncrementsVersionOnce()
    {
        var (handler, hireCase) = await CreateHandlerAsync();

        var result = await handler.CompleteTaskAsync(
            "client-acme",
            hireCase.Id,
            "identity",
            hireCase.Version,
            CancellationToken.None);

        Assert.Same(hireCase, result);
        var completed = Assert.Single(hireCase.Tasks, task => task.Code == "identity");
        Assert.True(completed.IsComplete);
        Assert.Equal(CompletionTime, completed.CompletedAt);
        Assert.Equal(TimeSpan.Zero, completed.CompletedAt?.Offset);
        Assert.Equal(2, hireCase.Version);
    }

    [Fact]
    public async Task CompleteTask_StaleOwnerVersion_PreservesUnchangedTask()
    {
        var (handler, hireCase) = await CreateHandlerAsync();
        await handler.CompleteTaskAsync(
            "client-acme",
            hireCase.Id,
```

## Course navigation

[README](README.md) / [01-CODEBASE-MAP](01-CODEBASE-MAP.md) / [02-CONCEPTS](02-CONCEPTS.md) / [03-WORKED-CHANGE](03-WORKED-CHANGE.md) / [04-TESTING-AND-DEBUGGING](04-TESTING-AND-DEBUGGING.md) / [05-PRACTICE](05-PRACTICE.md) / [06-SOLUTIONS-AND-REVIEW](06-SOLUTIONS-AND-REVIEW.md) / [07-TRACE-LAB](07-TRACE-LAB.md) / [VERIFICATION](VERIFICATION.md)
