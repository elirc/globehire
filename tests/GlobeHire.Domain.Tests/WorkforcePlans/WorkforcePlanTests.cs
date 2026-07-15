using GlobeHire.Domain.Common;
using GlobeHire.Domain.WorkforcePlans;

namespace GlobeHire.Domain.Tests.WorkforcePlans;

public sealed class WorkforcePlanTests
{
    private static readonly DateOnly Today = new(2026, 7, 14);
    private static readonly DateTimeOffset Now = new(2026, 7, 14, 18, 0, 0, TimeSpan.Zero);

    [Fact]
    public void Create_ValidInputs_NormalizesBoundedContextValues()
    {
        var plan = Create(roleTitle: "  Senior Engineer  ", startDate: Today);

        Assert.Equal("Senior Engineer", plan.RoleTitle);
        Assert.Equal("US", plan.Country.Value);
        Assert.Equal(1, plan.Version);
        Assert.Equal(TimeSpan.Zero, plan.CreatedAt.Offset);
    }

    [Fact]
    public void Create_PastStartDate_IsRejectedAtDomainBoundary()
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(
            () => Create(roleTitle: "Engineer", startDate: Today.AddDays(-1)));

        Assert.Equal("workforce_plan.start_date_in_past", exception.Code);
    }

    [Theory]
    [InlineData("U")]
    [InlineData("USA")]
    [InlineData("1S")]
    public void CountryCode_InvalidValue_IsRejected(string value)
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(() => CountryCode.Parse(value));

        Assert.Equal("country_code.invalid", exception.Code);
    }

    private static WorkforcePlan Create(string roleTitle, DateOnly startDate) =>
        WorkforcePlan.Create(
            WorkforcePlanId.New(Guid.Parse("5cd14e38-6f5e-44fd-9661-2b9769467e27")),
            ClientId.Parse("client-acme"),
            "request-12345678",
            roleTitle,
            CountryCode.Parse("us"),
            startDate,
            Money.Of(120_000, "USD"),
            new CountryEmploymentPolicy(
                "US-test",
                CountryCode.Parse("US"),
                new DateOnly(2026, 1, 1),
                null,
                "USD",
                0.12m,
                500m,
                ["Test fixture"]),
            Today,
            Now);
}
