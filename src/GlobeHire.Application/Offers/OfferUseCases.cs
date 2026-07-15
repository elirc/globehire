using GlobeHire.Application.Abstractions;
using GlobeHire.Application.HireCases;
using GlobeHire.Domain.Common;
using GlobeHire.Domain.Offers;

namespace GlobeHire.Application.Offers;

public interface IOfferRepository
{
    Task AddAsync(EmploymentOffer offer, CancellationToken cancellationToken);
    Task<EmploymentOffer?> FindAsync(ClientId clientId, Guid id, CancellationToken cancellationToken);
}

public sealed class OfferHandler(
    IHireCaseRepository hireCases,
    IOfferRepository offers,
    IIdGenerator ids,
    IClock clock)
{
    public async Task<EmploymentOffer> CreateAsync(
        string clientIdValue,
        Guid hireCaseId,
        CancellationToken cancellationToken)
    {
        var clientId = ClientId.Parse(clientIdValue);
        var hireCase = await hireCases.FindAsync(clientId, hireCaseId, cancellationToken).ConfigureAwait(false)
            ?? throw new BusinessRuleViolationException("hire_case.not_found", "Hire case was not found.");
        var offer = EmploymentOffer.Create(ids.NewId(), hireCase, clock.UtcNow);
        await offers.AddAsync(offer, cancellationToken).ConfigureAwait(false);
        return offer;
    }

    public async Task<EmploymentOffer?> ApproveAsync(
        string clientId, Guid id, ApprovalRole role, string actorId, int version, CancellationToken cancellationToken)
    {
        var offer = await offers.FindAsync(ClientId.Parse(clientId), id, cancellationToken).ConfigureAwait(false);
        offer?.Approve(role, actorId, version, clock.UtcNow);
        return offer;
    }

    public async Task<EmploymentOffer?> IssueAsync(
        string clientId, Guid id, string templateKey, int version, CancellationToken cancellationToken)
    {
        var offer = await offers.FindAsync(ClientId.Parse(clientId), id, cancellationToken).ConfigureAwait(false);
        offer?.IssueContract(ids.NewId(), templateKey, version, clock.UtcNow);
        return offer;
    }
}
