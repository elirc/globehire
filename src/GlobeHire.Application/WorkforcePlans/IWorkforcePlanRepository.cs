using GlobeHire.Domain.Common;
using GlobeHire.Domain.WorkforcePlans;

namespace GlobeHire.Application.WorkforcePlans;

public interface IWorkforcePlanRepository
{
    Task<StoredWorkforcePlan> AddOrGetAsync(WorkforcePlan plan, CancellationToken cancellationToken);

    Task<WorkforcePlan?> FindAsync(
        ClientId clientId,
        WorkforcePlanId planId,
        CancellationToken cancellationToken);
}

public sealed record StoredWorkforcePlan(WorkforcePlan Plan, bool WasAdded);
