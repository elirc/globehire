using GlobeHire.Application.WorkforcePlans;
using GlobeHire.Contracts.V1.WorkforcePlans;
using GlobeHire.Domain.Common;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace GlobeHire.Api.WorkforcePlans;

internal static class WorkforcePlanEndpoints
{
    public static IEndpointRouteBuilder MapWorkforcePlanEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/clients/{clientId}/workforce-plans")
            .WithTags("Workforce plans");

        group.MapPost("/", CreateAsync)
            .WithName("CreateWorkforcePlan")
            .WithSummary("Estimate and save a country-specific workforce plan")
            .Produces<WorkforcePlanResponse>(StatusCodes.Status201Created)
            .Produces<WorkforcePlanResponse>(StatusCodes.Status200OK)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        group.MapGet("/{planId}", GetAsync)
            .WithName("GetWorkforcePlan")
            .WithSummary("Retrieve a workforce plan within its client boundary")
            .Produces<WorkforcePlanResponse>()
            .Produces(StatusCodes.Status404NotFound)
            .ProducesProblem(StatusCodes.Status422UnprocessableEntity);

        return endpoints;
    }

    private static async Task<Results<Created<WorkforcePlanResponse>, Ok<WorkforcePlanResponse>>> CreateAsync(
        string clientId,
        [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
        CreateWorkforcePlanRequest request,
        CreateWorkforcePlanHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(idempotencyKey))
        {
            throw new BusinessRuleViolationException(
                "idempotency_key.required",
                "The Idempotency-Key header is required.");
        }

        var result = await handler.HandleAsync(
            new CreateWorkforcePlanCommand(
                clientId,
                idempotencyKey,
                request.RoleTitle,
                request.CountryCode,
                request.ProposedStartDate,
                request.AnnualCompensation,
                request.Currency),
            cancellationToken).ConfigureAwait(false);
        var response = result.Plan.ToResponse();
        httpContext.Response.Headers.ETag = $"\"{response.Version}\"";

        if (!result.WasCreated)
        {
            return TypedResults.Ok(response);
        }

        var location = $"/api/v1/clients/{Uri.EscapeDataString(clientId)}/workforce-plans/{response.Id:D}";
        return TypedResults.Created(location, response);
    }

    private static async Task<Results<Ok<WorkforcePlanResponse>, NotFound>> GetAsync(
        string clientId,
        string planId,
        GetWorkforcePlanHandler handler,
        HttpContext httpContext,
        CancellationToken cancellationToken)
    {
        var plan = await handler.HandleAsync(
            new GetWorkforcePlanQuery(clientId, planId),
            cancellationToken).ConfigureAwait(false);
        if (plan is null)
        {
            return TypedResults.NotFound();
        }

        var response = plan.ToResponse();
        httpContext.Response.Headers.ETag = $"\"{response.Version}\"";
        return TypedResults.Ok(response);
    }
}
