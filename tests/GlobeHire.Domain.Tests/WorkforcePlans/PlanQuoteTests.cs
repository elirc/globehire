using GlobeHire.Domain.Common;
using GlobeHire.Domain.WorkforcePlans;

namespace GlobeHire.Domain.Tests.WorkforcePlans;

public sealed class PlanQuoteTests
{
    [Fact]
    public void Estimate_AnnualCompensation_CalculatesAuditableMonthlyTotal()
    {
        var quote = PlanQuote.Estimate(Money.Of(100_000, "USD"), Money.Of(500, "USD"));

        Assert.Equal(8_333.33m, quote.MonthlySalary.Amount);
        Assert.Equal(500m, quote.MonthlyServiceFee.Amount);
        Assert.Equal(8_833.33m, quote.EstimatedMonthlyTotal.Amount);
    }

    [Fact]
    public void Estimate_NonPositiveCompensation_IsRejected()
    {
        var exception = Assert.Throws<BusinessRuleViolationException>(
            () => PlanQuote.Estimate(Money.Of(0, "USD"), Money.Of(500, "USD")));

        Assert.Equal("compensation.must_be_positive", exception.Code);
    }
}
