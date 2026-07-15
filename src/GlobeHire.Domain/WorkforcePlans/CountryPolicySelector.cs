using GlobeHire.Domain.Common;

namespace GlobeHire.Domain.WorkforcePlans;

public static class CountryPolicySelector
{
    public static CountryEmploymentPolicy Select(
        CountryCode country,
        DateOnly effectiveDate,
        IEnumerable<CountryEmploymentPolicy> policies)
    {
        ArgumentNullException.ThrowIfNull(policies);
        var matches = policies
            .Where(policy => policy.Country == country && policy.AppliesOn(effectiveDate))
            .OrderByDescending(policy => policy.EffectiveFrom)
            .ToArray();

        return matches.Length switch
        {
            0 => throw new BusinessRuleViolationException(
                "country_policy.not_found",
                $"No {country} employment policy applies on {effectiveDate:yyyy-MM-dd}."),
            1 => matches[0],
            _ => throw new BusinessRuleViolationException(
                "country_policy.overlap",
                $"Multiple {country} employment policies apply on {effectiveDate:yyyy-MM-dd}."),
        };
    }
}
