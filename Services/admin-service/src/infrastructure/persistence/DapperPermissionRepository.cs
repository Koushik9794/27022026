using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

using AdminService.Domain.Aggregates;
using AdminService.Domain.ValueObjects;
using AdminService.Infrastructure.Dapper;

namespace AdminService.Infrastructure.Persistence
{
    public sealed class DapperPermissionRepository : IPermissionRepository
    {
        private readonly IDbConnectionFactory _factory;
        public DapperPermissionRepository(IDbConnectionFactory factory) => _factory = factory;

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

        public async Task<Guid> CreateAsync(Permission p, CancellationToken ct)
        {
            const string sql = @"
INSERT INTO public.app_permissions
(id, permission_name, description, module_name, is_active, is_deleted, created_by, created_at, modified_by, modified_at, entity_name)
VALUES (@Id, @PermissionName, @Description, @ModuleName, @IsActive, @IsDeleted, @CreatedBy, @CreatedAt, @ModifiedBy, @ModifiedAt, @EntityName);";

            using var conn = _factory.CreateConnection();
            await conn.ExecuteAsync(new CommandDefinition(sql, new
            {
                Id = p.Id,
                PermissionName = p.PermissionName.Value,
                p.Description,
                ModuleName = p.ModuleName.Value,
                p.IsActive,
                p.IsDeleted,
                p.CreatedBy,
                p.CreatedAt,
                p.ModifiedBy,
                p.ModifiedAt,
                EntityName = p.EntityName?.Value
            }, cancellationToken: ct));

            return p.Id;
        }

        public async Task<Permission?> GetByIdAsync(Guid id, CancellationToken ct)
        {
            const string sql = @"
SELECT id, permission_name, description, module_name, is_active, is_deleted, created_by, created_at, modified_by, modified_at, entity_name
FROM public.app_permissions
WHERE id = @Id AND is_deleted = false;";

            using var conn = _factory.CreateConnection();
            var row = await conn.QuerySingleOrDefaultAsync<PermissionRow>(
                new CommandDefinition(sql, new { Id = id }, cancellationToken: ct));
            return row?.ToDomain();
        }

        public async Task<IEnumerable<Permission>> GetAllAsync(string? module, string? entityName, CancellationToken ct)
        {
            const string sql = @"
SELECT id, permission_name, description, module_name, is_active, is_deleted, created_by, created_at, modified_by, modified_at, entity_name
FROM public.app_permissions
WHERE is_deleted = false
  AND (@Module IS NULL OR module_name = @Module)
  AND (@EntityName IS NULL OR entity_name = @EntityName)
ORDER BY created_at DESC;";

            using var conn = _factory.CreateConnection();
            var rows = await conn.QueryAsync<PermissionRow>(
                new CommandDefinition(sql, new { Module = module, EntityName = entityName }, cancellationToken: ct));
            return rows.Select(r => r.ToDomain());
        }

        public async Task UpdateAsync(Permission p, CancellationToken ct)
        {
            const string sql = @"
UPDATE public.app_permissions
SET description = @Description, module_name = @ModuleName, entity_name = @EntityName,
    is_active = @IsActive, is_deleted = @IsDeleted, modified_by = @ModifiedBy, modified_at = @ModifiedAt
WHERE id = @Id;";

            using var conn = _factory.CreateConnection();
            await conn.ExecuteAsync(new CommandDefinition(sql, new
            {
                p.Id,
                p.Description,
                ModuleName = p.ModuleName.Value,
                EntityName = p.EntityName?.Value,
                p.IsActive,
                p.IsDeleted,
                p.ModifiedBy,
                p.ModifiedAt
            }, cancellationToken: ct));
        }

        public async Task SoftDeleteAsync(Guid id, string modifiedBy, DateTime nowUtc, CancellationToken ct)
        {
            const string sql = @"
UPDATE public.app_permissions
SET is_deleted = true, is_active = false, modified_by = @ModifiedBy, modified_at = @ModifiedAt
WHERE id = @Id;";

            using var conn = _factory.CreateConnection();
            await conn.ExecuteAsync(new CommandDefinition(sql, new
            {
                Id = id,
                ModifiedBy = modifiedBy,
                ModifiedAt = nowUtc
            }, cancellationToken: ct));
        }
    }
}
