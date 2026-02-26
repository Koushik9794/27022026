using System.Data;
using Dapper;
using AdminService.Domain.Aggregates;
using AdminService.Domain.ValueObjects;
using AdminService.Infrastructure.Dapper;

namespace AdminService.Infrastructure.Persistence;

public sealed class DapperRolePermissionRepository : IRolePermissionRepository
{
    private readonly IDbConnectionFactory _factory;
    public DapperRolePermissionRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<bool> ExistsAsync(Guid roleId, Guid permissionId, CancellationToken ct)
    {
        const string sql = @"SELECT 1 FROM public.app_role_permission WHERE role_id = @RoleId AND permission_id = @PermissionId LIMIT 1;";
        using var conn = _factory.CreateConnection();
        var exists = await conn.ExecuteScalarAsync<int?>(new CommandDefinition(sql, new { RoleId = roleId, PermissionId = permissionId }, cancellationToken: ct));
        return exists.HasValue;
    }

    public async Task<Guid> AssignAsync(AdminService.Domain.Entities.RolePermission rp, CancellationToken ct)
    {
        const string sql = @"INSERT INTO public.app_role_permission (id, role_id, permission_id, created_by, created_at)
                             VALUES (@Id, @RoleId, @PermissionId, @CreatedBy, @CreatedAt);";
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(sql, new {
            rp.Id, rp.RoleId, rp.PermissionId, rp.CreatedBy, rp.CreatedAt
        }, cancellationToken: ct));
        return rp.Id;
    }

    public async Task RemoveAsync(Guid roleId, Guid permissionId, CancellationToken ct)
    {
        const string sql = @"DELETE FROM public.app_role_permission WHERE role_id = @RoleId AND permission_id = @PermissionId;";
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(sql, new { RoleId = roleId, PermissionId = permissionId }, cancellationToken: ct));
    }

    private sealed class PermissionRow
    {
        public Guid id { get; set; }
        public string permission_name { get; set; } = "";
        public string? description { get; set; }
        public string module_name { get; set; } = "";
        public string? entity_name { get; set; }
        public bool is_active { get; set; }
        public bool is_deleted { get; set; }
        public string created_by { get; set; } = "";
        public DateTime created_at { get; set; }
        public string? modified_by { get; set; }
        public DateTime? modified_at { get; set; }

        public Permission ToDomain() => Permission.Rehydrate(
            id,
            PermissionName.Create(permission_name),
            description,
            ModuleName.Create(module_name),
            entity_name is null ? null : EntityName.Create(entity_name),
            is_active,
            is_deleted,
            created_by,
            created_at,
            modified_by,
            modified_at
        );
    }

    private sealed class RoleRow
    {
        public Guid id { get; set; }
        public string role_name { get; set; } = "";
        public string? description { get; set; }
        public bool is_active { get; set; }
        public bool is_deleted { get; set; }
        public string created_by { get; set; } = "";
        public DateTime created_at { get; set; }
        public string? modified_by { get; set; }
        public DateTime? modified_at { get; set; }

        public Role ToDomain() => Role.Rehydrate(
            id,
            role_name,
            description,
            is_active,
            is_deleted,
            created_by,
            created_at,
            modified_by,
            modified_at
        );
    }

    public async Task<IEnumerable<Permission>> GetPermissionsByRoleAsync(Guid roleId, CancellationToken ct)
    {
        const string sql = @"
SELECT p.id, p.permission_name, p.description, p.module_name, p.entity_name,
       p.is_active, p.is_deleted, p.created_by, p.created_at, p.modified_by, p.modified_at
FROM public.app_permissions p
JOIN public.app_role_permission rp ON p.id = rp.permission_id
WHERE rp.role_id = @RoleId AND p.is_deleted = false";
        using var conn = _factory.CreateConnection();
        var rows = await conn.QueryAsync<PermissionRow>(new CommandDefinition(sql, new { RoleId = roleId }, cancellationToken: ct));
        return rows.Select(r => r.ToDomain());
    }

    public async Task<IEnumerable<Role>> GetRolesByPermissionAsync(Guid permissionId, CancellationToken ct)
    {
        const string sql = @"
SELECT r.id, r.role_name, r.description, r.is_active, r.is_deleted, r.created_by, r.created_at, r.modified_by, r.modified_at
FROM public.app_roles r
JOIN public.app_role_permission rp ON r.id = rp.role_id
WHERE rp.permission_id = @PermissionId AND r.is_deleted = false";
        using var conn = _factory.CreateConnection();
        var rows = await conn.QueryAsync<RoleRow>(new CommandDefinition(sql, new { PermissionId = permissionId }, cancellationToken: ct));
        return rows.Select(r => r.ToDomain());
    }
}
