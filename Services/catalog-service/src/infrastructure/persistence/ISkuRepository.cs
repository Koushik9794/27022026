using CatalogService.Domain.Aggregates;

namespace CatalogService.Infrastructure.Persistence;

public interface ISkuRepository
{
    Task<Sku?> GetByIdAsync(Guid id);
    Task<Sku?> GetByCodeAsync(string code);
    Task<List<Sku>> GetAllAsync();
    Task<Guid> CreateAsync(Sku sku);
    Task<bool> UpdateAsync(Sku sku);
    Task<bool> DeleteAsync(Guid id, string? deletedBy);
    Task<bool> ExistsAsync(string code);
}
