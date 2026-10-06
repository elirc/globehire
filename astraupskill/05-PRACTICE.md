# Practice: extend the matrix

Each exercise has a **Goal** and a **Check**. Do the reading exercises with the source open; you do not need to run anything until exercise 5. Answers are in [06-SOLUTIONS-AND-REVIEW](06-SOLUTIONS-AND-REVIEW.md).

## Exercise 1: route to dictionary key

**Goal:** trace the `{clientId}` route value from HTTP to the dictionary key, naming every file and line where it is passed, parsed, or used. Then decide whether a globally unique case ID would make tenant scope unnecessary.

**Check:** your trace names five places: the route group in `HireCaseEndpoints.cs:13`, the handler call in `CompleteTaskAsync`, `ClientId.Parse` in `HireCaseUseCases.cs:58`, `IHireCaseRepository.FindAsync`, and the `(ClientId, Guid)` key at `InMemoryHireCaseRepository.cs:9`. `git grep -n "FindAsync(ClientId.Parse" src` should show the three handler call sites (lines 58, 69, 75); if your trace missed one, read it now.

## Exercise 2: version ledger

**Goal:** starting from Version 1, write the version after each of the three task completions and after submission. Then insert a stale `work-eligibility` call with version 1 right after `identity` and predict every task's `IsComplete` and `CompletedAt`.

**Check:** compare against `HireCaseTests.CompleteAllThenSubmit_AdvancesStateAndVersion` (final Version 5) and `HireCaseTenantIsolationTests.CompleteTask_StaleOwnerVersion_PreservesUnchangedTask` (Version stays 2, `work-eligibility` stays incomplete with a null timestamp). Both tests are in `tests/`.

## Exercise 3: the two extra rows

**Goal:** add two rows to the prediction matrix from [03-WORKED-CHANGE](03-WORKED-CHANGE.md): (a) an unknown Guid under `client-acme`; (b) completing `identity` again with the current version (2). For each, predict the return value, final Version, and which layer decides.

**Check:** (a) must give the same outcome as the wrong-client row, because `FindAsync` cannot tell them apart. (b) must not change Version; find the early `return` in `HireCase.cs:99-102` that proves it, and explain why calling with version 1 instead throws.

## Exercise 4: the race the flaw harvest missed

**Goal:** read row 4 of `docs/quality/FLAW-HARVEST.md` ("Approval mutations occurred after an unlocked repository read"). Compare how `OfferHandler.ApproveAsync` mutates an offer with how `ChangeHireCaseHandler.CompleteTaskAsync` mutates a hire case. Decide whether the Sprint 14 fix applies to hire cases.

**Check:** `git grep -n "MutateAsync" src` finds matches in the offer files only. Your answer states where the hire-case lock is released (`InMemoryHireCaseRepository.cs`, end of `FindAsync`) and which two lines then run unlocked (`HireCaseUseCases.cs:58-59`), and describes one interleaving of two requests carrying `If-Match: "1"` that both pass `EnsureVersion`.

## Exercise 5: resubmission

**Goal:** predict what happens when the owner calls `SubmitAsync` a second time, with the current version, on a case that is already `ReadyForReview`. Then write a domain test in `tests/GlobeHire.Domain.Tests/HireCases/HireCaseTests.cs` that pins the behaviour you think is correct.

**Check:** reading `SubmitForReview` (`HireCase.cs:108-120`) shows there is no `Status` guard, so today the call succeeds and Version moves from 5 to 6 with no visible state change. If your test asserts a rejection, it should fail with `dotnet test tests/GlobeHire.Domain.Tests --filter FullyQualifiedName~HireCaseTests`. That red test is the expected outcome here: you have found a missing rule, not broken the build.

## Exercise 6: review an unscoped lookup

**Goal:** imagine a pull request that adds `FindAsync(Guid id)` to `IHireCaseRepository` for an internal batch job. Write the review comment.

**Check:** the comment names the handler that could start calling it, the composite key it bypasses, why the outward 404 would disappear for foreign IDs, a safer alternative (a separate, narrowly named port owned by the batch job), and a regression test that fails if any request path calls the unscoped method. A comment that only says "security risk" does not pass.

## Exercise 7: durable design sketch

**Goal:** sketch, without implementing, a SQL-backed `IHireCaseRepository`.

**Check:** the sketch (1) takes `ClientId` from an authenticated claim, not the route; (2) puts both `client_id` and `id` in every `WHERE`; (3) saves with `UPDATE ... WHERE id = @id AND client_id = @client AND version = @expected`, treating zero affected rows as a conflict; (4) explains why exercise 4's race disappears once that conditional update exists.

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
