namespace GlobeHire.Contracts.V1.Employments;

public sealed record ActivateEmploymentRequest(Guid OfferId, string WorkerReference, DateOnly StartDate);
public sealed record EmploymentResponse(
    Guid Id, Guid OfferId, Guid ContractId, string WorkerReference, DateOnly StartDate,
    string Status, int Version, DateTimeOffset ActivatedAt);
