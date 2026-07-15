using GlobeHire.Application.HireCases;
using GlobeHire.Contracts.V1.HireCases;
using GlobeHire.Domain.Common;
using GlobeHire.Domain.HireCases;
using Microsoft.AspNetCore.Mvc;

namespace GlobeHire.Api.HireCases;

internal static class HireCaseEndpoints
{
    public static IEndpointRouteBuilder MapHireCaseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/clients/{clientId}/hire-cases").WithTags("Hire cases");
        group.MapPost("/", OpenAsync).Produces<HireCaseResponse>(StatusCodes.Status201Created);
        group.MapGet("/{id:guid}", GetAsync).Produces<HireCaseResponse>().Produces(StatusCodes.Status404NotFound);
        group.MapPut("/{id:guid}/tasks/{taskCode}/complete", CompleteTaskAsync).Produces<HireCaseResponse>();
        group.MapPost("/{id:guid}/submit", SubmitAsync).Produces<HireCaseResponse>();
        return endpoints;
    }

    private static async Task<IResult> OpenAsync(
        string clientId,
        OpenHireCaseRequest request,
        OpenHireCaseHandler handler,
        CancellationToken cancellationToken)
    {
        var hireCase = await handler.HandleAsync(
            new OpenHireCaseCommand(
                clientId,
                request.WorkforcePlanId.ToString("D"),
                request.CandidateName,
                request.CandidateEmail),
            cancellationToken).ConfigureAwait(false);
        return TypedResults.Created($"/api/v1/clients/{clientId}/hire-cases/{hireCase.Id:D}", ToResponse(hireCase));
    }

    private static async Task<IResult> GetAsync(
        Guid id,
        ChangeHireCaseHandler handler,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var hireCase = await handler.GetAsync(id, cancellationToken).ConfigureAwait(false);
        return Respond(hireCase, context);
    }

    private static async Task<IResult> CompleteTaskAsync(
        Guid id,
        string taskCode,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        ChangeHireCaseHandler handler,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var hireCase = await handler.CompleteTaskAsync(
            id,
            taskCode,
            ParseVersion(ifMatch),
            cancellationToken).ConfigureAwait(false);
        return Respond(hireCase, context);
    }

    private static async Task<IResult> SubmitAsync(
        Guid id,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        ChangeHireCaseHandler handler,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var hireCase = await handler.SubmitAsync(id, ParseVersion(ifMatch), cancellationToken).ConfigureAwait(false);
        return Respond(hireCase, context);
    }

    private static IResult Respond(HireCase? hireCase, HttpContext context)
    {
        if (hireCase is null)
        {
            return TypedResults.NotFound();
        }

        context.Response.Headers.ETag = $"\"{hireCase.Version}\"";
        return TypedResults.Ok(ToResponse(hireCase));
    }

    private static int ParseVersion(string? ifMatch)
    {
        if (!int.TryParse(ifMatch?.Trim('"'), out var version))
        {
            throw new BusinessRuleViolationException("concurrency.if_match_required", "A numeric If-Match ETag is required.");
        }

        return version;
    }

    private static HireCaseResponse ToResponse(HireCase hireCase) =>
        new(
            hireCase.Id,
            hireCase.ClientId.Value,
            hireCase.WorkforcePlanId.Value,
            hireCase.CandidateName,
            hireCase.CandidateEmail,
            hireCase.Status.ToString(),
            hireCase.Version,
            hireCase.Tasks.Select(task => new OnboardingTaskResponse(
                task.Code,
                task.Description,
                task.IsComplete,
                task.CompletedAt)).ToArray());
}
