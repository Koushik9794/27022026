using System.Data;
using Dapper;
using AdminService.Domain.Aggregates;
using AdminService.Domain.ValueObjects;
using AdminService.Infrastructure.Dapper;

namespace AdminService.Infrastructure.Persistence;

public sealed class DapperRoleRepository : AdminService.Domain.Services.IRoleRepository
{
    private readonly IDbConnectionFactory _factory;
    public DapperRoleRepository(IDbConnectionFactory factory) => _factory = factory;

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
        public int permission_count { get; set; }

        public Role ToDomain() => Role.Rehydrate(
            id,
            role_name,
            description,
            is_active,
            is_deleted,
            created_by,
            created_at,
            modified_by,
            modified_at,
            permission_count
        );
    }

    public async Task<Guid> CreateAsync(Role role, CancellationToken ct)
    {
        const string sql = @"
INSERT INTO public.app_roles (id, role_name, description, is_active, is_deleted, created_by, created_at, modified_by, modified_at)
VALUES (@Id, @RoleName, @Description, @IsActive, @IsDeleted, @CreatedBy, @CreatedAt, @ModifiedBy, @ModifiedAt);";

        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(sql, new {
            Id = role.Id,
            RoleName = role.RoleName,
            role.Description,
            role.IsActive,
            role.IsDeleted,
            role.CreatedBy,
            role.CreatedAt,
            role.ModifiedBy,
            role.ModifiedAt
        }, cancellationToken: ct));
        return role.Id;
    }

    public async Task<Role?> GetByIdAsync(Guid id, CancellationToken ct)
    {
        const string sql = @"SELECT r.id, r.role_name, r.description, r.is_active, r.is_deleted, r.created_by, r.created_at, r.modified_by, r.modified_at,
                                     COUNT(rp.id) as permission_count
                             FROM public.app_roles r
                             LEFT JOIN public.app_role_permission rp ON r.id = rp.role_id
                             WHERE r.id = @Id AND r.is_deleted = false
                             GROUP BY r.id, r.role_name, r.description, r.is_active, r.is_deleted, r.created_by, r.created_at, r.modified_by, r.modified_at";
        using var conn = _factory.CreateConnection();
        var row = await conn.QuerySingleOrDefaultAsync<RoleRow>(new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
        return row?.ToDomain();
    }

    public async Task<Role?> GetByNameAsync(string roleName, CancellationToken ct, bool includeDeleted = false)
    {
        var sql = @"SELECT id, role_name, description, is_active, is_deleted, created_by, created_at, modified_by, modified_at
                    FROM public.app_roles
                    WHERE LOWER(role_name) = LOWER(@RoleName)";
        
        if (!includeDeleted) sql += " AND is_deleted = false";
        
        using var conn = _factory.CreateConnection();
        var row = await conn.QuerySingleOrDefaultAsync<RoleRow>(new CommandDefinition(sql, new { RoleName = roleName }, cancellationToken: ct));
        return row?.ToDomain();
    }

    public async Task UpdateAsync(Role role, CancellationToken ct)
    {
        const string sql = @"UPDATE public.app_roles
SET role_name = @RoleName, description = @Description, is_active = @IsActive, is_deleted = @IsDeleted,
    modified_by = @ModifiedBy, modified_at = @ModifiedAt
WHERE id = @Id;";
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(sql, new {
            role.RoleName, role.Id, role.Description, role.IsActive, role.IsDeleted, role.ModifiedBy, role.ModifiedAt
        }, cancellationToken: ct));
    }

    public async Task SoftDeleteAsync(Guid id, string? modifiedBy, DateTime nowUtc, CancellationToken ct)
    {
        const string sql = @"UPDATE public.app_roles
SET is_deleted = true, is_active = false, modified_by = @ModifiedBy, modified_at = @ModifiedAt
WHERE id = @Id;";
        using var conn = _factory.CreateConnection();
        await conn.ExecuteAsync(new CommandDefinition(sql, new { Id = id, ModifiedBy = modifiedBy, ModifiedAt = nowUtc }, cancellationToken: ct));
    }

    public async Task<(IReadOnlyList<Role> Items, long Total)> ListAsync(string? search, bool? isActive, int page, int pageSize, CancellationToken ct)
    {
        var where = "is_deleted = false";
        var pars = new DynamicParameters();
        
        if (!string.IsNullOrWhiteSpace(search))
        {
            where += " AND (LOWER(role_name) LIKE @Search OR LOWER(description) LIKE @Search)";
            pars.Add("Search", $"%{search.ToLower()}%");
        }
        
        if (isActive.HasValue)
        {
            where += " AND is_active = @IsActive";
            pars.Add("IsActive", isActive.Value);
        }
        
        var countSql = $"SELECT COUNT(*) FROM public.app_roles WHERE {where}";
        var aliasedWhere = where
            .Replace("role_name", "r.role_name")
            .Replace("description", "r.description")
            .Replace("is_active", "r.is_active")
            .Replace("is_deleted", "r.is_deleted");

        var dataSql = $@"SELECT r.id, r.role_name, r.description, r.is_active, r.is_deleted, r.created_by, r.created_at, r.modified_by, r.modified_at,
                                COUNT(rp.id) as permission_count
                         FROM public.app_roles r
                         LEFT JOIN public.app_role_permission rp ON r.id = rp.role_id
                         WHERE {aliasedWhere}
                         GROUP BY r.id, r.role_name, r.description, r.is_active, r.is_deleted, r.created_by, r.created_at, r.modified_by, r.modified_at
                         ORDER BY r.created_at DESC
                         OFFSET @Offset LIMIT @Limit";
        
        pars.Add("Offset", (page - 1) * pageSize);
        pars.Add("Limit", pageSize);
        
        using var conn = _factory.CreateConnection();
        var total = await conn.ExecuteScalarAsync<long>(new CommandDefinition(countSql, pars, cancellationToken: ct));
        var rows = await conn.QueryAsync<RoleRow>(new CommandDefinition(dataSql, pars, cancellationToken: ct));
        
        return (rows.Select(r => r.ToDomain()).ToList(), total);
    }
}
