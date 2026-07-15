using GlobeHire.Domain.BulkData;
using GlobeHire.Domain.Common;

namespace GlobeHire.Domain.Tests.BulkData;

public sealed class BulkImportTests
{
    [Fact]
    public void Validate_MixedRows_ReturnsRowLevelOutcomesWithoutHidingFailures()
    {
        var result = BulkImportValidator.Validate(
        [
            new(1, "worker-1", "US", 120_000, "USD"),
            new(2, "worker-2", "USA", 90_000, "USD"),
            new(3, "worker-1", "GB", 80_000, "GBP"),
        ]);

        Assert.Equal(3, result.TotalRows);
        Assert.Equal(1, result.ValidRows);
        Assert.Equal("country_code.invalid", result.Outcomes[1].ErrorCode);
        Assert.Equal("bulk_import.external_key_duplicate", result.Outcomes[2].ErrorCode);
    }

    [Fact]
    public void ReportPage_ExcessiveSize_IsRejected()
    {
        var error = Assert.Throws<BusinessRuleViolationException>(() => ReportPageRequest.Create(201, null));

        Assert.Equal("report.page_size_invalid", error.Code);
    }
}
