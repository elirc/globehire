using GlobeHire.Domain.Common;
using GlobeHire.Domain.Integrations;

namespace GlobeHire.Domain.Tests.Integrations;

public sealed class IntegrationModelsTests
{
    [Fact]
    public void Subscription_NonHttpsEndpoint_IsRejected()
    {
        var error = Assert.Throws<BusinessRuleViolationException>(() => WebhookSubscription.Create(
            Guid.NewGuid(),
            ClientId.Parse("client-acme"),
            "http://example.com/hooks",
            ["employment.activated"]));

        Assert.Equal("webhook.endpoint_invalid", error.Code);
    }

    [Fact]
    public void Delivery_NonSuccessResponse_CanBeRetried()
    {
        var delivery = new WebhookDelivery(Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid());
        delivery.RecordResponse(503);

        var retry = delivery.Retry();

        Assert.Equal(DeliveryStatus.Failed, delivery.Status);
        Assert.Equal(delivery.MessageId, retry.MessageId);
        Assert.Equal(delivery.Id, retry.Id);
        Assert.Equal(2, retry.Attempt);
    }

    [Fact]
    public void SignatureInput_IsDeterministicForMessageAndPayload()
    {
        var messageId = Guid.Parse("82ea1480-ac3d-4b80-8adc-4c562443cb78");
        var delivery = new WebhookDelivery(Guid.NewGuid(), messageId, Guid.NewGuid());

        var input = delivery.SignatureInput(1_784_000_000, "{\"id\":42}");

        Assert.Equal($"1784000000.{messageId:D}.{{\"id\":42}}", input);
    }
}
