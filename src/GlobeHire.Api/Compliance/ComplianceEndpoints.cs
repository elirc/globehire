using GlobeHire.Domain.Common;
using GlobeHire.Domain.Compliance;

namespace GlobeHire.Api.Compliance;

internal static class ComplianceEndpoints
{
    public static IEndpointRouteBuilder MapComplianceEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/clients/{clientId}/compliance-cases/validate", (
            string clientId,
            ValidateComplianceCaseRequest request) =>
        {
            var complianceCase = new ComplianceCase(
                request.Id,
                ClientId.Parse(clientId),
                request.SubjectReference,
                CountryCode.Parse(request.CountryCode),
                request.RequiredEvidenceTypes);
            return TypedResults.Ok(new
            {
                complianceCase.Id,
                status = complianceCase.Status.ToString(),
                requiredEvidenceTypes = complianceCase.RequiredEvidenceTypes,
            });
        }).WithTags("Compliance");
        return endpoints;
    }

    private sealed record ValidateComplianceCaseRequest(
        Guid Id,
        string SubjectReference,
        string CountryCode,
        IReadOnlyList<string> RequiredEvidenceTypes);
}
