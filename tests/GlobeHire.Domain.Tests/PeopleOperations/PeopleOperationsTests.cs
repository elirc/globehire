using GlobeHire.Domain.Common;
using GlobeHire.Domain.PeopleOperations;

namespace GlobeHire.Domain.Tests.PeopleOperations;

public sealed class PeopleOperationsTests
{
    [Fact]
    public void LeaveBeyondBalance_IsRejected()
    {
        var balance = new LeaveBalance(10); balance.Approve(8);
        var error = Assert.Throws<BusinessRuleViolationException>(() => balance.Approve(3));
        Assert.Equal("leave.balance_insufficient", error.Code);
    }
    [Fact]
    public void RejectedExpense_RequiresReason()
    {
        var claim = new ExpenseClaim(Guid.NewGuid(), Money.Of(25, "USD"), "travel");
        var error = Assert.Throws<BusinessRuleViolationException>(() => claim.Decide(false, null));
        Assert.Equal("expense.rejection_reason_required", error.Code);
    }
}
