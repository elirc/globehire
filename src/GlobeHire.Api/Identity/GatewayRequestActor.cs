using GlobeHire.Application.Abstractions;

namespace GlobeHire.Api.Identity;

/// <summary>
/// Reads the actor that the API gateway stamps on every authenticated request.
/// </summary>
internal sealed class GatewayRequestActor(IHttpContextAccessor accessor) : IRequestActor
{
    public const string HeaderName = "X-GlobeHire-Actor";

    private string? _actorId;

    // Parsed once per request: approvals read it for validation and again for the audit stamp.
    public string? ActorId => _actorId ??= Read();

    private string? Read()
    {
        var value = accessor.HttpContext?.Request.Headers[HeaderName].ToString();
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }
}
