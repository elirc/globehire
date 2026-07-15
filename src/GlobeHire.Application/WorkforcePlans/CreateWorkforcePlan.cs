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
    private readonly WorkforcePlanPricingOptions _pricing;

    public CreateWorkforcePlanHandler(
        IWorkforcePlanRepository repository,
        IClock clock,
        IIdGenerator idGenerator,
        WorkforcePlanPricingOptions pricing)
    {
        _repository = repository;
        _clock = clock;
        _idGenerator = idGenerator;
        _pricing = pricing;
    }

    public async Task<CreateWorkforcePlanResult> HandleAsync(
        CreateWorkforcePlanCommand command,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(command);

        var clientId = ClientId.Parse(command.ClientId);
        var annualCompensation = Money.Of(command.AnnualCompensation, command.Currency);
        var plan = WorkforcePlan.Create(
            WorkforcePlanId.New(_idGenerator.NewId()),
            clientId,
            command.IdempotencyKey,
            command.RoleTitle,
            CountryCode.Parse(command.CountryCode),
            command.ProposedStartDate,
            annualCompensation,
            Money.Of(_pricing.MonthlyServiceFee, annualCompensation.Currency),
            _clock.Today,
            _clock.UtcNow);

        var stored = await _repository.AddOrGetAsync(plan, cancellationToken).ConfigureAwait(false);
        return new CreateWorkforcePlanResult(stored.Plan, stored.WasAdded);
    }
}
