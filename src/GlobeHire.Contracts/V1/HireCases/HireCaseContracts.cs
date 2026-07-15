namespace GlobeHire.Contracts.V1.HireCases;

public sealed record OpenHireCaseRequest(Guid WorkforcePlanId, string CandidateName, string CandidateEmail);

public sealed record HireCaseResponse(
    Guid Id,
    string ClientId,
    Guid WorkforcePlanId,
    string CandidateName,
    string CandidateEmail,
    string Status,
    int Version,
    IReadOnlyList<OnboardingTaskResponse> Tasks);

public sealed record OnboardingTaskResponse(
    string Code,
    string Description,
    bool IsComplete,
    DateTimeOffset? CompletedAt);
