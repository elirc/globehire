# GlobeHire tenant-boundary lab

This course uses one small GlobeHire workflow to teach a large engineering idea: a route identifier is not just text, but part of the authority for every read and mutation. The worked path begins at the hire-case HTTP route, passes the client string into ChangeHireCaseHandler, parses it as ClientId, and reaches InMemoryHireCaseRepository. That repository keys records by the pair `(ClientId, Guid)`. The aggregate then checks its optimistic Version and records a UTC completion time. Read those layers in that order before changing anything.

The improvement is deliberately a regression suite rather than a production rewrite. Current source already scopes hire-case lookup by client, uses explicit versions, and records completion history. What was missing was an application-level proof that the composition remains safe. The new tests create a case owned by `client-acme`, attempt completion and submission as `client-other`, and verify the owner object is unchanged. A valid owner call must complete exactly one task, retain the fixed UTC clock value, and increment Version exactly once. A stale owner call must throw the stable `concurrency.version_mismatch` business error without altering the second task.

Work through the chapters in order. Keep two mental ledgers: identity scope and state transition. For every call, predict the repository key, returned value, task flags, timestamps, and version before reading the assertion. The project is a teaching system with singleton in-memory repositories and no production authentication, durable database, or distributed concurrency guarantee. The tests establish the behavior of this boundary in one process. They do not turn a route client ID into verified identity. That limitation is part of the exercise: good tests state exactly what they prove.

## Source excerpt

From [src/GlobeHire.Application/HireCases/HireCaseUseCases.cs](../src/GlobeHire.Application/HireCases/HireCaseUseCases.cs).

```cs
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
```

## Course navigation

[README](README.md) / [01-CODEBASE-MAP](01-CODEBASE-MAP.md) / [02-CONCEPTS](02-CONCEPTS.md) / [03-WORKED-CHANGE](03-WORKED-CHANGE.md) / [04-TESTING-AND-DEBUGGING](04-TESTING-AND-DEBUGGING.md) / [05-PRACTICE](05-PRACTICE.md) / [06-SOLUTIONS-AND-REVIEW](06-SOLUTIONS-AND-REVIEW.md) / [07-TRACE-LAB](07-TRACE-LAB.md) / [VERIFICATION](VERIFICATION.md)
