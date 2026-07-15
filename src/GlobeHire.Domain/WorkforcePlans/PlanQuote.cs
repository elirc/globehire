using GlobeHire.Domain.Common;

namespace GlobeHire.Domain.WorkforcePlans;

public sealed record PlanQuote
{
    private PlanQuote(Money monthlySalary, Money monthlyServiceFee, Money estimatedMonthlyTotal)
    {
        MonthlySalary = monthlySalary;
        MonthlyServiceFee = monthlyServiceFee;
        EstimatedMonthlyTotal = estimatedMonthlyTotal;
    }

    public Money MonthlySalary { get; }

    public Money MonthlyServiceFee { get; }

    public Money EstimatedMonthlyTotal { get; }

    public static PlanQuote Estimate(Money annualCompensation, Money monthlyServiceFee)
    {
        if (annualCompensation.Amount <= 0)
        {
            throw new BusinessRuleViolationException(
                "compensation.must_be_positive",
                "Annual compensation must be greater than zero.");
        }

        if (monthlyServiceFee.Amount < 0)
        {
            throw new BusinessRuleViolationException(
                "quote.service_fee_negative",
                "Monthly service fee cannot be negative.");
        }

        var monthlySalary = annualCompensation.Divide(12);
        var monthlyTotal = monthlySalary.Add(monthlyServiceFee);
        return new PlanQuote(monthlySalary, monthlyServiceFee, monthlyTotal);
    }
}
