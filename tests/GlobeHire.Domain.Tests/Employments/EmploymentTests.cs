using GlobeHire.Domain.Common;
using GlobeHire.Domain.Employments;
using GlobeHire.Domain.HireCases;
using GlobeHire.Domain.Offers;
using GlobeHire.Domain.WorkforcePlans;

namespace GlobeHire.Domain.Tests.Employments;

public sealed class EmploymentTests
{
    [Fact]
    public void Activate_WithoutIssuedContract_IsRejected()
    {
        var offer = ReadyOffer();
        var exception = Assert.Throws<BusinessRuleViolationException>(() => Employment.Activate(
            Guid.NewGuid(), offer, "worker-1", new DateOnly(2026, 8, 1), new DateOnly(2026, 7, 14), DateTimeOffset.UtcNow));
        Assert.Equal("employment.contract_not_issued", exception.Code);
    }

    [Fact]
    public void Activate_IssuedOffer_PreservesSourceProvenance()
    {
        var offer = ReadyOffer();
        offer.Approve(ApprovalRole.Finance, "finance", 1, DateTimeOffset.UtcNow);
        offer.Approve(ApprovalRole.Legal, "legal", 2, DateTimeOffset.UtcNow);
        var contract = offer.IssueContract(Guid.NewGuid(), "employment-v1", 3, DateTimeOffset.UtcNow);
        var employment = Employment.Activate(Guid.NewGuid(), offer, "worker-1", new DateOnly(2026, 8, 1), new DateOnly(2026, 7, 14), DateTimeOffset.UtcNow);
        Assert.Equal(EmploymentStatus.Active, employment.Status);
        Assert.Equal(offer.Id, employment.OfferId);
        Assert.Equal(contract.Id, employment.ContractId);
    }

    private static EmploymentOffer ReadyOffer()
    {
        var hireCase = HireCase.Open(Guid.NewGuid(), ClientId.Parse("client-acme"), WorkforcePlanId.New(Guid.NewGuid()), "Ada Lovelace", "ada@example.com", DateTimeOffset.UtcNow);
        hireCase.CompleteTask("identity", 1, DateTimeOffset.UtcNow);
        hireCase.CompleteTask("work-eligibility", 2, DateTimeOffset.UtcNow);
        hireCase.CompleteTask("payment-details", 3, DateTimeOffset.UtcNow);
        hireCase.SubmitForReview(4);
        return EmploymentOffer.Create(Guid.NewGuid(), hireCase, DateTimeOffset.UtcNow);
    }
}
