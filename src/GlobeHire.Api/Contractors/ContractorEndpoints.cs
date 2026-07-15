using GlobeHire.Domain.Common;
using GlobeHire.Domain.Contractors;

namespace GlobeHire.Api.Contractors;

internal static class ContractorEndpoints
{
    public static IEndpointRouteBuilder MapContractorEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/clients/{clientId}/contractor-invoices/validate", (
            string clientId,
            ValidateInvoiceRequest request) =>
        {
            _ = ClientId.Parse(clientId);
            var invoice = new ContractorInvoice(
                request.Id,
                Money.Of(request.Amount, request.Currency),
                request.ServiceFrom,
                request.ServiceTo);
            return TypedResults.Ok(new { invoice.Id, status = invoice.Status.ToString() });
        }).WithTags("Contractors");
        return endpoints;
    }

    private sealed record ValidateInvoiceRequest(
        Guid Id,
        decimal Amount,
        string Currency,
        DateOnly ServiceFrom,
        DateOnly ServiceTo);
}
