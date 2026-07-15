using GlobeHire.Domain.Common;
using GlobeHire.Domain.WorkforcePlans;

namespace GlobeHire.Domain.Tests.WorkforcePlans;

public sealed class CountryPolicySelectorTests
{
    [Fact]
    public void Select_OnEffectiveStart_ReturnsMatchingVersion()
    {
        var expected = Policy("US-2027", new DateOnly(2027, 1, 1), null);

        var selected = CountryPolicySelector.Select(
            CountryCode.Parse("US"),
            new DateOnly(2027, 1, 1),
            [Policy("US-2026", new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31)), expected]);

        Assert.Same(expected, selected);
    }

    [Fact]
    public void Select_UnsupportedDate_ReturnsStableBusinessError()
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(() => CountryPolicySelector.Select(
            CountryCode.Parse("US"),
            new DateOnly(2025, 12, 31),
            [Policy("US-2026", new DateOnly(2026, 1, 1), null)]));

        Assert.Equal("country_policy.not_found", exception.Code);
    }

    [Fact]
    public void Select_OverlappingVersions_RejectsAmbiguousPolicy()
    {
        var policies = new[]
        {
            Policy("US-a", new DateOnly(2026, 1, 1), null),
            Policy("US-b", new DateOnly(2026, 6, 1), null),
        };

        var exception = Assert.Throws<BusinessRuleViolationException>(() => CountryPolicySelector.Select(
            CountryCode.Parse("US"),
            new DateOnly(2026, 7, 1),
            policies));

        Assert.Equal("country_policy.overlap", exception.Code);
    }

    private static CountryEmploymentPolicy Policy(string id, DateOnly from, DateOnly? to) =>
        new(id, CountryCode.Parse("US"), from, to, "USD", 0.12m, 500m, ["Test fixture"]);
}
