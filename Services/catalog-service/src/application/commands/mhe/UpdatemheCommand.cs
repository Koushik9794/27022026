namespace CatalogService.Application.commands.Mhe;

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
