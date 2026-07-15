using GlobeHire.Domain.Common;
using GlobeHire.Domain.WorkforcePlans;

namespace GlobeHire.Domain.Tests.WorkforcePlans;

public sealed class PlanQuoteTests
{
    [Fact]
    public void Estimate_AnnualCompensation_CalculatesAuditableMonthlyTotal()
    {
        var quote = PlanQuote.Estimate(Money.Of(100_000, "USD"), Policy());

        Assert.Equal(8_333.33m, quote.MonthlySalary.Amount);
        Assert.Equal(1_000m, quote.MonthlyEmployerContributions.Amount);
        Assert.Equal(500m, quote.MonthlyServiceFee.Amount);
        Assert.Equal(9_833.33m, quote.EstimatedMonthlyTotal.Amount);
    }

    [Fact]
    public void Estimate_NonPositiveCompensation_IsRejected()
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(
            () => PlanQuote.Estimate(Money.Of(0, "USD"), Policy()));

        Assert.Equal("compensation.must_be_positive", exception.Code);
    }

    private static CountryEmploymentPolicy Policy() =>
        new(
            "US-test",
            CountryCode.Parse("US"),
            new DateOnly(2026, 1, 1),
            null,
            "USD",
            0.12m,
            500m,
            ["Test fixture"]);
}
