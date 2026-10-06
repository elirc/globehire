# Solutions and review rubric

## Exercise answers

**1. Route to key.** The value enters at the route group `clients/{clientId}/hire-cases` (`HireCaseEndpoints.cs:13`), is passed unchanged as a `string` to `ChangeHireCaseHandler`, becomes a `ClientId` at `HireCaseUseCases.cs:58` (trimmed, 1–64 characters, otherwise `client_id.invalid`), and is half of the dictionary key at `InMemoryHireCaseRepository.cs:9`. A unique Guid does not establish authority. IDs leak through logs, links, and other responses, and future storage may use guessable keys. Keeping the client in the repository signature makes the unsafe lookup impossible to write by accident.

**2. Version ledger.** 1 → 2 → 3 → 4 after the three completions, then 5 after submission. The stale call with version 1 after `identity` throws `concurrency.version_mismatch` from `EnsureVersion` before the task index is even looked up, so `work-eligibility` keeps `IsComplete = false` and `CompletedAt = null`, `identity` keeps its timestamp, and Version stays 2.

**3. Extra rows.** (a) Unknown Guid: `FindAsync` returns null, the `?.` skips the aggregate, and the endpoint answers 404. The response is identical to the wrong-client case, which is what keeps foreign IDs from being confirmed. (b) Repeat with current version 2: `EnsureVersion` passes, the task is found already complete, and `HireCase.cs:99-102` returns early with no timestamp change and no increment, so the ETag stays `"2"`. With version 1 the version check runs first and throws. The order separates a harmless replay from a stale command.

**4. The unharvested race.** Sprint 14 fixed offers by moving the mutation inside the repository lock (`InMemoryOfferRepository.MutateAsync`, the `mutation(offer)` call at line 44). Hire cases never got that fix. `FindAsync` releases `_gate` when it returns, and `CompleteTaskAsync` then calls `CompleteTask` on the shared object at `HireCaseUseCases.cs:59` with no lock. Two requests carrying `If-Match: "1"` for different tasks can both read `Version == 1` in `EnsureVersion` before either reaches `Version++`. Both tasks then complete, Version may land on 3 or on 2 (the `++` is not atomic), and one client was told it won against a version it never saw. `List<T>` writes from two threads are also unsafe. This is the same class of defect as flaw-harvest row 4. The fix is the offer pattern: a `MutateAsync(clientId, id, mutation)` port, plus a two-writer test copied from `OfferConcurrencyTests`.

**5. Resubmission.** `SubmitForReview` checks only the version and the checklist. On a case that is already `ReadyForReview` with version 5, both checks pass, `Status` is set to the value it already has, and Version becomes 6. The ETag changes although nothing visible did, so any client holding `"5"` now gets a spurious conflict. Decide which rule you want and pin it: either reject with a new code such as `hire_case.not_draft`, or treat it as an idempotent no-op that returns without incrementing, as `CompleteTask` does for a finished task. Either is defensible; leaving it unspecified is not.

**6. Unscoped lookup review.** A strong comment says: "`FindAsync(Guid)` bypasses the `(ClientId, Guid)` key that makes `ChangeHireCaseHandler` return null for foreign cases. Once it exists, nothing stops a future handler from using it, and the 404 for foreign IDs silently becomes a 200. Give the batch job its own narrowly named port in its own module, and add an architecture or reflection test that fails if `IHireCaseRepository` gains a method without a `ClientId` parameter."

**7. Durable sketch.** Tenant identity comes from a validated token claim and is compared with the route before the handler runs. Every query filters on both `client_id` and `id`. The write is a conditional `UPDATE ... AND version = @expected`; zero rows affected means a 409/422 conflict. That single statement is also the cure for exercise 4, because the database performs the compare-and-swap atomically, whatever the process count.

## Senior review notes (verified against current source)

