# Codebase map: route to aggregate

Start in `HireCaseEndpoints.cs`. The route contains `clients/{clientId}/hire-cases/{id}` and passes both values to ChangeHireCaseHandler. The endpoint also parses an If-Match header into an expected integer version. This is the transport boundary: HTTP shapes and status codes belong here, while tenant lookup and lifecycle rules remain below it. Next open `HireCaseUseCases.cs`. CompleteTaskAsync parses the client string, asks IHireCaseRepository for the exact client/id pair, and invokes HireCase.CompleteTask only when a record is found. SubmitAsync follows the same pattern. A null result becomes not-found at the endpoint.

The infrastructure layer is intentionally small. `InMemoryHireCaseRepository` owns a dictionary keyed by `(ClientId ClientId, Guid Id)` and protects it with a lock. The pair matters: a globally valid Guid is insufficient. A request with the right case ID and wrong client produces no aggregate reference, so application code has nothing to mutate. Finally, `HireCase.cs` owns the checklist, Version, Status, and CompletedAt timestamps. CompleteTask checks the expected version before checking the task and uses the injected application clock value supplied by the handler.

The new test file sits in GlobeHire.Application.Tests because the claim spans application, infrastructure, and domain code. Domain-only tests already establish transition rules, while an API happy-path test checks cross-client GET. The new suite fills the middle: it composes the real handler and repository with a fixed clock. Map each assertion to an owner. Repository isolation explains null for the wrong client. Aggregate rules explain version rejection. The clock port explains the exact UTC timestamp. This ownership map prevents tests from accidentally asserting a feature in the wrong layer.

## Source excerpt

From [src/GlobeHire.Infrastructure/HireCases/InMemoryHireCaseRepository.cs](../src/GlobeHire.Infrastructure/HireCases/InMemoryHireCaseRepository.cs).

```cs
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
```

## Course navigation

[README](README.md) / [01-CODEBASE-MAP](01-CODEBASE-MAP.md) / [02-CONCEPTS](02-CONCEPTS.md) / [03-WORKED-CHANGE](03-WORKED-CHANGE.md) / [04-TESTING-AND-DEBUGGING](04-TESTING-AND-DEBUGGING.md) / [05-PRACTICE](05-PRACTICE.md) / [06-SOLUTIONS-AND-REVIEW](06-SOLUTIONS-AND-REVIEW.md) / [07-TRACE-LAB](07-TRACE-LAB.md) / [VERIFICATION](VERIFICATION.md)
