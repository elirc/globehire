using GlobeHire.Application.WorkforcePlans;
using GlobeHire.Domain.Common;
using GlobeHire.Domain.WorkforcePlans;

namespace GlobeHire.Infrastructure.WorkforcePlans;

public sealed class InMemoryCountryPolicyRepository : ICountryPolicyRepository
{
    private static readonly IReadOnlyList<CountryEmploymentPolicy> Policies =
    [
        new(
            "US-2026.1",
            CountryCode.Parse("US"),
            new DateOnly(2026, 1, 1),
            null,
            "USD",
            0.12m,
            500m,
            ["Fictional 12% employer contribution fixture", "Excludes variable benefits and one-time costs"]),
        new(
            "GB-2026.1",
            CountryCode.Parse("GB"),
            new DateOnly(2026, 1, 1),
            null,
            "GBP",
            0.15m,
            450m,
            ["Fictional 15% employer contribution fixture", "Excludes pension elections and one-time costs"]),
    ];

    public Task<IReadOnlyList<CountryEmploymentPolicy>> ListAsync(
        CountryCode country,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        IReadOnlyList<CountryEmploymentPolicy> matches = Policies.Where(policy => policy.Country == country).ToArray();
        return Task.FromResult(matches);
    }
}
