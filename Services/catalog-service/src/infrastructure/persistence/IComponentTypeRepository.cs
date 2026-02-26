using CatalogService.Domain.Aggregates;

namespace CatalogService.Infrastructure.Persistence;

public interface IComponentTypeRepository
{
    Task<ComponentType?> GetByIdAsync(Guid id);
    Task<ComponentType?> GetByCodeAsync(string code);
    Task<List<ComponentType>> GetAllAsync(string? componentGroupCode = null, Guid? componentGroupId = null, bool includeInactive = false);
    Task<List<ComponentType>> GetByGroupIdAsync(Guid componentGroupId);
    Task<Guid> CreateAsync(ComponentType componentType);
    Task<bool> UpdateAsync(ComponentType componentType);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(string code);
}
