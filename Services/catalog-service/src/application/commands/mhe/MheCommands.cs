using Microsoft.AspNetCore.Http;

namespace CatalogService.Application.Commands.Mhe;

public record CreateMheCommand(
    string Code,
    string Name,
    string Manufacturer,
    string Brand,
    string Model,
    string MheType,
    string MheCategory,
    IFormFile? GlbFile,
    string? Attributes,
    bool IsActive,
    string? CreatedBy
);

public record UpdateMheCommand(
    Guid Id,
    string Code,
    string Name,
    string Manufacturer,
    string Brand,
    string Model,
    string MheType,
    string MheCategory,
    IFormFile? GlbFile,
    string? Attributes,
    bool IsActive,
    string? UpdatedBy
);
