using GlobeHire.Domain.Common;
using GlobeHire.Domain.Contractors;

namespace GlobeHire.Domain.Tests.Contractors;

public sealed class ContractorEngagementTests
{
    [Fact]
    public void Activate_WithoutClassificationReview_IsRejected()
    {
        var engagement = Engagement();

        var error = Assert.Throws<BusinessRuleViolationException>(() => engagement.Activate(true, false));

        Assert.Equal("contractor.activation_requirements_incomplete", error.Code);
    }

    [Fact]
    public void Invoice_ApprovedThenPaid_FollowsLegalTransitions()
    {
        var engagement = Engagement();
        engagement.Activate(true, true);
        var invoice = new ContractorInvoice(
            Guid.NewGuid(),
            Money.Of(2_000, "USD"),
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 31));
        engagement.AddInvoice(invoice);

        invoice.Approve();
        invoice.MarkPaid();

        Assert.Equal(InvoiceStatus.Paid, invoice.Status);
    }

    [Fact]
    public void Dispute_WithoutReason_IsRejectedBeforeMutation()
    {
        var invoice = new ContractorInvoice(
            Guid.NewGuid(),
            Money.Of(2_000, "USD"),
            new DateOnly(2026, 7, 1),
            new DateOnly(2026, 7, 31));

        var error = Assert.Throws<BusinessRuleViolationException>(() => invoice.Dispute(" "));

        Assert.Equal("contractor.dispute_reason_required", error.Code);
        Assert.Equal(InvoiceStatus.Submitted, invoice.Status);
    }

    private static ContractorEngagement Engagement() =>
        new(Guid.NewGuid(), ClientId.Parse("client-acme"), "contractor-42", CountryCode.Parse("US"));
}
