namespace GlobeHire.Domain.Common;

public readonly record struct ClientId
{
    private ClientId(string value) => Value = value;

    public string Value { get; }

    public static ClientId Parse(string? value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 64)
        {
            throw new BusinessRuleViolationException(
                "client_id.invalid",
                "Client ID must contain between 1 and 64 characters.");
        }

        return new ClientId(value.Trim());
    }

    public override string ToString() => Value;
}
