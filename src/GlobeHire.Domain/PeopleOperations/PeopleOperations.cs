using GlobeHire.Domain.Common;

namespace GlobeHire.Domain.PeopleOperations;

public sealed class LeaveBalance
{
    public LeaveBalance(decimal entitledDays) { if (entitledDays < 0) throw new BusinessRuleViolationException("leave.entitlement_invalid", "Entitlement cannot be negative."); EntitledDays = entitledDays; }
    public decimal EntitledDays { get; }
    public decimal ApprovedDays { get; private set; }
    public decimal AvailableDays => EntitledDays - ApprovedDays;
    public void Approve(decimal days)
    {
        if (days <= 0 || days > AvailableDays) throw new BusinessRuleViolationException("leave.balance_insufficient", "Requested leave exceeds available balance.");
        ApprovedDays += days;
    }
}

public enum ExpenseStatus { Submitted, Approved, Rejected }
public sealed class ExpenseClaim
{
    public ExpenseClaim(Guid id, Money amount, string? category)
    {
        if (id == Guid.Empty || amount.Amount <= 0 || string.IsNullOrWhiteSpace(category)) throw new BusinessRuleViolationException("expense.invalid", "Expense claim is invalid.");
        Id = id; Amount = amount; Category = category.Trim();
    }
    public Guid Id { get; }
    public Money Amount { get; }
    public string Category { get; }
    public ExpenseStatus Status { get; private set; }
    public string? DecisionReason { get; private set; }
    public void Decide(bool approve, string? reason)
    {
        if (Status != ExpenseStatus.Submitted) throw new BusinessRuleViolationException("expense.already_decided", "Expense has already been decided.");
        if (!approve && string.IsNullOrWhiteSpace(reason)) throw new BusinessRuleViolationException("expense.rejection_reason_required", "Rejected expenses require a reason.");
        Status = approve ? ExpenseStatus.Approved : ExpenseStatus.Rejected; DecisionReason = reason?.Trim();
    }
}
