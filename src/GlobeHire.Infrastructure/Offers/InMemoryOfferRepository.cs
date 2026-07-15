using GlobeHire.Application.Offers;
using GlobeHire.Domain.Offers;

namespace GlobeHire.Infrastructure.Offers;

public sealed class InMemoryOfferRepository : IOfferRepository
{
    private readonly Dictionary<Guid, EmploymentOffer> _offers = [];
    private readonly Lock _gate = new();

    public Task AddAsync(EmploymentOffer offer, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate) { _offers.Add(offer.Id, offer); }
        return Task.CompletedTask;
    }

    public Task<EmploymentOffer?> FindAsync(Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _offers.TryGetValue(id, out var offer);
            return Task.FromResult(offer);
        }
    }
}
