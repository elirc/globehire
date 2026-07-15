using GlobeHire.Domain.Common;
using GlobeHire.Domain.WorkforcePlans;

namespace GlobeHire.Domain.HireCases;

public enum HireCaseStatus
{
    Draft,
    ReadyForReview,
}

public sealed record OnboardingTask(string Code, string Description, bool IsComplete, DateTimeOffset? CompletedAt);

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
        if (expectedVersion != Version)
        {
            throw new BusinessRuleViolationException("concurrency.version_mismatch", "The hire case changed; reload and retry.");
        }
    }
}
