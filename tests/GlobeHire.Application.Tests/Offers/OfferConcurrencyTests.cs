using GlobeHire.Domain.Common;
using GlobeHire.Domain.HireCases;
using GlobeHire.Domain.Offers;
using GlobeHire.Domain.WorkforcePlans;
using GlobeHire.Infrastructure.Offers;

namespace GlobeHire.Application.Tests.Offers;

public sealed class OfferConcurrencyTests
{
    [Fact]
    public async Task ConcurrentApprovals_WithSameVersion_OnlyOneMutationCommits()
    {
        var repository = new InMemoryOfferRepository();
        var offer = ReadyOffer();
        await repository.AddAsync(offer, CancellationToken.None);

        var finance = Task.Run(() => repository.MutateAsync(
            offer.ClientId,
            offer.Id,
            item => item.Approve(ApprovalRole.Finance, "finance", 1, DateTimeOffset.UtcNow),
            CancellationToken.None));
        var legal = Task.Run(() => repository.MutateAsync(
            offer.ClientId,
            offer.Id,
            item => item.Approve(ApprovalRole.Legal, "legal", 1, DateTimeOffset.UtcNow),
            CancellationToken.None));

        var errors = await CaptureErrorsAsync(finance, legal);

        Assert.Single(errors);
        Assert.Equal("concurrency.version_mismatch", errors[0].Code);
        Assert.Single(offer.Approvals);
        Assert.Equal(2, offer.Version);
    }

    private static async Task<IReadOnlyList<BusinessRuleViolationException>> CaptureErrorsAsync(params Task[] tasks)
    {
        var errors = new List<BusinessRuleViolationException>();
        foreach (var task in tasks)
        {
            try
            {
                await task;
            }
            catch (BusinessRuleViolationException exception)
            {
                errors.Add(exception);
            }
        }

        return errors;
    }

    private static EmploymentOffer ReadyOffer()
    {
        var hireCase = HireCase.Open(
            Guid.NewGuid(),
            ClientId.Parse("client-acme"),
            WorkforcePlanId.New(Guid.NewGuid()),
            "Ada Lovelace",
            "ada@example.com",
            DateTimeOffset.UtcNow);
        hireCase.CompleteTask("identity", 1, DateTimeOffset.UtcNow);
        hireCase.CompleteTask("work-eligibility", 2, DateTimeOffset.UtcNow);
        hireCase.CompleteTask("payment-details", 3, DateTimeOffset.UtcNow);
        hireCase.SubmitForReview(4);
        return EmploymentOffer.Create(Guid.NewGuid(), hireCase, DateTimeOffset.UtcNow);
    }
}
