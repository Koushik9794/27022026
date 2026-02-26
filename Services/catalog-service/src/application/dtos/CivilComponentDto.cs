using System.Text.Json;

namespace CatalogService.Application.dtos;

public sealed record CivilComponentDto
(
    Guid Id,
    string Code,
    string Name,
    string Label,
    string Icon,
    string? Tooltip,
    string Category,
    string? DefaultElement,
    bool IsActive,
    bool IsDeleted,
    DateTimeOffset CreatedAt,
    string? CreatedBy,
    DateTimeOffset? UpdatedAt,
    string? UpdatedBy
);
