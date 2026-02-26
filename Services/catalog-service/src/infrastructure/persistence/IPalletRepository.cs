using CatalogService.Domain.Aggregates;

namespace CatalogService.Infrastructure.Persistence;

/// <summary>
/// Repository interface for Pallet aggregate.
/// </summary>
public interface IPalletRepository
{
    Task<IEnumerable<Pallet>> GetAllAsync(bool includeInactive = false);
    Task<Pallet?> GetByIdAsync(Guid id);
    Task<Pallet?> GetByCodeAsync(string code);
    Task<Guid> CreateAsync(Pallet pallet);
    Task<bool> UpdateAsync(Pallet pallet);
    Task<bool> DeleteAsync(Guid id, string? deletedBy);
    Task<bool> ExistsAsync(string code);
}
