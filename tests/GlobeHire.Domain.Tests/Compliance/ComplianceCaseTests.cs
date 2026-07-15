using GlobeHire.Domain.Common;
using GlobeHire.Domain.Compliance;

namespace GlobeHire.Domain.Tests.Compliance;

public sealed class ComplianceCaseTests
{
    [Fact]
    public void Submit_MissingEvidence_IsRejected()
    {
        var complianceCase = Case();
        complianceCase.RecordEvidence(
            "identity",
            "doc-1",
            new DateOnly(2027, 1, 1),
            new DateOnly(2026, 7, 15),
            DateTimeOffset.UtcNow);

        var error = Assert.Throws<BusinessRuleViolationException>(
            () => complianceCase.SubmitForReview(new DateOnly(2026, 7, 15)));

        Assert.Equal("compliance.evidence_incomplete", error.Code);
    }

    [Fact]
    public void CompleteEvidence_CanBeReviewedAndApproved()
    {
        var complianceCase = Case();
        foreach (var type in complianceCase.RequiredEvidenceTypes)
        {
            complianceCase.RecordEvidence(
                type,
                $"{type}-doc",
                new DateOnly(2027, 1, 1),
                new DateOnly(2026, 7, 15),
                DateTimeOffset.UtcNow);
        }

        complianceCase.SubmitForReview(new DateOnly(2026, 7, 15));
        complianceCase.Approve();

        Assert.Equal(ComplianceCaseStatus.Approved, complianceCase.Status);
    }

    private static ComplianceCase Case() =>
        new(
            Guid.NewGuid(),
            ClientId.Parse("client-acme"),
            "worker-42",
            CountryCode.Parse("US"),
            ["identity", "work-authorization"]);
}
