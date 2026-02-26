using CatalogService.Domain.Aggregates;

namespace CatalogService.Infrastructure.Persistence;

public interface IComponentNameRepository
{
    Task<IEnumerable<ComponentName>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<ComponentName>> GetByTypeIdAsync(Guid componentTypeId, bool includeInactive = false);
    Task<ComponentName?> GetByIdAsync(Guid id);
    Task<ComponentName?> GetByCodeAsync(string code);
    Task<bool> ExistsByCodeAsync(string code);
    Task CreateAsync(ComponentName componentName);
    Task UpdateAsync(ComponentName componentName);
    Task DeleteAsync(Guid id);
}
