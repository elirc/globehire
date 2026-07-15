using GlobeHire.Application.Offers;
using GlobeHire.Domain.Common;
using GlobeHire.Domain.Offers;

namespace GlobeHire.Infrastructure.Offers;

public sealed class InMemoryOfferRepository : IOfferRepository
{
    private readonly Dictionary<(ClientId ClientId, Guid Id), EmploymentOffer> _offers = [];
    private readonly Lock _gate = new();

    public Task AddAsync(EmploymentOffer offer, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate) { _offers.Add((offer.ClientId, offer.Id), offer); }
        return Task.CompletedTask;
    }

    public Task<EmploymentOffer?> FindAsync(ClientId clientId, Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _offers.TryGetValue((clientId, id), out var offer);
            return Task.FromResult(offer);
        }
    }

    public Task<EmploymentOffer?> MutateAsync(
        ClientId clientId,
        Guid id,
        Action<EmploymentOffer> mutation,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(mutation);
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (!_offers.TryGetValue((clientId, id), out var offer))
            {
                return Task.FromResult<EmploymentOffer?>(null);
            }

            mutation(offer);
            return Task.FromResult<EmploymentOffer?>(offer);
        }
    }
}
