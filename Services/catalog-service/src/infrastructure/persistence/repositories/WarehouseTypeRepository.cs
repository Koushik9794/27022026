using CatalogService.Infrastructure.Persistence;
using System.Data;
using System.Data.Common;
using CatalogService.Domain.Entities;
using Dapper;
using GssCommon.utility;

namespace CatalogService.Infrastructure.Persistence.Repositories;

public class WarehouseTypeRepository(IDbConnectionFactory connectionFactory) : IWarehouseTypeRepository
{
    public async Task<WarehouseType?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT id,
                   name,
                   label,
                   icon,
                   tooltip,
                   template_path_civil  AS templatePath_Civil,
                   template_path_json  AS templatePath_Json,
                   attributes,
                   is_active      AS IsActive,
                   is_deleted     AS IsDeleted,
                   created_at     AS CreatedAt,
                   created_by     AS CreatedBy,
                   updated_at     AS UpdatedAt,
                   updated_by     AS UpdatedBy
            FROM public.warehouse_types
            WHERE id = @Id AND is_deleted = false;";
        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<WarehouseTypeRow>(command);
        return row != null ? MapToDomain(row) : null;
    }
    public async Task<bool> ExistsAsync(string name, Guid? excludeId, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT count(1) 
            FROM public.warehouse_types 
            WHERE name = @Name 
              AND (@ExcludeId IS NULL OR id <> @ExcludeId)
              AND is_deleted = false;";
        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);
        var args = new { Name = name, ExcludeId = excludeId };
        var command = new CommandDefinition(sql, args, cancellationToken: cancellationToken);
        var count = await connection.ExecuteScalarAsync<int>(command);
        return count > 0;
    }
    public async Task<Guid> CreateAsync(WarehouseType warehouseType, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO public.warehouse_types
                (id, name, label, icon, tooltip, template_path_civil,template_path_json, attributes,
                 is_active, is_deleted, created_at, created_by, updated_at, updated_by)
            VALUES
                (@Id, @Name, @Label, @Icon, @Tooltip, @templatePath_Civil,@templatePath_Json, @Attributes::jsonb,
                 @IsActive, @IsDeleted, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy);";
        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);
        var args = new
        {
            warehouseType.Id,
            warehouseType.Name,
            warehouseType.Label,
            warehouseType.Icon,
            warehouseType.Tooltip,
            warehouseType.templatePath_Civil,
            warehouseType.templatePath_Json,
            Attributes = warehouseType.Attributes,
            warehouseType.IsActive,
            warehouseType.IsDeleted,
            warehouseType.CreatedAt,
            warehouseType.CreatedBy,
            warehouseType.UpdatedAt,
            warehouseType.UpdatedBy
        };
        var command = new CommandDefinition(sql, args, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
        return warehouseType.Id;
    }
    public async Task<bool> UpdateAsync(WarehouseType warehouseType, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE public.warehouse_types
            SET name = @Name,
                label = @Label,
                icon = @Icon,
                tooltip = @Tooltip,
                template_path_civil = @templatePath_Civil,
                template_path_json = @templatePath_Json,
                attributes = @Attributes::jsonb,
                is_active = @IsActive,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id AND is_deleted = false;";
        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);
        var args = new
        {
            warehouseType.Id,
            warehouseType.Name,
            warehouseType.Label,
            warehouseType.Icon,
            warehouseType.Tooltip,
            warehouseType.templatePath_Civil,
            warehouseType.templatePath_Json,
            Attributes = warehouseType.Attributes,
            warehouseType.IsActive,
            warehouseType.UpdatedAt,
            warehouseType.UpdatedBy
        };
        var command = new CommandDefinition(sql, args, cancellationToken: cancellationToken);
        var rowcount = await connection.ExecuteAsync(command);
        return rowcount > 0 ? true : false;
    }
    public async Task<bool> Delete(Guid id, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE public.warehouse_types
            SET is_deleted = true,
                is_active = false,
                updated_at = @UpdatedAt
            WHERE id = @Id;";
        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);
        var args = new
        {
            Id = id,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        var command = new CommandDefinition(sql, args, cancellationToken: cancellationToken);
        int rowcount = await connection.ExecuteAsync(command);
       return rowcount > 0 ? true : false;
    }
    public async Task<List<WarehouseType>> GetAllAsync(bool IsActive, CancellationToken cancellationToken)
    {
        // Note: The 'id' parameter in the interface signature for GetAllAsync is unusual 
        // if this is just getting all warehouse types. 
        // Assuming this might be for filtering, or ignoring it if fetching all.
        // Below fetches all active warehouse types. If 'id' usage is intended for something specific 
        // (like filtering by parent ID), modify the WHERE clause.
        const string sql = @"
            SELECT id,
                   name,
                   label,
                   icon,
                   tooltip,
                   template_path_civil  AS templatePath_Civil,
                   template_path_json  AS templatePath_Json,
                   attributes,
                   is_active      AS IsActive,
                   is_deleted     AS IsDeleted,
                   created_at     AS CreatedAt,
                   created_by     AS CreatedBy,
                   updated_at     AS UpdatedAt,
                   updated_by     AS UpdatedBy
            FROM public.warehouse_types
            WHERE is_deleted = false
            ORDER BY created_at DESC;";
        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);
        // Not using 'id' in the query currently.
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync<WarehouseTypeRow>(command);
        return [.. rows.Select(MapToDomain)];
    }
    private static WarehouseType MapToDomain(WarehouseTypeRow row)
    {
        // Requires a Rehydrate factory method on WarehouseType similar to AttributeDefinition.Rehydrate
        return WarehouseType.Rehydrate(
            row.Id,
            row.Name,
            row.Label,
            row.Icon,
            row.Tooltip,
            row.templatePath_Civil,
            row.templatePath_Json,
            row.Attributes,
            row.IsActive,
            row.IsDeleted,
            row.CreatedAt,
            row.CreatedBy,
            row.UpdatedAt,
            row.UpdatedBy
        );
    }




    private sealed class WarehouseTypeRow
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = default!;
        public string Label { get; set; } = default!;
        public string Icon { get; set; } = default!;
        public string? Tooltip { get; set; }
        public string? templatePath_Civil { get; private set; }
        public string? templatePath_Json { get; private set; }
        public string? Attributes { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
