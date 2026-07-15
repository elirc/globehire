namespace GlobeHire.Domain.Common;

public sealed class BusinessRuleViolationException : Exception
{
    public BusinessRuleViolationException(string code, string message)
        : base(message)
    {
        Code = code;
    }

    public string Code { get; }
}
