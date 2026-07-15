using GlobeHire.Domain.Common;
using GlobeHire.Domain.EmploymentLifecycle;

namespace GlobeHire.Domain.Tests.EmploymentLifecycle;

public sealed class EmploymentChangeTests
{
    [Fact]
    public void Apply_BeforeEffectiveDate_IsRejected()
    {
        var change = Change();
        change.Approve();

        var error = Assert.Throws<BusinessRuleViolationException>(
            () => change.Apply(new DateOnly(2026, 7, 31)));

        Assert.Equal("employment_change.not_effective", error.Code);
        Assert.Equal(EmploymentChangeStatus.Approved, change.Status);
    }

    [Fact]
    public void ApprovedChange_OnEffectiveDate_CanBeApplied()
    {
        var change = Change();
        change.Approve();

        change.Apply(new DateOnly(2026, 8, 1));

        Assert.Equal(EmploymentChangeStatus.Applied, change.Status);
    }

    [Fact]
    public void Offboarding_MissingTask_IsRejected()
    {
        var checklist = new OffboardingChecklist();
        checklist.Complete("final-pay");
        checklist.Complete("access-revoked");

        var error = Assert.Throws<BusinessRuleViolationException>(checklist.EnsureReadyToEnd);

        Assert.Equal("offboarding.tasks_incomplete", error.Code);
    }

    private static EmploymentChange Change() =>
        new(
            Guid.NewGuid(),
            Guid.NewGuid(),
            "compensation",
            new DateOnly(2026, 8, 1),
            "Annual review",
            new DateOnly(2026, 7, 15));
}
