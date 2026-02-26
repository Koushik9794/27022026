namespace CatalogService.Application.Commands.Sku;

public record CreateSkuCommand(
    string Code,
    string Name,
    string? Description,
    Dictionary<string, object>? AttributeSchema,
        IFormFile? GlbFile,
    bool IsActive,
    string? CreatedBy
);
