

namespace CatalogService.Application.Commands.Sku;

public record DeleteSkuCommand(
    Guid Id,
    string? DeletedBy
);
