using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Dtos;

public record ComponentNameDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    Guid ComponentTypeId,
    string? ComponentTypeCode,
    string? ComponentTypeName,
    bool IsActive,
    DateTime CreatedAt,
    DateTime? UpdatedAt
);

public record CreateComponentNameRequest(
    [Required] [MaxLength(50)] string Code,
    [Required] [MaxLength(255)] string Name,
    [MaxLength(1000)] string? Description,
    [Required] Guid ComponentTypeId
);

public record UpdateComponentNameRequest(
    [Required] [MaxLength(255)] string Name,
    [MaxLength(1000)] string? Description,
    [Required] Guid ComponentTypeId
);
