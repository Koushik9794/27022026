using CatalogService.Domain.Entities;


namespace CatalogService.Infrastructure.Persistence;

public interface IAttributeRepository
{
    Task<Guid> AddAsync(AttributeDefinition entity, CancellationToken ct);
    Task<bool> UpdateAsync(AttributeDefinition entity, CancellationToken ct);

    Task<AttributeDefinition?> GetByIdAsync(Guid id, CancellationToken ct);
    Task<AttributeDefinition?> GetByKeyAsync(string attributeKey, CancellationToken ct);

    Task<bool> AttributeKeyExistsAsync(Guid? id, string attributeKey, CancellationToken ct);

    Task<IEnumerable<AttributeDefinition>> GetListAsync(bool? isActive, CancellationToken ct);

    //Task<(IReadOnlyList<AttributeDefinition> Items, int Total)> SearchAsync(
    //    string? term, bool? isActive, int skip, int take, CancellationToken ct);



    Task<bool> DeleteAsync(Guid id, string deletedBy, CancellationToken ct);
}
