namespace GlobeHire.Contracts.V1.WorkforcePlans;

public sealed record CreateWorkforcePlanRequest(
    string RoleTitle,
    string CountryCode,
    DateOnly ProposedStartDate,
    decimal AnnualCompensation,
    string Currency);
