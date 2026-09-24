namespace GlobeHire.Application.Abstractions;

/// <summary>
/// The person the API gateway authenticated for the current request.
/// </summary>
public interface IRequestActor
{
    string? ActorId { get; }
}
