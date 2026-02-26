using CatalogService.Domain.Aggregates;

namespace CatalogService.Infrastructure.Persistence;

public interface ICountryRepository
{
    Task<IEnumerable<Country>> GetAllAsync(bool includeInactive = false);
    Task<Country?> GetByIdAsync(Guid id);
    Task<Country?> GetByCodeAsync(string code);
    Task<bool> ExistsByCodeAsync(string code);
    Task CreateAsync(Country country);
    Task UpdateAsync(Country country);
    Task DeleteAsync(Guid id);
}
