using CatalogService.Domain.Aggregates;

namespace CatalogService.Infrastructure.Persistence;

public interface IMheRepository
{
    Task<Mhe?> GetByIdAsync(Guid id, CancellationToken cancellationToken);
    Task<List<Mhe>> GetAllAsync(bool? IsActive, CancellationToken cancellationToken);
    Task<Guid> CreateAsync(Mhe mhe, CancellationToken cancellationToken);
    Task<Guid> UpdateAsync(Mhe mhe, CancellationToken cancellationToken);
    Task<bool> DeleteAsync(Guid id, string? deletedBy, CancellationToken cancellationToken);
    Task<bool> CodeExistsAsync(Guid? id, string Code, CancellationToken ct);
}
