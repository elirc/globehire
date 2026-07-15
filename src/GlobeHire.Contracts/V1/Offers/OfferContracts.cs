namespace GlobeHire.Contracts.V1.Offers;

public sealed record CreateOfferRequest(Guid HireCaseId);
public sealed record ApproveOfferRequest(string Role, string ActorId);
public sealed record IssueContractRequest(string TemplateKey);
public sealed record OfferApprovalResponse(string Role, string ActorId, DateTimeOffset ApprovedAt);
public sealed record ContractResponse(Guid Id, int Revision, string TemplateKey, DateTimeOffset IssuedAt);
public sealed record OfferResponse(
    Guid Id, Guid HireCaseId, string Status, int Version,
    IReadOnlyList<OfferApprovalResponse> Approvals, ContractResponse? Contract);
