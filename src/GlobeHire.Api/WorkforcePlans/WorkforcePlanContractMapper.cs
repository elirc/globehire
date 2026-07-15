using GlobeHire.Contracts.V1.WorkforcePlans;
using GlobeHire.Domain.Common;
using GlobeHire.Domain.WorkforcePlans;

namespace GlobeHire.Api.WorkforcePlans;

internal static class WorkforcePlanContractMapper
{
    public static WorkforcePlanResponse ToResponse(this WorkforcePlan plan) =>
        new(
            plan.Id.Value,
            plan.ClientId.Value,
            plan.RoleTitle,
            plan.Country.Value,
            plan.ProposedStartDate,
            plan.AnnualCompensation.ToResponse(),
            new PlanQuoteResponse(
                plan.Quote.MonthlySalary.ToResponse(),
                plan.Quote.MonthlyServiceFee.ToResponse(),
                plan.Quote.EstimatedMonthlyTotal.ToResponse()),
            plan.Version,
            plan.CreatedAt);

    private static MoneyResponse ToResponse(this Money money) => new(money.Amount, money.Currency);
}
