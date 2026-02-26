using CatalogService.Domain.Aggregates;

namespace CatalogService.Infrastructure.Persistence;

public interface IProductGroupRepository
{
    Task<ProductGroup?> GetByIdAsync(Guid id);
    Task<ProductGroup?> GetByCodeAsync(string code);
    Task<List<ProductGroup>> GetAllAsync(bool includeInactive = false);
    Task<List<ProductGroup>> GetVariantsAsync(Guid parentGroupId);
    Task<Guid> CreateAsync(ProductGroup productGroup);
    Task<bool> UpdateAsync(ProductGroup productGroup);
    Task<bool> DeleteAsync(Guid id);
    Task<bool> ExistsAsync(string code);
}
