using GlobeHire.Application.Offers;
using GlobeHire.Contracts.V1.Offers;
using GlobeHire.Domain.Common;
using GlobeHire.Domain.Offers;
using Microsoft.AspNetCore.Mvc;

namespace GlobeHire.Api.Offers;

internal static class OfferEndpoints
{
    public static IEndpointRouteBuilder MapOfferEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/clients/{clientId}/offers").WithTags("Offers");
        group.MapPost("/", CreateAsync).Produces<OfferResponse>(StatusCodes.Status201Created);
        group.MapPost("/{id:guid}/approvals", ApproveAsync).Produces<OfferResponse>();
        group.MapPost("/{id:guid}/contract", IssueAsync).Produces<OfferResponse>();
        return endpoints;
    }

    private static async Task<IResult> CreateAsync(
        CreateOfferRequest request, OfferHandler handler, HttpContext context, CancellationToken cancellationToken)
    {
        var offer = await handler.CreateAsync(request.HireCaseId, cancellationToken).ConfigureAwait(false);
        return Respond(offer, context, created: true);
    }

    private static async Task<IResult> ApproveAsync(
        Guid id, ApproveOfferRequest request, [FromHeader(Name = "If-Match")] string? ifMatch,
        OfferHandler handler, HttpContext context, CancellationToken cancellationToken)
    {
        if (!Enum.TryParse<ApprovalRole>(request.Role, true, out var role))
        {
            throw new BusinessRuleViolationException("approval.role_invalid", "Approval role is invalid.");
        }

        var offer = await handler.ApproveAsync(id, role, request.ActorId, Version(ifMatch), cancellationToken).ConfigureAwait(false);
        return Respond(offer, context);
    }

    private static async Task<IResult> IssueAsync(
        Guid id, IssueContractRequest request, [FromHeader(Name = "If-Match")] string? ifMatch,
        OfferHandler handler, HttpContext context, CancellationToken cancellationToken)
    {
        var offer = await handler.IssueAsync(id, request.TemplateKey, Version(ifMatch), cancellationToken).ConfigureAwait(false);
        return Respond(offer, context);
    }

    private static int Version(string? value) => int.TryParse(value?.Trim('"'), out var version)
        ? version
        : throw new BusinessRuleViolationException("concurrency.if_match_required", "A numeric If-Match ETag is required.");

    private static IResult Respond(EmploymentOffer? offer, HttpContext context, bool created = false)
    {
        if (offer is null) return TypedResults.NotFound();
        context.Response.Headers.ETag = $"\"{offer.Version}\"";
        var response = new OfferResponse(
            offer.Id, offer.HireCaseId, offer.Status.ToString(), offer.Version,
            offer.Approvals.Select(a => new OfferApprovalResponse(a.Role.ToString(), a.ActorId, a.ApprovedAt)).ToArray(),
            offer.Contract is null ? null : new ContractResponse(offer.Contract.Id, offer.Contract.Revision, offer.Contract.TemplateKey, offer.Contract.IssuedAt));
        return created
            ? TypedResults.Created($"/api/v1/clients/{offer.ClientId}/offers/{offer.Id:D}", response)
            : TypedResults.Ok(response);
    }
}
