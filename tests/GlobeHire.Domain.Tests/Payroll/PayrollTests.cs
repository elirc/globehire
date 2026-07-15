using GlobeHire.Domain.Common;
using GlobeHire.Domain.Payroll;

namespace GlobeHire.Domain.Tests.Payroll;

public sealed class PayrollTests
{
    [Fact]
    public void Calculate_ProducesReconciledStatementLines()
    {
        var statement = PayrollStatement.Calculate(Guid.NewGuid(), Money.Of(120_000, "USD"), 12, .20m, .12m);
        Assert.Equal(10_000m, statement.GrossPay.Amount);
        Assert.Equal(2_000m, statement.EmployeeDeductions.Amount);
        Assert.Equal(8_000m, statement.NetPay.Amount);
        Assert.Equal(statement.GrossPay.Amount, statement.NetPay.Amount + statement.EmployeeDeductions.Amount);
    }

    [Fact]
    public void Finalize_EmptyPeriod_IsRejected()
    {
        var period = new PayrollPeriod(Guid.NewGuid(), new DateOnly(2026, 7, 1), new DateOnly(2026, 7, 31));
        var exception = Assert.Throws<BusinessRuleViolationException>(period.FinalizePeriod);
        Assert.Equal("payroll.period_empty", exception.Code);
    }
}
