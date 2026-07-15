using GlobeHire.Application.HireCases;
using GlobeHire.Domain.Common;
using GlobeHire.Domain.HireCases;

namespace GlobeHire.Infrastructure.HireCases;

public sealed class InMemoryHireCaseRepository : IHireCaseRepository
{
    private readonly Dictionary<(ClientId ClientId, Guid Id), HireCase> _cases = [];
    private readonly Lock _gate = new();

    public Task AddAsync(HireCase hireCase, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _cases.Add((hireCase.ClientId, hireCase.Id), hireCase);
        }

        return Task.CompletedTask;
    }

    public Task<HireCase?> FindAsync(ClientId clientId, Guid id, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _cases.TryGetValue((clientId, id), out var hireCase);
            return Task.FromResult(hireCase);
        }
    }
}
