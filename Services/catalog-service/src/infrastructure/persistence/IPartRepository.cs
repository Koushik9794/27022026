using CatalogService.Domain.Aggregates;

namespace CatalogService.Infrastructure.Persistence;

public interface IPartRepository
{
    Task<IEnumerable<Part>> GetAllAsync(
        string? countryCode = null,
        Guid? componentGroupId = null,
        Guid? componentTypeId = null,
        bool? isActive = true,
        bool includeDeleted = false,
        int page = 1,
        int pageSize = 50);
        
    Task<Part?> GetByIdAsync(Guid id);
    Task<Part?> GetByCodeAndCountryAsync(string partCode, string countryCode);
    Task<bool> ExistsByCodeAndCountryAsync(string partCode, string countryCode);
    
    Task CreateAsync(Part part);
    Task UpdateAsync(Part part);
}
