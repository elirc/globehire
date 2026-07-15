using GlobeHire.Domain.Common;
using GlobeHire.Domain.Integrations;

namespace GlobeHire.Api.Integrations;

internal static class IntegrationEndpoints
{
    public static IEndpointRouteBuilder MapIntegrationEndpoints(this IEndpointRouteBuilder endpoints)
    {
        endpoints.MapPost("/api/v1/clients/{clientId}/webhook-subscriptions/validate", (
            string clientId,
            ValidateSubscriptionRequest request) =>
        {
            var subscription = WebhookSubscription.Create(
                request.Id,
                ClientId.Parse(clientId),
                request.Endpoint,
                request.EventTypes);
            return TypedResults.Ok(new
            {
                subscription.Id,
                endpoint = subscription.Endpoint.ToString(),
                eventTypes = subscription.EventTypes,
            });
        }).WithTags("Integrations");
        return endpoints;
    }

    private sealed record ValidateSubscriptionRequest(Guid Id, string Endpoint, IReadOnlyList<string> EventTypes);
}
