namespace GlobeHire.Domain.Common;

public readonly record struct Money
{
    private Money(decimal amount, string currency)
    {
        Amount = amount;
        Currency = currency;
    }

    public decimal Amount { get; }

    public string Currency { get; }

    public static Money Of(decimal amount, string? currency)
    {
        var normalizedCurrency = currency?.Trim().ToUpperInvariant();
        if (normalizedCurrency is null ||
            normalizedCurrency.Length != 3 ||
            !normalizedCurrency.All(char.IsAsciiLetterUpper))
        {
            throw new BusinessRuleViolationException(
                "money.currency_invalid",
                "Currency must be a three-letter ISO-style code.");
        }

        return new Money(Round(amount), normalizedCurrency);
    }

    public Money Add(Money other)
    {
        EnsureSameCurrency(other);
        return Of(Amount + other.Amount, Currency);
    }

    public Money Multiply(decimal multiplier) => Of(Amount * multiplier, Currency);

    public Money Divide(decimal divisor)
    {
        if (divisor == 0)
        {
            throw new BusinessRuleViolationException("money.divide_by_zero", "Money cannot be divided by zero.");
        }

        return Of(Amount / divisor, Currency);
    }

    private static decimal Round(decimal value) => decimal.Round(value, 2, MidpointRounding.AwayFromZero);

    private void EnsureSameCurrency(Money other)
    {
        if (!string.Equals(Currency, other.Currency, StringComparison.Ordinal))
        {
            throw new BusinessRuleViolationException(
                "money.currency_mismatch",
                $"Cannot combine {Currency} and {other.Currency} amounts.");
        }
    }
}
