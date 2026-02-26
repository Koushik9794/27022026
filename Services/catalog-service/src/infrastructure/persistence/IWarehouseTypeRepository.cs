using CatalogService.Domain.Entities;

namespace CatalogService.Infrastructure.Persistence;

public interface IWarehouseTypeRepository
{
    Task<WarehouseType?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string name, Guid? excludeId, CancellationToken cancellationToken);
    Task<Guid> CreateAsync(WarehouseType warehouseType, CancellationToken cancellationToken);
    Task<bool> UpdateAsync(WarehouseType warehouseType, CancellationToken cancellationToken);
    Task<bool> Delete(Guid id, CancellationToken cancellationToken);
    Task<List<WarehouseType>> GetAllAsync(bool IncludeInactive , CancellationToken cancellationToken);
}
