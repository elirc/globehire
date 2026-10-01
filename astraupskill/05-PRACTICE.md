# Practice: extend the matrix

Before coding, write a prediction table with columns for requested client, expected version, requested action, returned value, final status, final version, and changed task. Populate four rows from the worked tests. Then add two proposed rows without implementation: an unknown Guid under the owner client, and a repeated completion of the already-complete identity task using the current version. Explain which layer decides each outcome.

Exercise one is a reading task. Trace the client value from the endpoint route to ClientId.Parse and into the dictionary key. Record every location where the value could be lost or replaced. Decide whether a globally unique case ID would make tenant scope unnecessary. Your answer should explain why defense at the repository contract still reduces accidental cross-client reads even when IDs are hard to guess.

Exercise two is a state task. Starting at Version 1, list the expected version after each successful checklist completion and after submission. Insert a stale work-eligibility attempt after identity. Predict all task timestamps after the failure. Confirm your prediction against the domain code before running tests.

Exercise three is a design task. Sketch a durable repository method with the same `FindAsync(ClientId, Guid)` signature. State where an authenticated principal would become a ClientId, how a database query would include both columns, and how an update would compare the expected version. Do not implement a database or authentication provider for this exercise.

Exercise four is a review task. Imagine a refactor that adds `FindAsync(Guid id)` for an internal batch job. Write a review comment describing the risk, the safer alternative, and a regression that would expose misuse. Your rubric is specific: reference the handler, repository key, aggregate Version, and the outward not-found behavior. General statements about “security” are insufficient.

## Source excerpt

From [src/GlobeHire.Api/HireCases/HireCaseEndpoints.cs](../src/GlobeHire.Api/HireCases/HireCaseEndpoints.cs).

```cs
internal static class HireCaseEndpoints
{
    public static IEndpointRouteBuilder MapHireCaseEndpoints(this IEndpointRouteBuilder endpoints)
    {
        var group = endpoints.MapGroup("/api/v1/clients/{clientId}/hire-cases").WithTags("Hire cases");
        group.MapPost("/", OpenAsync).Produces<HireCaseResponse>(StatusCodes.Status201Created);
        group.MapGet("/{id:guid}", GetAsync).Produces<HireCaseResponse>().Produces(StatusCodes.Status404NotFound);
        group.MapPut("/{id:guid}/tasks/{taskCode}/complete", CompleteTaskAsync).Produces<HireCaseResponse>();
        group.MapPost("/{id:guid}/submit", SubmitAsync).Produces<HireCaseResponse>();
        return endpoints;
    }

    private static async Task<IResult> OpenAsync(
        string clientId,
        OpenHireCaseRequest request,
        OpenHireCaseHandler handler,
        CancellationToken cancellationToken)
    {
        var hireCase = await handler.HandleAsync(
            new OpenHireCaseCommand(
                clientId,
                request.WorkforcePlanId.ToString("D"),
                request.CandidateName,
                request.CandidateEmail),
            cancellationToken).ConfigureAwait(false);
        return TypedResults.Created($"/api/v1/clients/{clientId}/hire-cases/{hireCase.Id:D}", ToResponse(hireCase));
    }

    private static async Task<IResult> GetAsync(
        string clientId,
        Guid id,
        ChangeHireCaseHandler handler,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var hireCase = await handler.GetAsync(clientId, id, cancellationToken).ConfigureAwait(false);
        return Respond(hireCase, context);
    }

    private static async Task<IResult> CompleteTaskAsync(
        string clientId,
        Guid id,
        string taskCode,
        [FromHeader(Name = "If-Match")] string? ifMatch,
        ChangeHireCaseHandler handler,
        HttpContext context,
        CancellationToken cancellationToken)
    {
        var hireCase = await handler.CompleteTaskAsync(
            clientId,
            id,
            taskCode,
            ParseVersion(ifMatch),
            cancellationToken).ConfigureAwait(false);
        return Respond(hireCase, context);
```

## Course navigation

[README](README.md) / [01-CODEBASE-MAP](01-CODEBASE-MAP.md) / [02-CONCEPTS](02-CONCEPTS.md) / [03-WORKED-CHANGE](03-WORKED-CHANGE.md) / [04-TESTING-AND-DEBUGGING](04-TESTING-AND-DEBUGGING.md) / [05-PRACTICE](05-PRACTICE.md) / [06-SOLUTIONS-AND-REVIEW](06-SOLUTIONS-AND-REVIEW.md) / [07-TRACE-LAB](07-TRACE-LAB.md) / [VERIFICATION](VERIFICATION.md)
