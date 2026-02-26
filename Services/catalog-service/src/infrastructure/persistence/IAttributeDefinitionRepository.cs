using CatalogService.Domain.Entities;
using CatalogService.Domain.Enums;

namespace CatalogService.Infrastructure.Persistence;

public interface IAttributeDefinitionRepository
{
    Task<AttributeDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<AttributeDefinition>> GetAllAsync(bool? isActive, CancellationToken cancellationToken);
    Task<Guid> CreateAsync(AttributeDefinition attributeDefinition, CancellationToken cancellationToken);
    Task<Guid> UpdateAsync(AttributeDefinition attributeDefinition, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, string? deletedBy, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string attributeKey, Guid? excludeId, AttributeScreen Screen, CancellationToken cancellationToken);

    Task<List<AttributeDefinition>> GetByScreenAsync(AttributeScreen Screne, CancellationToken cancellationToken);
    Task<AttributeDefinition?> GetByKeyAsync(string attributeKey, CancellationToken cancellationToken);
}
