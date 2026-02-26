using CatalogService.Domain.Aggregates;

namespace CatalogService.Infrastructure.Persistence;

public interface ICurrencyRepository
{
    Task<IEnumerable<Currency>> GetAllAsync(bool includeInactive = false);
    Task<Currency?> GetByIdAsync(Guid id);
    Task<Currency?> GetByCodeAsync(string code);
    Task<bool> ExistsByCodeAsync(string code);
    Task CreateAsync(Currency currency);
    Task UpdateAsync(Currency currency);
    Task DeleteAsync(Guid id);
}
