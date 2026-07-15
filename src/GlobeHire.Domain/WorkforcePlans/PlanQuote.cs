using GlobeHire.Domain.Common;

namespace GlobeHire.Domain.WorkforcePlans;

public sealed record PlanQuote
{
    private PlanQuote(
        string policyId,
        Money monthlySalary,
        Money monthlyEmployerContributions,
        Money monthlyServiceFee,
        Money estimatedMonthlyTotal,
        IReadOnlyList<string> assumptions)
    {
        PolicyId = policyId;
        MonthlySalary = monthlySalary;
        MonthlyEmployerContributions = monthlyEmployerContributions;
        MonthlyServiceFee = monthlyServiceFee;
        EstimatedMonthlyTotal = estimatedMonthlyTotal;
        Assumptions = assumptions;
    }

    public string PolicyId { get; }

    public Money MonthlySalary { get; }

    public Money MonthlyEmployerContributions { get; }

    public Money MonthlyServiceFee { get; }

    public Money EstimatedMonthlyTotal { get; }

    public IReadOnlyList<string> Assumptions { get; }

    public static PlanQuote Estimate(Money annualCompensation, CountryEmploymentPolicy policy)
    {
        ArgumentNullException.ThrowIfNull(policy);
        if (annualCompensation.Amount <= 0)
        {
            throw new BusinessRuleViolationException(
                "compensation.must_be_positive",
                "Annual compensation must be greater than zero.");
        }

        if (!string.Equals(annualCompensation.Currency, policy.Currency, StringComparison.Ordinal))
        {
            throw new BusinessRuleViolationException(
                "country_policy.currency_unsupported",
                $"Policy {policy.Id} requires {policy.Currency} compensation.");
        }

        var monthlySalary = annualCompensation.Divide(12);
        var contributions = monthlySalary.Multiply(policy.EmployerContributionRate);
        var serviceFee = Money.Of(policy.MonthlyServiceFee, policy.Currency);
        var monthlyTotal = monthlySalary.Add(contributions).Add(serviceFee);
        return new PlanQuote(
            policy.Id,
            monthlySalary,
            contributions,
            serviceFee,
            monthlyTotal,
            policy.Assumptions);
    }
}
