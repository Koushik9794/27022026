using CatalogService.Application.dtos;

namespace CatalogService.Application.Validation.Common;


public interface IAttributeDefinitionProvider
{
    Task<IReadOnlyList<AttributeDefinitionDto>> GetByScreenKeyAsync(
        string screenKey,
        CancellationToken ct);
}

