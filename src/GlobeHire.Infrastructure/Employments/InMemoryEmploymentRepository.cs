using GlobeHire.Application.Employments;
using GlobeHire.Domain.Employments;

namespace GlobeHire.Infrastructure.Employments;

public sealed class InMemoryEmploymentRepository : IEmploymentRepository
{
    private readonly Dictionary<Guid, Employment> _byOffer = [];
    private readonly Lock _gate = new();

    public Task<(Employment Employment, bool Added)> AddOrGetAsync(Employment employment, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            if (_byOffer.TryGetValue(employment.OfferId, out var existing)) return Task.FromResult((existing, false));
            _byOffer.Add(employment.OfferId, employment);
            return Task.FromResult((employment, true));
        }
    }
}
