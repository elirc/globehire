using GlobeHire.Application.Abstractions;
using GlobeHire.Application.WorkforcePlans;
using GlobeHire.Domain.Common;
using GlobeHire.Domain.HireCases;
using GlobeHire.Domain.WorkforcePlans;

namespace GlobeHire.Application.HireCases;

public interface IHireCaseRepository
{
    Task AddAsync(HireCase hireCase, CancellationToken cancellationToken);
    Task<HireCase?> FindAsync(ClientId clientId, Guid id, CancellationToken cancellationToken);
}

public sealed record OpenHireCaseCommand(
    string ClientId,
    string WorkforcePlanId,
    string CandidateName,
    string CandidateEmail);

public sealed class OpenHireCaseHandler(
    IWorkforcePlanRepository workforcePlans,
    IHireCaseRepository hireCases,
    IIdGenerator idGenerator,
    IClock clock)
{
    public async Task<HireCase> HandleAsync(OpenHireCaseCommand command, CancellationToken cancellationToken)
    {
        var clientId = ClientId.Parse(command.ClientId);
        var planId = WorkforcePlanId.Parse(command.WorkforcePlanId);
        var plan = await workforcePlans.FindAsync(clientId, planId, cancellationToken).ConfigureAwait(false);
        if (plan is null)
        {
            throw new BusinessRuleViolationException("workforce_plan.not_found", "Workforce plan was not found.");
        }

        var hireCase = HireCase.Open(
            idGenerator.NewId(),
            clientId,
            planId,
            command.CandidateName,
            command.CandidateEmail,
            clock.UtcNow);
        await hireCases.AddAsync(hireCase, cancellationToken).ConfigureAwait(false);
        return hireCase;
    }
}

public sealed class ChangeHireCaseHandler(IHireCaseRepository hireCases, IClock clock)
{
    public async Task<HireCase?> CompleteTaskAsync(
        string clientId,
        Guid id,
        string taskCode,
        int expectedVersion,
        CancellationToken cancellationToken)
    {
        var hireCase = await hireCases.FindAsync(ClientId.Parse(clientId), id, cancellationToken).ConfigureAwait(false);
        hireCase?.CompleteTask(taskCode, expectedVersion, clock.UtcNow);
        return hireCase;
    }

    public async Task<HireCase?> SubmitAsync(
        string clientId,
        Guid id,
        int expectedVersion,
        CancellationToken cancellationToken)
    {
        var hireCase = await hireCases.FindAsync(ClientId.Parse(clientId), id, cancellationToken).ConfigureAwait(false);
        hireCase?.SubmitForReview(expectedVersion);
        return hireCase;
    }

    public Task<HireCase?> GetAsync(string clientId, Guid id, CancellationToken cancellationToken) =>
        hireCases.FindAsync(ClientId.Parse(clientId), id, cancellationToken);
}
