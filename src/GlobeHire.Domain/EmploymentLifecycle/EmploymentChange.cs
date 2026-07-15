using GlobeHire.Domain.Common;

namespace GlobeHire.Domain.EmploymentLifecycle;

public enum EmploymentChangeStatus
{
    Proposed,
    Approved,
    Applied,
    Cancelled,
}

public sealed class EmploymentChange
{
    public EmploymentChange(
        Guid id,
        Guid employmentId,
        string? changeType,
        DateOnly effectiveOn,
        string? reason,
        DateOnly today)
    {
        if (id == Guid.Empty || employmentId == Guid.Empty || string.IsNullOrWhiteSpace(changeType))
        {
            throw new BusinessRuleViolationException("employment_change.invalid", "Employment change is invalid.");
        }

        if (effectiveOn < today)
        {
            throw new BusinessRuleViolationException(
                "employment_change.effective_date_in_past",
                "Employment changes cannot be proposed in the past.");
        }

        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new BusinessRuleViolationException("employment_change.reason_required", "A change reason is required.");
        }

        Id = id;
        EmploymentId = employmentId;
        ChangeType = changeType.Trim();
        EffectiveOn = effectiveOn;
        Reason = reason.Trim();
    }

    public Guid Id { get; }
    public Guid EmploymentId { get; }
    public string ChangeType { get; }
    public DateOnly EffectiveOn { get; }
    public string Reason { get; }
    public EmploymentChangeStatus Status { get; private set; }

    public void Approve() => Transition(EmploymentChangeStatus.Proposed, EmploymentChangeStatus.Approved);

    public void Apply(DateOnly today)
    {
        if (today < EffectiveOn)
        {
            throw new BusinessRuleViolationException(
                "employment_change.not_effective",
                "Change cannot be applied before its effective date.");
        }

        Transition(EmploymentChangeStatus.Approved, EmploymentChangeStatus.Applied);
    }

    public void Cancel(string? reason)
    {
        if (Status is EmploymentChangeStatus.Applied or EmploymentChangeStatus.Cancelled ||
            string.IsNullOrWhiteSpace(reason))
        {
            throw new BusinessRuleViolationException(
                "employment_change.not_cancellable",
                "Only unapplied changes can be cancelled with a reason.");
        }

        Status = EmploymentChangeStatus.Cancelled;
    }

    private void Transition(EmploymentChangeStatus expected, EmploymentChangeStatus next)
    {
        if (Status != expected)
        {
            throw new BusinessRuleViolationException(
                "employment_change.transition_invalid",
                $"Cannot move employment change from {Status} to {next}.");
        }

        Status = next;
    }
}

public sealed class OffboardingChecklist
{
    private readonly HashSet<string> _completed = new(StringComparer.Ordinal);
    private static readonly string[] Required = ["final-pay", "access-revoked", "documents-issued"];

    public void Complete(string task)
    {
        if (!Required.Contains(task, StringComparer.Ordinal))
        {
            throw new BusinessRuleViolationException("offboarding.task_invalid", "Offboarding task is invalid.");
        }

        _completed.Add(task);
    }

    public void EnsureReadyToEnd()
    {
        if (Required.Any(task => !_completed.Contains(task)))
        {
            throw new BusinessRuleViolationException(
                "offboarding.tasks_incomplete",
                "Every required offboarding task must be complete.");
        }
    }
}
