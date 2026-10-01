# Trace lab: predict every state

Use one case with ID `d9d59414-c677-49fb-a9b5-31243fa3ec11`, owner `client-acme`, and three incomplete tasks. At checkpoint A the repository contains one key pair, Status is Draft, Version is 1, and no CompletedAt exists. Do not run code yet.

Trace 1 sends client-other, the same ID, identity, and expected version 1. ClientId.Parse succeeds. FindAsync constructs `(client-other, id)`, which does not match `(client-acme, id)`. The handler’s null-conditional call skips CompleteTask. At checkpoint B, predict the exact same owner state as A. The response cannot distinguish an existing foreign case from a nonexistent case.

Trace 2 sends client-acme with version 1. Lookup returns the owner reference. EnsureVersion succeeds; status is editable; identity is found and incomplete. The record is replaced with IsComplete true and CompletedAt from FixedClock, then Version becomes 2. At checkpoint C only identity changed.

Trace 3 sends client-acme, work-eligibility, and stale version 1. Lookup succeeds, but EnsureVersion observes 2 and throws `concurrency.version_mismatch`. At checkpoint D work-eligibility remains incomplete, its timestamp is null, identity retains its earlier audit, and Version remains 2.

Now create a second fixture with all tasks complete and Version 4 but Status Draft. Trace 4 attempts submit as client-other using version 4. Repository lookup returns null before aggregate validation. Status stays Draft and Version stays 4. Finally submit as the owner: the aggregate finds no incomplete tasks, changes Status to ReadyForReview, and increments Version to 5. Compare your trace against the tests. If any prediction differs, identify whether the misunderstanding concerns lookup scope, null-conditional control flow, validation order, or mutation ownership.

## Source excerpt

From [src/GlobeHire.Domain/HireCases/HireCase.cs](../src/GlobeHire.Domain/HireCases/HireCase.cs).

```cs
public sealed class HireCase
{
    private readonly List<OnboardingTask> _tasks;

    private HireCase(
        Guid id,
        ClientId clientId,
        WorkforcePlanId workforcePlanId,
        string candidateName,
        string candidateEmail,
        List<OnboardingTask> tasks,
        DateTimeOffset createdAt)
    {
        Id = id;
        ClientId = clientId;
        WorkforcePlanId = workforcePlanId;
        CandidateName = candidateName;
        CandidateEmail = candidateEmail;
        _tasks = tasks;
        CreatedAt = createdAt;
    }

    public Guid Id { get; }
    public ClientId ClientId { get; }
    public WorkforcePlanId WorkforcePlanId { get; }
    public string CandidateName { get; }
    public string CandidateEmail { get; }
    public HireCaseStatus Status { get; private set; }
    public int Version { get; private set; } = 1;
    public DateTimeOffset CreatedAt { get; }
    public IReadOnlyList<OnboardingTask> Tasks => _tasks;

    public static HireCase Open(
        Guid id,
        ClientId clientId,
        WorkforcePlanId workforcePlanId,
        string? candidateName,
        string? candidateEmail,
        DateTimeOffset now)
    {
        if (id == Guid.Empty)
        {
            throw new BusinessRuleViolationException("hire_case.id_invalid", "Hire case ID cannot be empty.");
        }

        var name = candidateName?.Trim();
        if (name is null || name.Length is < 2 or > 120)
        {
            throw new BusinessRuleViolationException("hire_case.candidate_name_invalid", "Candidate name is invalid.");
        }

        var email = candidateEmail?.Trim().ToLowerInvariant();
        if (email is null || email.Length > 254 || !email.Contains('@'))
        {
            throw new BusinessRuleViolationException("hire_case.candidate_email_invalid", "Candidate email is invalid.");
        }

        return new HireCase(
            id,
            clientId,
            workforcePlanId,
            name,
            email,
            [
                new("identity", "Verify identity", false, null),
                new("work-eligibility", "Verify work eligibility", false, null),
                new("payment-details", "Collect payment details", false, null),
            ],
            now.ToUniversalTime());
    }

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
```

## Course navigation

[README](README.md) / [01-CODEBASE-MAP](01-CODEBASE-MAP.md) / [02-CONCEPTS](02-CONCEPTS.md) / [03-WORKED-CHANGE](03-WORKED-CHANGE.md) / [04-TESTING-AND-DEBUGGING](04-TESTING-AND-DEBUGGING.md) / [05-PRACTICE](05-PRACTICE.md) / [06-SOLUTIONS-AND-REVIEW](06-SOLUTIONS-AND-REVIEW.md) / [07-TRACE-LAB](07-TRACE-LAB.md) / [VERIFICATION](VERIFICATION.md)
