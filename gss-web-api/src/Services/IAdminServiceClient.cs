using GssWebApi.Dto;

namespace GssWebApi.Services
{
    public interface IAdminServiceClient
    {
        // Roles
        Task<IEnumerable<RoleResponse>> GetRolesAsync(CancellationToken ct = default);
        Task<RoleResponse?> GetRoleByIdAsync(Guid id, CancellationToken ct = default);
        Task<Guid> CreateRoleAsync(CreateRoleRequest request, CancellationToken ct = default);
        Task<bool> UpdateRoleAsync(Guid id, UpdateRoleRequest request, CancellationToken ct = default);
        Task<bool> DeleteRoleAsync(Guid id, string? modifiedBy, CancellationToken ct = default);
        Task<bool> ActivateRoleAsync(Guid id, bool activate, string? modifiedBy, CancellationToken ct = default);

        // Permissions
        Task<IEnumerable<PermissionResponse>> GetPermissionsAsync(string? module = null, string? entityName = null, CancellationToken ct = default);
        Task<PermissionResponse?> GetPermissionByIdAsync(Guid id, CancellationToken ct = default);
        Task<Guid> CreatePermissionAsync(CreatePermissionRequest request, CancellationToken ct = default);
        Task<bool> UpdatePermissionAsync(Guid id, UpdatePermissionRequest request, CancellationToken ct = default);
        Task<bool> DeletePermissionAsync(Guid id, string? modifiedBy, CancellationToken ct = default);
        Task<bool> ActivatePermissionAsync(Guid id, bool activate, string? modifiedBy, CancellationToken ct = default);

        // Entities
        Task<IEnumerable<EntityResponse>> GetEntitiesAsync(CancellationToken ct = default);
        Task<string> CreateEntityAsync(CreateEntityRequest request, CancellationToken ct = default);

        // Role Permissions
        Task<Guid> AssignPermissionToRoleAsync(AssignPermissionRequest request, CancellationToken ct = default);
        Task<bool> RemovePermissionFromRoleAsync(Guid roleId, Guid permissionId, CancellationToken ct = default);
        Task<IEnumerable<PermissionResponse>> GetPermissionsByRoleAsync(Guid roleId, CancellationToken ct = default);
        Task<IEnumerable<RoleResponse>> GetRolesByPermissionAsync(Guid permissionId, CancellationToken ct = default);

    Task<IEnumerable<UserResponse>> GetUsersAsync(CancellationToken ct = default);
    Task<UserResponse?> GetUserByIdAsync(Guid id, CancellationToken ct = default);
    Task<UserResponse?> GetUserByEmailAsync(string email, CancellationToken ct = default);

    // Dealers
    Task<IEnumerable<DealerDto>> GetDealersAsync(CancellationToken ct = default);
    Task<DealerDto?> GetDealerByIdAsync(Guid id, CancellationToken ct = default);
    Task<Guid> CreateDealerAsync(CreateDealerRequest request, CancellationToken ct = default);
    Task<bool> UpdateDealerAsync(Guid id, UpdateDealerRequest request, CancellationToken ct = default);
    Task<bool> DeleteDealerAsync(Guid id, Guid updatedBy, CancellationToken ct = default);
    Task<bool> ActivateDealerAsync(Guid id, Guid updatedBy, CancellationToken ct = default);
    Task<bool> DeactivateDealerAsync(Guid id, Guid updatedBy, CancellationToken ct = default);
    }
}
