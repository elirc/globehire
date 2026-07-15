using GlobeHire.Domain.Common;
using GlobeHire.Domain.HireCases;
using GlobeHire.Domain.WorkforcePlans;

namespace GlobeHire.Domain.Tests.HireCases;

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
