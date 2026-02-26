using System.Collections.Generic;

namespace RuleService.Application.Messages;

/// <summary>
/// Command to import a load chart from CSV data
/// </summary>
public record ImportLoadChartCommand(
    string MatrixName,
    string Category,
    List<string[]> CsvRows
);

/// <summary>
/// Result of import operation
/// </summary>
public record ImportLoadChartResult(
    bool Success,
    string MatrixName,
    int RowsProcessed,
    string GeneratedJsonb,
    string? ErrorMessage = null
);

/// <summary>
/// Command to preview import without saving
/// </summary>
public record PreviewImportCommand(
    List<string[]> CsvRows
);

/// <summary>
/// Result of preview operation
/// </summary>
public record PreviewImportResult(
    bool Success,
    int RowCount,
    string[] Headers,
    List<string[]> SampleData,
    string GeneratedJsonb,
    string? ErrorMessage = null
);
