# Concepts: tenant keys, versions, and audit time

Tenant isolation is strongest when it is structural. The repository does not retrieve a case by Guid and then ask a caller to remember an authorization check. It uses ClientId and Guid together as the lookup key. That makes the common unsafe path—load by resource ID alone—unavailable through IHireCaseRepository. The handler still does not authenticate the caller; it trusts the client ID passed from the transport. In a production system, authenticated claims or server-side membership would determine that client. Here we test the scope that the project actually implements.

Optimistic versioning protects a different boundary. A correct tenant can still send stale state. HireCase starts at Version 1. Completing a previously incomplete task changes its record and advances the version. A second mutation claiming version 1 must fail before touching another task. Tenant scope answers “which aggregate may this request see?” Version checks answer “which observed state may this request change?” Treating these as separate questions makes failure diagnosis clearer.

Audit time is an input, not a reason to consult the machine clock throughout domain code. ChangeHireCaseHandler receives IClock and passes UtcNow into CompleteTask. The regression uses a FixedClock so the expected CompletedAt is exact and its offset is zero. A wrong-client attempt never reaches that clocked mutation. A stale attempt reaches the aggregate but fails before assigning a timestamp. Those negative assertions matter as much as the success assertion.

The in-memory lock prevents simultaneous dictionary operations from corrupting the collection, and nothing more. `FindAsync` releases it before returning, so `CompleteTask` and `SubmitForReview` run on the shared aggregate with no lock held. The version check is therefore a check-then-act, not a compare-and-swap. Offers avoid this by mutating inside `InMemoryOfferRepository.MutateAsync`; hire cases do not (see exercise 4). None of it substitutes for database transactions across processes. Likewise, returning null for another tenant avoids revealing whether the Guid exists, but no authentication is proven. Keep these limits beside the concepts so future changes do not overstate a teaching repository’s guarantees.

## Source excerpt

From [src/GlobeHire.Domain/HireCases/HireCase.cs](../src/GlobeHire.Domain/HireCases/HireCase.cs).

```cs
public void CompleteTask(string taskCode, int expectedVersion, DateTimeOffset now)
    {
        EnsureVersion(expectedVersion);
        if (Status != HireCaseStatus.Draft)
        {
            throw new BusinessRuleViolationException("hire_case.not_editable", "Submitted hire cases cannot be edited.");
        }

        var index = _tasks.FindIndex(task => string.Equals(task.Code, taskCode, StringComparison.Ordinal));
        if (index < 0)
        {
            throw new BusinessRuleViolationException("hire_case.task_not_found", "Onboarding task was not found.");
        }

        if (_tasks[index].IsComplete)
        {
            return;
        }

        _tasks[index] = _tasks[index] with { IsComplete = true, CompletedAt = now.ToUniversalTime() };
        Version++;
    }

    public void SubmitForReview(int expectedVersion)
    {
        EnsureVersion(expectedVersion);
        if (_tasks.Any(task => !task.IsComplete))
        {
            throw new BusinessRuleViolationException(
                "hire_case.tasks_incomplete",
                "Every onboarding task must be completed before review.");
        }

        Status = HireCaseStatus.ReadyForReview;
        Version++;
    }

    private void EnsureVersion(int expectedVersion)
    {
        if (expectedVersion != Version)
        {
            throw new BusinessRuleViolationException("concurrency.version_mismatch", "The hire case changed; reload and retry.");
        }
    }
}
```

## Course navigation

[README](README.md) / [01-CODEBASE-MAP](01-CODEBASE-MAP.md) / [02-CONCEPTS](02-CONCEPTS.md) / [03-WORKED-CHANGE](03-WORKED-CHANGE.md) / [04-TESTING-AND-DEBUGGING](04-TESTING-AND-DEBUGGING.md) / [05-PRACTICE](05-PRACTICE.md) / [06-SOLUTIONS-AND-REVIEW](06-SOLUTIONS-AND-REVIEW.md) / [07-TRACE-LAB](07-TRACE-LAB.md) / [VERIFICATION](VERIFICATION.md)
