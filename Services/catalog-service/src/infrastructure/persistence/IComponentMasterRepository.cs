using CatalogService.Domain.Aggregates;

namespace CatalogService.Infrastructure.Persistence;

public interface IComponentMasterRepository
{
    Task<IEnumerable<ComponentMaster>> GetAllAsync(
        string? countryCode = null,
        Guid? componentGroupId = null,
        Guid? componentTypeId = null,
        bool? isActive = true,
        bool includeDeleted = false,
        int page = 1,
        int pageSize = 50);

    Task<ComponentMaster?> GetByIdAsync(Guid id);
    Task<ComponentMaster?> GetByCodeAndCountryAsync(string componentMasterCode, string countryCode);
    Task<bool> ExistsByCodeAndCountryAsync(string componentMasterCode, string countryCode);
    Task CreateAsync(ComponentMaster componentMaster);
    Task UpdateAsync(ComponentMaster componentMaster);
}
