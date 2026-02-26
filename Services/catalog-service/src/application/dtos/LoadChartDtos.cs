using System.ComponentModel.DataAnnotations;
using System.Text.Json;

namespace CatalogService.Application.Dtos;

/// <summary>
/// Data transfer object for LoadChart details.
/// </summary>
public record LoadChartDto(
    Guid Id,
    Guid ProductGroupId,
    string? ProductGroupName,
    string ChartType,
    string ComponentCode,
    Guid ComponentTypeId,
    string? ComponentName,
    Dictionary<string, JsonElement> Attributes,
    bool IsActive,
    bool IsDelete,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    Guid? CreatedBy,
    Guid? UpdatedBy
);

/// <summary>
/// Data transfer object for creating a new LoadChart.
/// </summary>
public record CreateLoadChartRequest(
    [Required] Guid ProductGroupId,
    [Required] string ChartType,
    [Required] string ComponentCode,
    [Required] Guid ComponentTypeId,
    string? Attributes, // JSON string
    Guid? CreatedBy
);

/// <summary>
/// Data transfer object for updating an existing LoadChart.
/// </summary>
public record UpdateLoadChartRequest(
    [Required] Guid ProductGroupId,
    [Required] string ChartType,
    [Required] string ComponentCode,
    [Required] Guid ComponentTypeId,
    string? Attributes, // JSON string
    bool IsActive,
    Guid? UpdatedBy
);
/// <summary>
/// Data transfer object for importing LoadCharts from Excel.
/// </summary>
public record ImportLoadChartExcelRequest(
    [Required] Microsoft.AspNetCore.Http.IFormFile File,
    [Required] Guid ProductGroupId,
    [Required] string ChartType,
    Guid? CreatedBy
);
