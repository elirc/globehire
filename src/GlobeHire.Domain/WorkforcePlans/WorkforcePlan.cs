using GlobeHire.Domain.Common;

namespace GlobeHire.Domain.WorkforcePlans;

public sealed class WorkforcePlan
{
    private WorkforcePlan(
        WorkforcePlanId id,
        ClientId clientId,
        string idempotencyKey,
        string roleTitle,
        CountryCode country,
        DateOnly proposedStartDate,
        Money annualCompensation,
        PlanQuote quote,
        DateTimeOffset createdAt)
    {
        Id = id;
        ClientId = clientId;
        IdempotencyKey = idempotencyKey;
        RoleTitle = roleTitle;
        Country = country;
        ProposedStartDate = proposedStartDate;
        AnnualCompensation = annualCompensation;
        Quote = quote;
        CreatedAt = createdAt;
    }

    public WorkforcePlanId Id { get; }

    public ClientId ClientId { get; }

    public string IdempotencyKey { get; }

    public string RoleTitle { get; }

    public CountryCode Country { get; }

    public DateOnly ProposedStartDate { get; }

    public Money AnnualCompensation { get; }

    public PlanQuote Quote { get; }

    public DateTimeOffset CreatedAt { get; }

    public int Version { get; } = 1;

    public static WorkforcePlan Create(
        WorkforcePlanId id,
        ClientId clientId,
        string? idempotencyKey,
        string? roleTitle,
        CountryCode country,
        DateOnly proposedStartDate,
        Money annualCompensation,
        CountryEmploymentPolicy policy,
        DateOnly today,
        DateTimeOffset now)
    {
        var normalizedRoleTitle = roleTitle?.Trim();
        if (normalizedRoleTitle is null || normalizedRoleTitle.Length is < 2 or > 120)
        {
            throw new BusinessRuleViolationException(
                "workforce_plan.role_title_invalid",
                "Role title must contain between 2 and 120 characters.");
        }

        var normalizedKey = idempotencyKey?.Trim();
        if (normalizedKey is null || normalizedKey.Length is < 8 or > 128)
        {
            throw new BusinessRuleViolationException(
                "idempotency_key.invalid",
                "Idempotency key must contain between 8 and 128 characters.");
        }

        if (proposedStartDate < today)
        {
            throw new BusinessRuleViolationException(
                "workforce_plan.start_date_in_past",
                "Proposed start date cannot be in the past.");
        }

        if (!policy.AppliesOn(proposedStartDate) || policy.Country != country)
        {
            throw new BusinessRuleViolationException(
                "country_policy.not_applicable",
                "The selected employment policy does not apply to the proposed hire.");
        }

        var quote = PlanQuote.Estimate(annualCompensation, policy);
        return new WorkforcePlan(
            id,
            clientId,
            normalizedKey,
            normalizedRoleTitle,
            country,
            proposedStartDate,
            annualCompensation,
            quote,
            now.ToUniversalTime());
    }
}
