using GlobeHire.Application.WorkforcePlans;
using GlobeHire.Domain.Common;
using GlobeHire.Domain.WorkforcePlans;

namespace GlobeHire.Infrastructure.WorkforcePlans;

public sealed class InMemoryWorkforcePlanRepository : IWorkforcePlanRepository
{
    private readonly Lock _gate = new();
    private readonly Dictionary<PlanKey, WorkforcePlan> _plans = [];
    private readonly Dictionary<RequestKey, PlanKey> _requests = [];

    public int Count
    {
        get
        {
            lock (_gate)
            {
                return _plans.Count;
            }
        }
    }

    public Task<StoredWorkforcePlan> AddOrGetAsync(
        WorkforcePlan plan,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(plan);
        cancellationToken.ThrowIfCancellationRequested();

        lock (_gate)
        {
            var requestKey = new RequestKey(plan.ClientId, plan.IdempotencyKey);
            if (_requests.TryGetValue(requestKey, out var existingKey))
            {
                return Task.FromResult(new StoredWorkforcePlan(_plans[existingKey], WasAdded: false));
            }

            var planKey = new PlanKey(plan.ClientId, plan.Id);
            _plans.Add(planKey, plan);
            _requests.Add(requestKey, planKey);
            return Task.FromResult(new StoredWorkforcePlan(plan, WasAdded: true));
        }
    }

    public Task<WorkforcePlan?> FindAsync(
        ClientId clientId,
        WorkforcePlanId planId,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        lock (_gate)
        {
            _plans.TryGetValue(new PlanKey(clientId, planId), out var plan);
            return Task.FromResult(plan);
        }
    }

    private readonly record struct PlanKey(ClientId ClientId, WorkforcePlanId PlanId);

    private readonly record struct RequestKey(ClientId ClientId, string IdempotencyKey);
}
