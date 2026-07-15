namespace GlobeHire.Contracts.V1.WorkforcePlans;

public sealed record WorkforcePlanResponse(
    Guid Id,
    string ClientId,
    string RoleTitle,
    string CountryCode,
    DateOnly ProposedStartDate,
    MoneyResponse AnnualCompensation,
    PlanQuoteResponse Quote,
    int Version,
    DateTimeOffset CreatedAt);

public sealed record PlanQuoteResponse(
    MoneyResponse MonthlySalary,
    MoneyResponse MonthlyServiceFee,
    MoneyResponse EstimatedMonthlyTotal);

public sealed record MoneyResponse(decimal Amount, string Currency);
