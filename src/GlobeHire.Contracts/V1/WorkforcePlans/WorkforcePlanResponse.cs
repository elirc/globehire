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
    string PolicyId,
    MoneyResponse MonthlySalary,
    MoneyResponse MonthlyEmployerContributions,
    MoneyResponse MonthlyServiceFee,
    MoneyResponse EstimatedMonthlyTotal,
    IReadOnlyList<string> Assumptions);

public sealed record MoneyResponse(decimal Amount, string Currency);
