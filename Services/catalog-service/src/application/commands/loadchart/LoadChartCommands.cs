namespace CatalogService.Application.Commands;

public record CreateLoadChartCommand(
    Guid ProductGroupId,
    string ChartType,
    string ComponentCode,
    Guid ComponentTypeId,
    string? Attributes, // JSON string
    Guid? CreatedBy
);

public record UpdateLoadChartCommand(
    Guid Id,
    Guid ProductGroupId,
    string ChartType,
    string ComponentCode,
    Guid ComponentTypeId,
    string? Attributes, // JSON string
    bool IsActive,
    Guid? UpdatedBy
);

public record DeleteLoadChartCommand(
    Guid Id,
    Guid? DeletedBy
);
