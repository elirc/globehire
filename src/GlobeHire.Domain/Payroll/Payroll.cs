using GlobeHire.Domain.Common;

namespace GlobeHire.Domain.Payroll;

public enum PayrollPeriodStatus { Draft, Finalized }

public sealed record PayrollStatement(
    Guid EmploymentId,
    Money GrossPay,
    Money EmployeeDeductions,
    Money EmployerContributions,
    Money NetPay)
{
    public static PayrollStatement Calculate(
        Guid employmentId, Money annualSalary, int periodsPerYear,
        decimal employeeDeductionRate, decimal employerContributionRate)
    {
        if (employmentId == Guid.Empty || periodsPerYear is < 1 or > 366)
            throw new BusinessRuleViolationException("payroll.input_invalid", "Payroll inputs are invalid.");
        if (employeeDeductionRate is < 0 or > 1 || employerContributionRate is < 0 or > 1)
            throw new BusinessRuleViolationException("payroll.rate_invalid", "Payroll rates must be between zero and one.");

        var gross = annualSalary.Divide(periodsPerYear);
        var deductions = gross.Multiply(employeeDeductionRate);
        var contributions = gross.Multiply(employerContributionRate);
        return new PayrollStatement(employmentId, gross, deductions, contributions, Money.Of(gross.Amount - deductions.Amount, gross.Currency));
    }
}

public sealed class PayrollPeriod
{
    private readonly Dictionary<Guid, PayrollStatement> _statements = [];
    public PayrollPeriod(Guid id, DateOnly start, DateOnly end)
    {
        if (id == Guid.Empty || end < start) throw new BusinessRuleViolationException("payroll.period_invalid", "Payroll period is invalid.");
        Id = id; Start = start; End = end;
    }
    public Guid Id { get; }
    public DateOnly Start { get; }
    public DateOnly End { get; }
    public PayrollPeriodStatus Status { get; private set; }
    public IReadOnlyCollection<PayrollStatement> Statements => _statements.Values;
    public void Add(PayrollStatement statement)
    {
        if (Status != PayrollPeriodStatus.Draft) throw new BusinessRuleViolationException("payroll.period_finalized", "Finalized payroll cannot change.");
        if (!_statements.TryAdd(statement.EmploymentId, statement)) throw new BusinessRuleViolationException("payroll.statement_duplicate", "Employment already has a statement.");
    }
    public void FinalizePeriod()
    {
        if (_statements.Count == 0) throw new BusinessRuleViolationException("payroll.period_empty", "Empty payroll cannot be finalized.");
        Status = PayrollPeriodStatus.Finalized;
    }
}
