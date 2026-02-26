using CatalogService.Domain.Entities;

namespace CatalogService.Infrastructure.Persistence;

public interface ICivilComponentRepository
{
    Task<CivilComponent?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<CivilComponent?> GetByCodeAsync(string code, CancellationToken cancellationToken);
    Task<bool> ExistsAsync(string code, Guid? excludeId, CancellationToken cancellationToken);
    Task CreateAsync(CivilComponent civilComponent, CancellationToken cancellationToken);
    Task UpdateAsync(CivilComponent civilComponent, CancellationToken cancellationToken);
    Task DeleteAsync(Guid id, CancellationToken cancellationToken);
    Task<List<CivilComponent>> GetAllAsync(bool? IncludeInactive,CancellationToken cancellationToken);
}
