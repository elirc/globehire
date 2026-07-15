namespace GlobeHire.Application.WorkforcePlans;

public sealed class WorkforcePlanPricingOptions
{
    public const string SectionName = "WorkforcePlanPricing";

    public decimal MonthlyServiceFee { get; init; } = 500m;
}
