namespace GssCommon.Export;

public enum ExportFormat
{
    Excel,
    Pdf,
    Csv
}

public record ExportOptions(
    ExportFormat Format,
    string? FileName = null,
    bool IncludeHeaders = true
);

public record ExportResult(
    byte[] Data,
    string ContentType,
    string FileName
);

public interface IExcelExporter
{
    Task<ExportResult> ExportAsync<T>(IEnumerable<T> data, ExportOptions options, CancellationToken cancellationToken = default);
}

public interface IPdfExporter
{
    Task<ExportResult> ExportAsync<T>(IEnumerable<T> data, ExportOptions options, CancellationToken cancellationToken = default);
}
