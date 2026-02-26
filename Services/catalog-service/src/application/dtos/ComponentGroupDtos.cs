using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Dtos;

public record ComponentGroupDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    int SortOrder,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateComponentGroupRequest(
    [Required] [MaxLength(50)] string Code,
    [Required] [MaxLength(255)] string Name,
    [MaxLength(1000)] string? Description,
    int SortOrder = 0
);

public record UpdateComponentGroupRequest(
    [Required] [MaxLength(255)] string Name,
    [MaxLength(1000)] string? Description,
    int SortOrder
);
