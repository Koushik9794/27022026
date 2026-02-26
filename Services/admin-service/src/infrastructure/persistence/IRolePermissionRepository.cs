using AdminService.Domain.Aggregates;
using AdminService.Domain.Entities;
namespace AdminService.Infrastructure.Persistence;

public interface IRolePermissionRepository
{
    Task<bool> ExistsAsync(Guid roleId, Guid permissionId, CancellationToken ct);
    Task<Guid> AssignAsync(RolePermission rp, CancellationToken ct);
    Task RemoveAsync(Guid roleId, Guid permissionId, CancellationToken ct);
    Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(Guid roleId, CancellationToken ct);
    Task<IEnumerable<Role>> GetRolesByPermissionAsync(Guid permissionId, CancellationToken ct);
}
