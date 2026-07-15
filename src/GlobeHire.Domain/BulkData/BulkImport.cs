using GlobeHire.Domain.Common;

namespace GlobeHire.Domain.BulkData;

public sealed record WorkforceImportRow(
    int RowNumber,
    string ExternalKey,
    string CountryCode,
    decimal AnnualCompensation,
    string Currency);

public sealed record ImportRowOutcome(
    int RowNumber,
    string ExternalKey,
    bool IsValid,
    string? ErrorCode);

public sealed record BulkImportResult(
    int TotalRows,
    int ValidRows,
    int InvalidRows,
    IReadOnlyList<ImportRowOutcome> Outcomes);

public static class BulkImportValidator
{
    public static BulkImportResult Validate(IReadOnlyList<WorkforceImportRow> rows)
    {
        ArgumentNullException.ThrowIfNull(rows);
        if (rows.Count is < 1 or > 1_000)
        {
            throw new BusinessRuleViolationException(
                "bulk_import.size_invalid",
                "Bulk imports must contain between 1 and 1000 rows.");
        }

        var seenKeys = new HashSet<string>(StringComparer.Ordinal);
        var outcomes = new List<ImportRowOutcome>(rows.Count);
        foreach (var row in rows)
        {
            outcomes.Add(ValidateRow(row, seenKeys));
        }

        var validRows = outcomes.Count(outcome => outcome.IsValid);
        return new BulkImportResult(rows.Count, validRows, rows.Count - validRows, outcomes);
    }

    private static ImportRowOutcome ValidateRow(WorkforceImportRow row, HashSet<string> seenKeys)
    {
        if (row.RowNumber < 1 || string.IsNullOrWhiteSpace(row.ExternalKey))
        {
            return Invalid(row, "bulk_import.row_identity_invalid");
        }

        if (!seenKeys.Add(row.ExternalKey.Trim()))
        {
            return Invalid(row, "bulk_import.external_key_duplicate");
        }

        try
        {
            _ = CountryCode.Parse(row.CountryCode);
            var compensation = Money.Of(row.AnnualCompensation, row.Currency);
            if (compensation.Amount <= 0)
            {
                return Invalid(row, "compensation.must_be_positive");
            }
        }
        catch (BusinessRuleViolationException exception)
        {
            return Invalid(row, exception.Code);
        }

        return new ImportRowOutcome(row.RowNumber, row.ExternalKey, true, null);
    }

    private static ImportRowOutcome Invalid(WorkforceImportRow row, string code) =>
        new(row.RowNumber, row.ExternalKey, false, code);
}

public sealed record ReportPageRequest
{
    private ReportPageRequest(int pageSize, string? cursor)
    {
        PageSize = pageSize;
        Cursor = cursor;
    }

    public int PageSize { get; }
    public string? Cursor { get; }

    public static ReportPageRequest Create(int pageSize, string? cursor)
    {
        if (pageSize is < 1 or > 200)
        {
            throw new BusinessRuleViolationException(
                "report.page_size_invalid",
                "Report page size must be between 1 and 200.");
        }

        return new ReportPageRequest(pageSize, string.IsNullOrWhiteSpace(cursor) ? null : cursor.Trim());
    }
}
