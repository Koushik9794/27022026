using AdminService.Domain.Aggregates;

namespace AdminService.Domain.Services
{
    /// <summary>
    /// Repository interface for role persistence.
    /// </summary>
    public interface IRoleRepository
    {
        Task<Role?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<Role?> GetByNameAsync(string roleName, CancellationToken ct, bool includeDeleted = false);
        Task<Guid> CreateAsync(Role role, CancellationToken ct);
        Task UpdateAsync(Role role, CancellationToken ct);
        Task SoftDeleteAsync(Guid id, string? modifiedBy, DateTime nowUtc, CancellationToken ct);
        Task<(IReadOnlyList<Role> Items, long Total)> ListAsync(
            string? search, bool? isActive, int page, int pageSize, CancellationToken ct);
    }
}
