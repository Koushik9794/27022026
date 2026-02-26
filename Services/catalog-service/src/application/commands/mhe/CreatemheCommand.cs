namespace CatalogService.Application.commands.Mhe;

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
