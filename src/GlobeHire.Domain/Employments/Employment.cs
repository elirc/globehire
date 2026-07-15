using GlobeHire.Domain.Common;
using GlobeHire.Domain.Offers;

namespace GlobeHire.Domain.Employments;

public enum EmploymentStatus { Active, Ended }

public sealed class Employment
{
    private Employment(Guid id, EmploymentOffer offer, string workerReference, DateOnly startDate, DateTimeOffset activatedAt)
    {
        Id = id;
        ClientId = offer.ClientId;
        OfferId = offer.Id;
        ContractId = offer.Contract!.Id;
        WorkerReference = workerReference;
        StartDate = startDate;
        ActivatedAt = activatedAt.ToUniversalTime();
    }

    public Guid Id { get; }
    public ClientId ClientId { get; }
    public Guid OfferId { get; }
    public Guid ContractId { get; }
    public string WorkerReference { get; }
    public DateOnly StartDate { get; }
    public DateTimeOffset ActivatedAt { get; }
    public EmploymentStatus Status { get; private set; }
    public int Version { get; private set; } = 1;

    public static Employment Activate(
        Guid id, EmploymentOffer offer, string? workerReference, DateOnly startDate, DateOnly today, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(offer);
        if (id == Guid.Empty || offer.Status != OfferStatus.ContractIssued || offer.Contract is null)
        {
            throw new BusinessRuleViolationException("employment.contract_not_issued", "An issued contract is required.");
        }

        var reference = workerReference?.Trim();
        if (reference is null || reference.Length is < 3 or > 64)
        {
            throw new BusinessRuleViolationException("employment.worker_reference_invalid", "Worker reference is invalid.");
        }

        if (startDate < today)
        {
            throw new BusinessRuleViolationException("employment.start_date_in_past", "Employment cannot be activated in the past.");
        }

        return new Employment(id, offer, reference, startDate, now);
    }
}
