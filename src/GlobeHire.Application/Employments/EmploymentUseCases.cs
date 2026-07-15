using GlobeHire.Application.Abstractions;
using GlobeHire.Application.Offers;
using GlobeHire.Domain.Common;
using GlobeHire.Domain.Employments;

namespace GlobeHire.Application.Employments;

public interface IEmploymentRepository
{
    Task<(Employment Employment, bool Added)> AddOrGetAsync(Employment employment, CancellationToken cancellationToken);
}

public sealed class ActivateEmploymentHandler(
    IOfferRepository offers, IEmploymentRepository employments, IIdGenerator ids, IClock clock)
{
    public async Task<(Employment Employment, bool Created)> HandleAsync(
        string clientId, Guid offerId, string workerReference, DateOnly startDate, CancellationToken cancellationToken)
    {
        var offer = await offers.FindAsync(ClientId.Parse(clientId), offerId, cancellationToken).ConfigureAwait(false)
            ?? throw new BusinessRuleViolationException("offer.not_found", "Offer was not found.");
        var employment = Employment.Activate(ids.NewId(), offer, workerReference, startDate, clock.Today, clock.UtcNow);
        var stored = await employments.AddOrGetAsync(employment, cancellationToken).ConfigureAwait(false);
        return (stored.Employment, stored.Added);
    }
}
