using GlobeHire.Domain.Common;

namespace GlobeHire.Domain.Compliance;

public enum ComplianceCaseStatus
{
    CollectingEvidence,
    ReadyForReview,
    Approved,
    Expired,
}

public sealed record ComplianceEvidence(
    string Type,
    string Reference,
    DateOnly? ExpiresOn,
    DateTimeOffset RecordedAt);

public sealed class ComplianceCase
{
    private readonly Dictionary<string, ComplianceEvidence> _evidence = new(StringComparer.Ordinal);

    public ComplianceCase(
        Guid id,
        ClientId clientId,
        string? subjectReference,
        CountryCode country,
        IEnumerable<string> requiredEvidenceTypes)
    {
        if (id == Guid.Empty || string.IsNullOrWhiteSpace(subjectReference))
        {
            throw new BusinessRuleViolationException("compliance.case_invalid", "Compliance case is invalid.");
        }

        var required = requiredEvidenceTypes
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Select(value => value.Trim())
            .ToHashSet(StringComparer.Ordinal);
        if (required.Count == 0)
        {
            throw new BusinessRuleViolationException(
                "compliance.requirements_empty",
                "At least one evidence type is required.");
        }

        Id = id;
        ClientId = clientId;
        SubjectReference = subjectReference.Trim();
        Country = country;
        RequiredEvidenceTypes = required;
    }

    public Guid Id { get; }
    public ClientId ClientId { get; }
    public string SubjectReference { get; }
    public CountryCode Country { get; }
    public IReadOnlySet<string> RequiredEvidenceTypes { get; }
    public IReadOnlyCollection<ComplianceEvidence> Evidence => _evidence.Values;
    public ComplianceCaseStatus Status { get; private set; }

    public void RecordEvidence(
        string? type,
        string? reference,
        DateOnly? expiresOn,
        DateOnly today,
        DateTimeOffset now)
    {
        if (Status != ComplianceCaseStatus.CollectingEvidence || type is null ||
            !RequiredEvidenceTypes.Contains(type) || string.IsNullOrWhiteSpace(reference))
        {
            throw new BusinessRuleViolationException(
                "compliance.evidence_invalid",
                "Evidence is not required or the case is not collecting evidence.");
        }

        if (expiresOn < today)
        {
            throw new BusinessRuleViolationException("compliance.evidence_expired", "Expired evidence cannot be recorded.");
        }

        _evidence[type] = new ComplianceEvidence(type, reference.Trim(), expiresOn, now.ToUniversalTime());
    }

    public void SubmitForReview(DateOnly today)
    {
        if (RequiredEvidenceTypes.Any(type => !_evidence.ContainsKey(type)))
        {
            throw new BusinessRuleViolationException(
                "compliance.evidence_incomplete",
                "Every required evidence type must be recorded.");
        }

        if (_evidence.Values.Any(item => item.ExpiresOn < today))
        {
            Status = ComplianceCaseStatus.Expired;
            throw new BusinessRuleViolationException("compliance.evidence_expired", "Case evidence has expired.");
        }

        Status = ComplianceCaseStatus.ReadyForReview;
    }

    public void Approve()
    {
        if (Status != ComplianceCaseStatus.ReadyForReview)
        {
            throw new BusinessRuleViolationException(
                "compliance.not_ready",
                "Only review-ready compliance cases can be approved.");
        }

        Status = ComplianceCaseStatus.Approved;
    }
}
