using GlobeHire.Application.Abstractions;

namespace GlobeHire.Infrastructure.Identifiers;

public sealed class GuidIdGenerator : IIdGenerator
{
    public Guid NewId() => Guid.NewGuid();
}
