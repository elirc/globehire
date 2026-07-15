using GlobeHire.Domain.EmploymentLifecycle;

namespace GlobeHire.Api.EmploymentLifecycle;

internal static class EmploymentLifecycleEndpoints
{
    public static IEndpointRouteBuilder MapEmploymentLifecycleEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/clients/{clientId}/employment-changes/validate", (
            ValidateEmploymentChangeRequest request) =>
        {
            var change = new EmploymentChange(
                request.Id,
                request.EmploymentId,
                request.ChangeType,
                request.EffectiveOn,
                request.Reason,
                request.Today);
            return TypedResults.Ok(new { change.Id, status = change.Status.ToString() });
        }).WithTags("Employment lifecycle");
        return endpoints;
    }

    private sealed record ValidateEmploymentChangeRequest(
        Guid Id,
        Guid EmploymentId,
        string ChangeType,
        DateOnly EffectiveOn,
        string Reason,
        DateOnly Today);
}
