using GlobeHire.Domain.Common;
using GlobeHire.Domain.PeopleOperations;

namespace GlobeHire.Api.PeopleOperations;

internal static class PeopleOperationsEndpoints
{
    public static IEndpointRouteBuilder MapPeopleOperationsEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/clients/{clientId}/people-operations").WithTags("People operations");
        group.MapPost("/leave/check", (LeaveCheckRequest request) =>
        {
            var balance = new LeaveBalance(request.EntitledDays);
            if (request.AlreadyApprovedDays > 0) balance.Approve(request.AlreadyApprovedDays);
            balance.Approve(request.RequestedDays);
            return TypedResults.Ok(new { balance.AvailableDays });
        });
        group.MapPost("/expenses/check", (ExpenseCheckRequest request) =>
        {
            var claim = new ExpenseClaim(Guid.NewGuid(), Money.Of(request.Amount, request.Currency), request.Category);
            return TypedResults.Ok(new { claim.Id, status = claim.Status.ToString() });
        });
        return endpoints;
    }

    private sealed record LeaveCheckRequest(decimal EntitledDays, decimal AlreadyApprovedDays, decimal RequestedDays);
    private sealed record ExpenseCheckRequest(decimal Amount, string Currency, string Category);
}