- **Two offers per hire case.** `OfferHandler.CreateAsync` checks only that the hire case is `ReadyForReview`. `InMemoryOfferRepository` keys by `(ClientId, OfferId)`, so posting `/offers` twice for one hire case creates two independent offers that can both be approved and issued. Employment is unique per offer (`InMemoryEmploymentRepository._byOffer`), not per hire case. Ask the product owner whether a candidate can hold two live offers before choosing a fix.
- **404 versus 422 for foreign references.** A foreign ID in the URL returns 404. A foreign ID in the body (`hireCaseId` on `POST /offers`, `offerId` on `POST /employments`) throws `*.not_found`, which `ApiExceptionHandler` maps to 422. Both hide existence, but API consumers see two conventions. Document the rule ("path → 404, body reference → 422") or unify it.
- **Approver identity is caller-supplied.** `ApproveOfferRequest(string Role, string ActorId)` comes from the JSON body, so separation of duties (`EmploymentOffer.cs:67-72`) compares strings the caller chose. The tests prove the rule's logic, not its security. `training/incidents/001-approver-identity.md` explores what happens when the identity source changes.
- **Mutation without a save.** Because the in-memory repository returns live references, "find then mutate" is also the commit. A durable adapter needs an explicit save step. Write the port so a forgotten save is a compile error (for example `MutateAsync`), not a silent no-op.

## Review rubric

Accept a submission when: negative tests inspect unchanged state as well as the null or 404; clocks are fixed and timestamps exact; stale-version tests show the untouched task and Version; concurrency claims come with a two-writer test; and the write-up states its limits (single-process memory store, no verified principal, no distributed transaction). Reject an extra unscoped repository method, assertions that depend on random IDs, and timing windows used instead of deterministic ordering.

## Source excerpt

From [tests/GlobeHire.Domain.Tests/HireCases/HireCaseTests.cs](../tests/GlobeHire.Domain.Tests/HireCases/HireCaseTests.cs).

```cs
public sealed class HireCaseTests
{
    [Fact]
    public void Submit_IncompleteChecklist_IsRejected()
    {
        var hireCase = Create();

        var exception = Assert.Throws<BusinessRuleViolationException>(() => hireCase.SubmitForReview(1));

        Assert.Equal("hire_case.tasks_incomplete", exception.Code);
        Assert.Equal(HireCaseStatus.Draft, hireCase.Status);
    }

    [Fact]
    public void CompleteAllThenSubmit_AdvancesStateAndVersion()
    {
        var hireCase = Create();
        hireCase.CompleteTask("identity", 1, DateTimeOffset.UtcNow);
        hireCase.CompleteTask("work-eligibility", 2, DateTimeOffset.UtcNow);
        hireCase.CompleteTask("payment-details", 3, DateTimeOffset.UtcNow);

        hireCase.SubmitForReview(4);

        Assert.Equal(HireCaseStatus.ReadyForReview, hireCase.Status);
        Assert.Equal(5, hireCase.Version);
    }

    [Fact]
    public void CompleteTask_StaleVersion_IsRejectedBeforeMutation()
    {
        var hireCase = Create();
        hireCase.CompleteTask("identity", 1, DateTimeOffset.UtcNow);

        var exception = Assert.Throws<BusinessRuleViolationException>(
            () => hireCase.CompleteTask("work-eligibility", 1, DateTimeOffset.UtcNow));

        Assert.Equal("concurrency.version_mismatch", exception.Code);
        Assert.False(hireCase.Tasks.Single(task => task.Code == "work-eligibility").IsComplete);
    }

    private static HireCase Create() =>
        HireCase.Open(
            Guid.NewGuid(),
            ClientId.Parse("client-acme"),
            WorkforcePlanId.New(Guid.NewGuid()),
            "Ada Lovelace",
            "ada@example.com",
            DateTimeOffset.UtcNow);
}
```

## Course navigation

[README](README.md) / [01-CODEBASE-MAP](01-CODEBASE-MAP.md) / [02-CONCEPTS](02-CONCEPTS.md) / [03-WORKED-CHANGE](03-WORKED-CHANGE.md) / [04-TESTING-AND-DEBUGGING](04-TESTING-AND-DEBUGGING.md) / [05-PRACTICE](05-PRACTICE.md) / [06-SOLUTIONS-AND-REVIEW](06-SOLUTIONS-AND-REVIEW.md) / [07-TRACE-LAB](07-TRACE-LAB.md) / [VERIFICATION](VERIFICATION.md)
