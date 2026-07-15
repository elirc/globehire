namespace GlobeHire.Application.Abstractions;

public interface IClock
{
    DateTimeOffset UtcNow { get; }

    DateOnly Today { get; }
}
