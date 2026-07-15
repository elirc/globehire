namespace GlobeHire.Domain.Common;

public readonly record struct CountryCode
{
    private CountryCode(string value) => Value = value;

    public string Value { get; }

    public static CountryCode Parse(string? value)
    {
        var normalized = value?.Trim().ToUpperInvariant();
        if (normalized is null || normalized.Length != 2 || !normalized.All(char.IsAsciiLetterUpper))
        {
            throw new BusinessRuleViolationException(
                "country_code.invalid",
                "Country code must be a two-letter ISO-style code.");
        }

        return new CountryCode(normalized);
    }

    public override string ToString() => Value;
}
