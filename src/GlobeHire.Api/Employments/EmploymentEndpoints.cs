using GlobeHire.Application.Employments;
using GlobeHire.Contracts.V1.Employments;
using GlobeHire.Domain.Employments;

namespace GlobeHire.Api.Employments;

internal static class EmploymentEndpoints
{
    public static IEndpointRouteBuilder MapEmploymentEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/clients/{clientId}/employments", ActivateAsync)
            .WithTags("Employments").Produces<EmploymentResponse>(StatusCodes.Status201Created);
        return endpoints;
    }

    private static async Task<IResult> ActivateAsync(
        string clientId,
        ActivateEmploymentRequest request, ActivateEmploymentHandler handler, CancellationToken cancellationToken)
    {
        var result = await handler.HandleAsync(
            clientId,
            request.OfferId,
            request.WorkerReference,
            request.StartDate,
            cancellationToken).ConfigureAwait(false);
        var response = ToResponse(result.Employment);
        return result.Created
            ? TypedResults.Created($"/api/v1/clients/{result.Employment.ClientId}/employments/{response.Id:D}", response)
            : TypedResults.Ok(response);
    }

    private static EmploymentResponse ToResponse(Employment employment) => new(
        employment.Id, employment.OfferId, employment.ContractId, employment.WorkerReference,
        employment.StartDate, employment.Status.ToString(), employment.Version, employment.ActivatedAt);
}
