using GlobeHire.Domain.Common;

namespace GlobeHire.Domain.WorkforcePlans;

public sealed record CountryEmploymentPolicy
{
    public CountryEmploymentPolicy(
        string id,
        CountryCode country,
        DateOnly effectiveFrom,
        DateOnly? effectiveTo,
        string currency,
        decimal employerContributionRate,
        decimal monthlyServiceFee,
        IReadOnlyList<string> assumptions)
    {
        if (string.IsNullOrWhiteSpace(id))
        {
            throw new BusinessRuleViolationException("country_policy.id_required", "Policy ID is required.");
        }

        if (effectiveTo <= effectiveFrom)
        {
            throw new BusinessRuleViolationException(
                "country_policy.date_range_invalid",
                "Policy exclusive end date must follow its start date.");
        }

        if (employerContributionRate is < 0 or > 1)
        {
            throw new BusinessRuleViolationException(
                "country_policy.contribution_rate_invalid",
                "Employer contribution rate must be between zero and one.");
        }

        if (monthlyServiceFee < 0)
        {
            throw new BusinessRuleViolationException(
                "country_policy.service_fee_invalid",
                "Monthly service fee cannot be negative.");
        }

        Id = id.Trim();
        Country = country;
        EffectiveFrom = effectiveFrom;
        EffectiveTo = effectiveTo;
        Currency = Money.Of(0, currency).Currency;
        EmployerContributionRate = employerContributionRate;
        MonthlyServiceFee = monthlyServiceFee;
        Assumptions = assumptions;
    }

    public string Id { get; }

    public CountryCode Country { get; }

    public DateOnly EffectiveFrom { get; }

    public DateOnly? EffectiveTo { get; }

    public string Currency { get; }

    public decimal EmployerContributionRate { get; }

    public decimal MonthlyServiceFee { get; }

    public IReadOnlyList<string> Assumptions { get; }

    public bool AppliesOn(DateOnly date) =>
        date >= EffectiveFrom && (EffectiveTo is null || date < EffectiveTo.Value);
}
