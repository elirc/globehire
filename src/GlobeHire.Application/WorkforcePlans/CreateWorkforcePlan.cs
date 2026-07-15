using GlobeHire.Application.Abstractions;
using GlobeHire.Domain.Common;
using GlobeHire.Domain.WorkforcePlans;

namespace GlobeHire.Application.WorkforcePlans;

public sealed record CreateWorkforcePlanCommand(
    string ClientId,
    string IdempotencyKey,
    string RoleTitle,
    string CountryCode,
    DateOnly ProposedStartDate,
    decimal AnnualCompensation,
    string Currency);

public sealed record CreateWorkforcePlanResult(WorkforcePlan Plan, bool WasCreated);

public sealed class CreateWorkforcePlanHandler
{
    private readonly IWorkforcePlanRepository _repository;
    private readonly IClock _clock;
    private readonly IIdGenerator _idGenerator;
    private readonly ICountryPolicyRepository _countryPolicies;

    public CreateWorkforcePlanHandler(
        IWorkforcePlanRepository repository,
        IClock clock,
        IIdGenerator idGenerator,
        ICountryPolicyRepository countryPolicies)
    {
        _repository = repository;
        _clock = clock;
        _idGenerator = idGenerator;
        _countryPolicies = countryPolicies;
    }

    public async Task<CreateWorkforcePlanResult> HandleAsync(
        CreateWorkforcePlanCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var clientId = ClientId.Parse(command.ClientId);
        var annualCompensation = Money.Of(command.AnnualCompensation, command.Currency);
        var country = CountryCode.Parse(command.CountryCode);
        var policies = await _countryPolicies.ListAsync(country, cancellationToken).ConfigureAwait(false);
        var policy = CountryPolicySelector.Select(country, command.ProposedStartDate, policies);
        var plan = WorkforcePlan.Create(
            WorkforcePlanId.New(_idGenerator.NewId()),
            clientId,
            command.IdempotencyKey,
            command.RoleTitle,
            country,
            command.ProposedStartDate,
            annualCompensation,
            policy,
            _clock.Today,
            _clock.UtcNow);

        var stored = await _repository.AddOrGetAsync(plan, cancellationToken).ConfigureAwait(false);
        return new CreateWorkforcePlanResult(stored.Plan, stored.WasAdded);
    }
}
