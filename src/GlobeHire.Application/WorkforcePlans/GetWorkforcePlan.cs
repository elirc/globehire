using GlobeHire.Domain.Common;
using GlobeHire.Domain.WorkforcePlans;

namespace GlobeHire.Application.WorkforcePlans;

public sealed record GetWorkforcePlanQuery(string ClientId, string WorkforcePlanId);

public sealed class GetWorkforcePlanHandler
{
    private readonly IWorkforcePlanRepository _repository;

    public GetWorkforcePlanHandler(IWorkforcePlanRepository repository) => _repository = repository;

    public Task<WorkforcePlan?> HandleAsync(
        GetWorkforcePlanQuery query,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(query);
        return _repository.FindAsync(
            ClientId.Parse(query.ClientId),
            WorkforcePlanId.Parse(query.WorkforcePlanId),
            cancellationToken);
    }
}
