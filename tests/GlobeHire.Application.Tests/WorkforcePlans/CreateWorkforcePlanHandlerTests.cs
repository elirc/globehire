using GlobeHire.Application.Abstractions;
using GlobeHire.Application.WorkforcePlans;
using GlobeHire.Infrastructure.WorkforcePlans;

namespace GlobeHire.Application.Tests.WorkforcePlans;

public sealed class CreateWorkforcePlanHandlerTests
{
    private static readonly Guid FirstId = Guid.Parse("42d8fe41-fdab-4f5b-8dc9-0d584d915e0d");
    private readonly InMemoryWorkforcePlanRepository _repository = new();
    private readonly StubClock _clock = new();

    [Fact]
    public async Task Handle_SameTenantAndKey_ReturnsOriginalPlanWithoutDuplicate()
    {
        var handler = CreateHandler();
        var command = ValidCommand("client-acme", "request-12345678");

        var first = await handler.HandleAsync(command, CancellationToken.None);
        var replay = await handler.HandleAsync(command, CancellationToken.None);

        Assert.True(first.WasCreated);
        Assert.False(replay.WasCreated);
        Assert.Equal(first.Plan.Id, replay.Plan.Id);
        Assert.Equal(1, _repository.Count);
    }

    [Fact]
    public async Task Handle_SameKeyForDifferentTenants_CreatesIndependentPlans()
    {
        var ids = new QueueIdGenerator(FirstId, Guid.Parse("0d7a2596-8557-41f1-862f-7f6af73ae39e"));
        var handler = CreateHandler(ids);

        var acme = await handler.HandleAsync(ValidCommand("client-acme", "request-12345678"), CancellationToken.None);
        var other = await handler.HandleAsync(ValidCommand("client-other", "request-12345678"), CancellationToken.None);

        Assert.NotEqual(acme.Plan.Id, other.Plan.Id);
        Assert.Equal(2, _repository.Count);
    }

    [Fact]
    public async Task Get_DifferentTenantCannotReadPlan()
    {
        var created = await CreateHandler().HandleAsync(
            ValidCommand("client-acme", "request-12345678"),
            CancellationToken.None);
        var getHandler = new GetWorkforcePlanHandler(_repository);

        var result = await getHandler.HandleAsync(
            new GetWorkforcePlanQuery("client-other", created.Plan.Id.ToString()),
            CancellationToken.None);

        Assert.Null(result);
    }

    private CreateWorkforcePlanHandler CreateHandler(IIdGenerator? idGenerator = null) =>
        new(
            _repository,
            _clock,
            idGenerator ?? new QueueIdGenerator(FirstId, Guid.NewGuid()),
            new WorkforcePlanPricingOptions { MonthlyServiceFee = 500m });

    private static CreateWorkforcePlanCommand ValidCommand(string clientId, string idempotencyKey) =>
        new(
            clientId,
            idempotencyKey,
            "Senior Engineer",
            "US",
            new DateOnly(2026, 8, 1),
            120_000m,
            "USD");

    private sealed class StubClock : IClock
    {
        public DateTimeOffset UtcNow => new(2026, 7, 14, 18, 0, 0, TimeSpan.Zero);

        public DateOnly Today => new(2026, 7, 14);
    }

    private sealed class QueueIdGenerator(params Guid[] ids) : IIdGenerator
    {
        private readonly Queue<Guid> _ids = new(ids);

        public Guid NewId() => _ids.Dequeue();
    }
}
