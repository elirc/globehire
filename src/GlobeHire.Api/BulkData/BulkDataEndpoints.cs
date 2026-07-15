using GlobeHire.Domain.BulkData;

namespace GlobeHire.Api.BulkData;

internal static class BulkDataEndpoints
{
    public static IEndpointRouteBuilder MapBulkDataEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/clients/{clientId}/bulk/workforce-plans/validate", (
            ValidateBulkRequest request) => TypedResults.Ok(BulkImportValidator.Validate(request.Rows)))
            .WithTags("Bulk data");

        endpoints.MapGet("/api/v1/clients/{clientId}/reports/workforce", (
            int pageSize,
            string? cursor) =>
        {
            var page = ReportPageRequest.Create(pageSize, cursor);
            return TypedResults.Ok(new { page.PageSize, page.Cursor, items = Array.Empty<object>() });
        }).WithTags("Reports");
        return endpoints;
    }

    private sealed record ValidateBulkRequest(IReadOnlyList<WorkforceImportRow> Rows);
}
