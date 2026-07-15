using GlobeHire.Domain.Common;

namespace GlobeHire.Domain.WorkforcePlans;

public readonly record struct WorkforcePlanId
{
    private WorkforcePlanId(Guid value) => Value = value;

    public Guid Value { get; }

    public static WorkforcePlanId New(Guid value)
    {
        if (value == Guid.Empty)
        {
            throw new BusinessRuleViolationException("workforce_plan.id_invalid", "Workforce plan ID cannot be empty.");
        }

        return new WorkforcePlanId(value);
    }

    public static bool TryParse(string? value, out WorkforcePlanId id)
    {
        if (Guid.TryParse(value, out var parsed) && parsed != Guid.Empty)
        {
            id = new WorkforcePlanId(parsed);
            return true;
        }

        id = default;
        return false;
    }

    public override string ToString() => Value.ToString("D");
}
