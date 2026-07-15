using GlobeHire.Domain.Common;

namespace GlobeHire.Domain.Contractors;

public enum ContractorStatus
{
    Draft,
    Active,
    Ended,
}

public enum InvoiceStatus
{
    Submitted,
    Approved,
    Disputed,
    Paid,
}

public sealed class ContractorInvoice
{
    public ContractorInvoice(Guid id, Money amount, DateOnly serviceFrom, DateOnly serviceTo)
    {
        if (id == Guid.Empty || amount.Amount <= 0 || serviceTo < serviceFrom)
        {
            throw new BusinessRuleViolationException("contractor.invoice_invalid", "Invoice inputs are invalid.");
        }

        Id = id;
        Amount = amount;
        ServiceFrom = serviceFrom;
        ServiceTo = serviceTo;
    }

    public Guid Id { get; }
    public Money Amount { get; }
    public DateOnly ServiceFrom { get; }
    public DateOnly ServiceTo { get; }
    public InvoiceStatus Status { get; private set; }
    public string? DisputeReason { get; private set; }

    public void Approve()
    {
        EnsureSubmitted();
        Status = InvoiceStatus.Approved;
    }

    public void Dispute(string? reason)
    {
        EnsureSubmitted();
        if (string.IsNullOrWhiteSpace(reason))
        {
            throw new BusinessRuleViolationException(
                "contractor.dispute_reason_required",
                "A dispute reason is required.");
        }

        Status = InvoiceStatus.Disputed;
        DisputeReason = reason.Trim();
    }

    public void MarkPaid()
    {
        if (Status != InvoiceStatus.Approved)
        {
            throw new BusinessRuleViolationException(
                "contractor.invoice_not_approved",
                "Only approved invoices can be paid.");
        }

        Status = InvoiceStatus.Paid;
    }

    private void EnsureSubmitted()
    {
        if (Status != InvoiceStatus.Submitted)
        {
            throw new BusinessRuleViolationException(
                "contractor.invoice_already_decided",
                "Invoice has already been decided.");
        }
    }
}

public sealed class ContractorEngagement
{
    private readonly List<ContractorInvoice> _invoices = [];

    public ContractorEngagement(Guid id, ClientId clientId, string? contractorReference, CountryCode country)
    {
        if (id == Guid.Empty || string.IsNullOrWhiteSpace(contractorReference))
        {
            throw new BusinessRuleViolationException("contractor.engagement_invalid", "Engagement inputs are invalid.");
        }

        Id = id;
        ClientId = clientId;
        ContractorReference = contractorReference.Trim();
        Country = country;
    }

    public Guid Id { get; }
    public ClientId ClientId { get; }
    public string ContractorReference { get; }
    public CountryCode Country { get; }
    public ContractorStatus Status { get; private set; }
    public IReadOnlyList<ContractorInvoice> Invoices => _invoices;

    public void Activate(bool agreementSigned, bool classificationReviewed)
    {
        if (!agreementSigned || !classificationReviewed)
        {
            throw new BusinessRuleViolationException(
                "contractor.activation_requirements_incomplete",
                "Signed agreement and classification review are required.");
        }

        Status = ContractorStatus.Active;
    }

    public void AddInvoice(ContractorInvoice invoice)
    {
        ArgumentNullException.ThrowIfNull(invoice);
        if (Status != ContractorStatus.Active)
        {
            throw new BusinessRuleViolationException(
                "contractor.engagement_not_active",
                "Invoices require an active engagement.");
        }

        if (_invoices.Any(existing => existing.Id == invoice.Id))
        {
            throw new BusinessRuleViolationException("contractor.invoice_duplicate", "Invoice already exists.");
        }

        _invoices.Add(invoice);
    }
}
