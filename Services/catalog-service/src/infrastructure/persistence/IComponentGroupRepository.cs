using CatalogService.Domain.Aggregates;

namespace CatalogService.Infrastructure.Persistence;

public interface IComponentGroupRepository
{
    Task<IEnumerable<ComponentGroup>> GetAllAsync(bool includeInactive = false);
    Task<ComponentGroup?> GetByIdAsync(Guid id);
    Task<ComponentGroup?> GetByCodeAsync(string code);
    Task<bool> ExistsByCodeAsync(string code);
    Task CreateAsync(ComponentGroup componentGroup);
    Task UpdateAsync(ComponentGroup componentGroup);
    Task DeleteAsync(Guid id);
}
