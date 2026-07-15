using GlobeHire.Domain.Common;

namespace GlobeHire.Domain.Integrations;

public enum DeliveryStatus
{
    Pending,
    Delivered,
    Failed,
}

public sealed record OutboxMessage(
    Guid Id,
    ClientId ClientId,
    string EventType,
    string AggregateId,
    string Payload,
    DateTimeOffset OccurredAt)
{
    public static OutboxMessage Create(
        Guid id,
        ClientId clientId,
        string? eventType,
        string? aggregateId,
        string? payload,
        DateTimeOffset occurredAt)
    {
        if (id == Guid.Empty || string.IsNullOrWhiteSpace(eventType) ||
            string.IsNullOrWhiteSpace(aggregateId) || string.IsNullOrWhiteSpace(payload))
        {
            throw new BusinessRuleViolationException("outbox.message_invalid", "Outbox message is invalid.");
        }

        return new OutboxMessage(
            id,
            clientId,
            eventType.Trim(),
            aggregateId.Trim(),
            payload,
            occurredAt.ToUniversalTime());
    }
}

public sealed record WebhookSubscription
{
    private WebhookSubscription(Guid id, ClientId clientId, Uri endpoint, IReadOnlySet<string> eventTypes)
    {
        Id = id;
        ClientId = clientId;
        Endpoint = endpoint;
        EventTypes = eventTypes;
    }

    public Guid Id { get; }
    public ClientId ClientId { get; }
    public Uri Endpoint { get; }
    public IReadOnlySet<string> EventTypes { get; }

    public static WebhookSubscription Create(
        Guid id,
        ClientId clientId,
        string? endpoint,
        IEnumerable<string> eventTypes)
    {
        if (id == Guid.Empty || !Uri.TryCreate(endpoint, UriKind.Absolute, out var uri) ||
            uri.Scheme != Uri.UriSchemeHttps)
        {
            throw new BusinessRuleViolationException(
                "webhook.endpoint_invalid",
                "Webhook endpoint must be an absolute HTTPS URL.");
        }

        var events = eventTypes
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToHashSet(StringComparer.Ordinal);
        if (events.Count == 0)
        {
            throw new BusinessRuleViolationException("webhook.events_required", "At least one event type is required.");
        }

        return new WebhookSubscription(id, clientId, uri, events);
    }
}

public sealed class WebhookDelivery
{
    public WebhookDelivery(Guid id, Guid messageId, Guid subscriptionId, int attempt = 1)
    {
        if (id == Guid.Empty || messageId == Guid.Empty || subscriptionId == Guid.Empty || attempt < 1)
        {
            throw new BusinessRuleViolationException("webhook.delivery_invalid", "Webhook delivery is invalid.");
        }

        Id = id;
        MessageId = messageId;
        SubscriptionId = subscriptionId;
        Attempt = attempt;
    }

    public Guid Id { get; }
    public Guid MessageId { get; }
    public Guid SubscriptionId { get; }
    public int Attempt { get; }
    public DeliveryStatus Status { get; private set; }
    public int? ResponseStatusCode { get; private set; }

    public string SignatureInput(long unixTimestamp, string payload) =>
        $"{unixTimestamp}.{MessageId:D}.{payload}";

    public void RecordResponse(int statusCode)
    {
        ResponseStatusCode = statusCode;
        Status = statusCode is >= 200 and < 300 ? DeliveryStatus.Delivered : DeliveryStatus.Failed;
    }

    public WebhookDelivery Retry(Guid retryId)
    {
        if (Status != DeliveryStatus.Failed)
        {
            throw new BusinessRuleViolationException(
                "webhook.retry_not_allowed",
                "Only failed deliveries can be retried.");
        }

        return new WebhookDelivery(retryId, MessageId, SubscriptionId, Attempt + 1);
    }
}
