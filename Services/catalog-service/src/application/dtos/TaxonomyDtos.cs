using System.Text.Json;

namespace CatalogService.Application.Dtos;

public record ComponentTypeDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    Guid ComponentGroupId,
    string? ComponentGroupCode,
    string? ComponentGroupName,
    Guid? ParentTypeId,
    string? ParentTypeCode,
    object? AttributeSchema,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record ProductGroupDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    Guid? ParentGroupId,
    string? ParentGroupCode,
    bool IsVariant,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);
public sealed record WarehouseTypeDto
(
    Guid Id,
    string Name,
    string Label,
    string Icon,
    string? Tooltip,
    string? templatePath_Civil,
    string? templatePath_Json,
    string? Attributes,
    bool IsActive,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    string? CreatedBy,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy
);
