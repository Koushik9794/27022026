using Microsoft.AspNetCore.Http;

namespace CatalogService.Application.Commands.Sku;

public record UpdateSkuCommand(
    Guid Id,
    string Name,
    string? Description,
    Dictionary<string, object>? AttributeSchema,
    IFormFile? GlbFile,
    bool IsActive,
    string? UpdatedBy
);
