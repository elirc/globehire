namespace GlobeHire.Domain.Common;

public static class MoneyAllocator
{
    public static IReadOnlyList<Money> Allocate(Money total, int parts)
    {
        if (total.Amount < 0 || parts is < 1 or > 10_000)
        {
            throw new BusinessRuleViolationException(
                "money.allocation_invalid",
                "A non-negative amount and between 1 and 10000 parts are required.");
        }

        var totalCents = decimal.ToInt64(total.Amount * 100m);
        var baseCents = totalCents / parts;
        var remainder = totalCents % parts;
        var allocations = new Money[parts];
        for (var index = 0; index < parts; index++)
        {
            var cents = baseCents + (index < remainder ? 1 : 0);
            allocations[index] = Money.Of(cents / 100m, total.Currency);
        }

        return allocations;
    }
}
