using GlobeHire.Application.Abstractions;
using GlobeHire.Application.HireCases;
using GlobeHire.Domain.Common;
using GlobeHire.Domain.HireCases;
using GlobeHire.Domain.WorkforcePlans;
using GlobeHire.Infrastructure.HireCases;

namespace GlobeHire.Application.Tests.HireCases;

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
            "identity",
            hireCase.Version,
            CancellationToken.None);

        var exception = await Assert.ThrowsAsync<BusinessRuleViolationException>(() =>
            handler.CompleteTaskAsync(
                "client-acme",
                hireCase.Id,
                "work-eligibility",
                1,
                CancellationToken.None));

        Assert.Equal("concurrency.version_mismatch", exception.Code);
        var unchanged = Assert.Single(hireCase.Tasks, task => task.Code == "work-eligibility");
        Assert.False(unchanged.IsComplete);
        Assert.Null(unchanged.CompletedAt);
        Assert.Equal(2, hireCase.Version);
    }

    private static async Task<(ChangeHireCaseHandler Handler, HireCase HireCase)> CreateHandlerAsync(
        bool completedChecklist = false)
    {
        var repository = new InMemoryHireCaseRepository();
        var hireCase = HireCase.Open(
            Guid.Parse("d9d59414-c677-49fb-a9b5-31243fa3ec11"),
            ClientId.Parse("client-acme"),
            WorkforcePlanId.New(Guid.Parse("da73173f-8e2a-4184-98f5-cb5aa98b22ec")),
            "Ada Lovelace",
            "ada@example.com",
            new DateTimeOffset(2026, 9, 18, 12, 0, 0, TimeSpan.Zero));

        if (completedChecklist)
        {
            hireCase.CompleteTask("identity", 1, CompletionTime.AddMinutes(-3));
            hireCase.CompleteTask("work-eligibility", 2, CompletionTime.AddMinutes(-2));
            hireCase.CompleteTask("payment-details", 3, CompletionTime.AddMinutes(-1));
        }

        await repository.AddAsync(hireCase, CancellationToken.None);
        return (new ChangeHireCaseHandler(repository, new FixedClock()), hireCase);
    }

    private sealed class FixedClock : IClock
    {
        public DateTimeOffset UtcNow => CompletionTime;

        public DateOnly Today => DateOnly.FromDateTime(CompletionTime.UtcDateTime);
    }
}
