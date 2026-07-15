using GlobeHire.Domain.Common;
using GlobeHire.Domain.HireCases;

namespace GlobeHire.Domain.Offers;

public enum ApprovalRole { Finance, Legal }
public enum OfferStatus { AwaitingApprovals, Approved, ContractIssued }
public sealed record OfferApproval(ApprovalRole Role, string ActorId, DateTimeOffset ApprovedAt);
public sealed record ContractRecord(Guid Id, int Revision, string TemplateKey, DateTimeOffset IssuedAt);

public sealed class EmploymentOffer
{
    private readonly List<OfferApproval> _approvals = [];

    private EmploymentOffer(Guid id, HireCase hireCase, DateTimeOffset createdAt)
    {
        Id = id;
        ClientId = hireCase.ClientId;
        HireCaseId = hireCase.Id;
        CreatedAt = createdAt.ToUniversalTime();
    }

    public Guid Id { get; }
    public ClientId ClientId { get; }
    public Guid HireCaseId { get; }
    public OfferStatus Status { get; private set; }
    public int Version { get; private set; } = 1;
    public DateTimeOffset CreatedAt { get; }
    public IReadOnlyList<OfferApproval> Approvals => _approvals;
    public ContractRecord? Contract { get; private set; }

    public static EmploymentOffer Create(Guid id, HireCase hireCase, DateTimeOffset now)
    {
        ArgumentNullException.ThrowIfNull(hireCase);
        if (id == Guid.Empty)
        {
            throw new BusinessRuleViolationException("offer.id_invalid", "Offer ID cannot be empty.");
        }

        if (hireCase.Status != HireCaseStatus.ReadyForReview)
        {
            throw new BusinessRuleViolationException("offer.hire_case_not_ready", "Hire case must be ready for review.");
        }

        return new EmploymentOffer(id, hireCase, now);
    }

    public void Approve(ApprovalRole role, string? actorId, int expectedVersion, DateTimeOffset now)
    {
        EnsureVersion(expectedVersion);
        if (Status != OfferStatus.AwaitingApprovals)
        {
            throw new BusinessRuleViolationException("offer.not_approvable", "Offer is no longer awaiting approvals.");
        }

        var actor = actorId?.Trim();
        if (string.IsNullOrWhiteSpace(actor))
        {
            throw new BusinessRuleViolationException("approval.actor_required", "Approval actor is required.");
        }

        if (_approvals.Any(approval => approval.Role == role))
        {
            return;
        }

        if (_approvals.Any(approval => string.Equals(approval.ActorId, actor, StringComparison.OrdinalIgnoreCase)))
        {
            throw new BusinessRuleViolationException(
                "approval.separation_of_duties",
                "One actor cannot satisfy multiple required approval roles.");
        }

        _approvals.Add(new OfferApproval(role, actor, now.ToUniversalTime()));
        Version++;
        if (Enum.GetValues<ApprovalRole>().All(required => _approvals.Any(item => item.Role == required)))
        {
            Status = OfferStatus.Approved;
        }
    }

    public ContractRecord IssueContract(Guid contractId, string? templateKey, int expectedVersion, DateTimeOffset now)
    {
        EnsureVersion(expectedVersion);
        if (Status != OfferStatus.Approved)
        {
            throw new BusinessRuleViolationException("offer.approvals_incomplete", "All approvals are required before issue.");
        }

        if (contractId == Guid.Empty || string.IsNullOrWhiteSpace(templateKey))
        {
            throw new BusinessRuleViolationException("contract.metadata_invalid", "Contract metadata is invalid.");
        }

        Contract = new ContractRecord(contractId, 1, templateKey.Trim(), now.ToUniversalTime());
        Status = OfferStatus.ContractIssued;
        Version++;
        return Contract;
    }

    private void EnsureVersion(int expectedVersion)
    {
        if (Version != expectedVersion)
        {
            throw new BusinessRuleViolationException("concurrency.version_mismatch", "The offer changed; reload and retry.");
        }
    }
}
