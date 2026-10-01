# Solutions and review rubric

For the prediction matrix, an unknown Guid and the wrong tenant both return null from the repository and therefore make no state change. Repeating an already-complete task is subtler. CompleteTask checks the expected version first. With the current version it finds the completed task and returns without another increment or timestamp replacement. With an old version it throws before reaching the idempotent branch. This ordering deliberately distinguishes a valid replay based on current state from a stale command.

The route-to-key trace should name HireCaseEndpoints, ChangeHireCaseHandler, ClientId.Parse, IHireCaseRepository.FindAsync, and the dictionary pair in InMemoryHireCaseRepository. A Guid’s uniqueness does not establish authority. Even unguessable identifiers can leak through logs, links, or other responses, and future storage may use predictable keys. Keeping client scope in the repository contract makes an unsafe lookup harder to express.

A durable design should derive tenant identity from trusted server context, then query with both client and case ID. The update should include the observed version or use a concurrency token so a competing change cannot be silently overwritten. Authentication, membership, query scoping, and optimistic concurrency are separate controls; the current teaching app fully demonstrates only repository scoping and in-process aggregate version checks.

Review submissions with four criteria. First, negative tests must inspect unchanged state, not only null or HTTP 404. Second, deterministic clocks must produce an exact UTC timestamp. Third, stale-version coverage must prove the untouched task and unchanged version. Fourth, explanations must state limits: singleton memory storage, no verified principal, and no distributed transaction claim. A strong solution preserves existing production code when evidence shows it is correct. Adding a second unscoped repository method, relying on random IDs, or using broad timing windows should fail review.

## Source excerpt

From [tests/GlobeHire.Domain.Tests/HireCases/HireCaseTests.cs](../tests/GlobeHire.Domain.Tests/HireCases/HireCaseTests.cs).

```cs
public sealed class HireCaseTests
{
    [Fact]
    public void Submit_IncompleteChecklist_IsRejected()
    {
        var hireCase = Create();

        var exception = Assert.Throws<BusinessRuleViolationException>(() => hireCase.SubmitForReview(1));

        Assert.Equal("hire_case.tasks_incomplete", exception.Code);
        Assert.Equal(HireCaseStatus.Draft, hireCase.Status);
    }

    [Fact]
    public void CompleteAllThenSubmit_AdvancesStateAndVersion()
    {
        var hireCase = Create();
        hireCase.CompleteTask("identity", 1, DateTimeOffset.UtcNow);
        hireCase.CompleteTask("work-eligibility", 2, DateTimeOffset.UtcNow);
        hireCase.CompleteTask("payment-details", 3, DateTimeOffset.UtcNow);

        hireCase.SubmitForReview(4);

        Assert.Equal(HireCaseStatus.ReadyForReview, hireCase.Status);
        Assert.Equal(5, hireCase.Version);
    }

    [Fact]
    public void CompleteTask_StaleVersion_IsRejectedBeforeMutation()
    {
        var hireCase = Create();
        hireCase.CompleteTask("identity", 1, DateTimeOffset.UtcNow);

        var exception = Assert.Throws<BusinessRuleViolationException>(
            () => hireCase.CompleteTask("work-eligibility", 1, DateTimeOffset.UtcNow));

        Assert.Equal("concurrency.version_mismatch", exception.Code);
        Assert.False(hireCase.Tasks.Single(task => task.Code == "work-eligibility").IsComplete);
    }

    private static HireCase Create() =>
        HireCase.Open(
            Guid.NewGuid(),
            ClientId.Parse("client-acme"),
            WorkforcePlanId.New(Guid.NewGuid()),
            "Ada Lovelace",
            "ada@example.com",
            DateTimeOffset.UtcNow);
}
```

## Course navigation

[README](README.md) / [01-CODEBASE-MAP](01-CODEBASE-MAP.md) / [02-CONCEPTS](02-CONCEPTS.md) / [03-WORKED-CHANGE](03-WORKED-CHANGE.md) / [04-TESTING-AND-DEBUGGING](04-TESTING-AND-DEBUGGING.md) / [05-PRACTICE](05-PRACTICE.md) / [06-SOLUTIONS-AND-REVIEW](06-SOLUTIONS-AND-REVIEW.md) / [07-TRACE-LAB](07-TRACE-LAB.md) / [VERIFICATION](VERIFICATION.md)
