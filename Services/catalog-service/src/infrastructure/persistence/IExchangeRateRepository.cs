using CatalogService.Domain.Aggregates;

namespace CatalogService.Infrastructure.Persistence;

public interface IExchangeRateRepository
{
    Task<IEnumerable<ExchangeRate>> GetAllAsync(bool includeInactive = false);
    Task<ExchangeRate?> GetByIdAsync(Guid id);
    Task<ExchangeRate?> GetLatestRateAsync(string baseCurrency, string quoteCurrency);
    Task<bool> ExistsOverlappingAsync(string baseCurrency, string quoteCurrency, DateTime validFrom, DateTime? validEnd, Guid? excludeId = null);
    Task CreateAsync(ExchangeRate exchangeRate);
    Task UpdateAsync(ExchangeRate exchangeRate);
    Task DeleteAsync(Guid id);
}
