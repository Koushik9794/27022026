using System.Text.Json;

namespace CatalogService.Application.Dtos;

public record SkuDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    Dictionary<string, object>? AttributeSchema,
    string? GlbFilePath,
bool IsActive,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy
);
