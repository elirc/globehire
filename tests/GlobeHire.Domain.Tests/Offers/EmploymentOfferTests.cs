using GlobeHire.Domain.Common;
using GlobeHire.Domain.HireCases;
using GlobeHire.Domain.Offers;
using GlobeHire.Domain.WorkforcePlans;

namespace GlobeHire.Domain.Tests.Offers;

public sealed class EmploymentOfferTests
{
    [Fact]
    public void IssueBeforeApprovals_IsRejected()
    {
        var offer = CreateOffer();
        var exception = Assert.Throws<BusinessRuleViolationException>(
            () => offer.IssueContract(Guid.NewGuid(), "employment-v1", 1, DateTimeOffset.UtcNow));
        Assert.Equal("offer.approvals_incomplete", exception.Code);
    }

    [Fact]
    public void SameActorForBothRoles_IsRejected()
    {
        var offer = CreateOffer();
        offer.Approve(ApprovalRole.Finance, "reviewer-1", 1, DateTimeOffset.UtcNow);
        var exception = Assert.Throws<BusinessRuleViolationException>(
            () => offer.Approve(ApprovalRole.Legal, "reviewer-1", 2, DateTimeOffset.UtcNow));
        Assert.Equal("approval.separation_of_duties", exception.Code);
    }

    [Fact]
    public void DistinctApprovalsThenIssue_CreatesImmutableContractMetadata()
    {
        var offer = CreateOffer();
        offer.Approve(ApprovalRole.Finance, "finance-1", 1, DateTimeOffset.UtcNow);
        offer.Approve(ApprovalRole.Legal, "legal-1", 2, DateTimeOffset.UtcNow);
        var contract = offer.IssueContract(Guid.NewGuid(), "employment-v1", 3, DateTimeOffset.UtcNow);
        Assert.Equal(OfferStatus.ContractIssued, offer.Status);
        Assert.Equal(1, contract.Revision);
        Assert.Equal(4, offer.Version);
    }

    private static EmploymentOffer CreateOffer()
    {
        var hireCase = HireCase.Open(Guid.NewGuid(), ClientId.Parse("client-acme"), WorkforcePlanId.New(Guid.NewGuid()), "Ada Lovelace", "ada@example.com", DateTimeOffset.UtcNow);
        hireCase.CompleteTask("identity", 1, DateTimeOffset.UtcNow);
        hireCase.CompleteTask("work-eligibility", 2, DateTimeOffset.UtcNow);
        hireCase.CompleteTask("payment-details", 3, DateTimeOffset.UtcNow);
        hireCase.SubmitForReview(4);
        return EmploymentOffer.Create(Guid.NewGuid(), hireCase, DateTimeOffset.UtcNow);
    }
}
