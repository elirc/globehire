using GlobeHire.Application.Abstractions;
using GlobeHire.Application.WorkforcePlans;
using GlobeHire.Domain.Common;
using GlobeHire.Domain.WorkforcePlans;
using GlobeHire.Infrastructure.WorkforcePlans;

namespace GlobeHire.Application.Tests.WorkforcePlans;

public sealed class ClientCalendarTests
{
    [Fact]
    public async Task Handle_UsEveningAfterUtcMidnight_AcceptsSameDayLocalStart()
    {
        // 21:30 on 15 July in New York is already 16 July in UTC.
        var clock = new FixedClock(new DateTimeOffset(2026, 7, 16, 1, 30, 0, TimeSpan.Zero));
        var handler = new CreateWorkforcePlanHandler(
            new InMemoryWorkforcePlanRepository(),
            clock,
            new FixedIdGenerator(),
            new InMemoryCountryPolicyRepository());

        var result = await handler.HandleAsync(
            new CreateWorkforcePlanCommand(
                "client-acme", "request-evening-01", "Engineer", "US", new DateOnly(2026, 7, 15), 120_000m, "USD"),
            CancellationToken.None);

        Assert.Equal(new DateOnly(2026, 7, 15), result.Plan.ProposedStartDate);
    }

    [Fact]
    public void TodayIn_CountryWithoutConfiguredZone_FallsBackToUtcDate()
    {
        var clock = new FixedClock(new DateTimeOffset(2026, 7, 16, 1, 30, 0, TimeSpan.Zero));

        Assert.Equal(new DateOnly(2026, 7, 16), clock.TodayIn(CountryCode.Parse("DE")));
    }

    private sealed class FixedClock(DateTimeOffset now) : IClock
    {
        public DateTimeOffset UtcNow => now;

        public DateOnly Today => DateOnly.FromDateTime(now.UtcDateTime);
    }

    private sealed class FixedIdGenerator : IIdGenerator
    {
        public Guid NewId() => Guid.Parse("5b0d6c5e-4d8e-4f40-9f5f-6a7c1c9f2a11");
    }
}
