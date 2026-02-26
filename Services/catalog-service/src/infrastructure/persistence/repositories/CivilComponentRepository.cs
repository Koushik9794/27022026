using CatalogService.Infrastructure.Persistence;
using System.Data;
using System.Data.Common;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;
using Dapper;
using GssCommon.utility;

namespace CatalogService.Infrastructure.Persistence.Repositories;

public class CivilComponentRepository(IDbConnectionFactory connectionFactory) : ICivilComponentRepository
{
    public async Task<CivilComponent?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT id, code, name, label, icon, tooltip, category,
                   default_element AS DefaultElement,
                   is_active AS IsActive, is_deleted AS IsDeleted,
                   created_at AS CreatedAt, created_by AS CreatedBy,
                   updated_at AS UpdatedAt, updated_by AS UpdatedBy
            FROM public.civil_components
            WHERE id = @Id AND is_deleted = false;";
        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<CivilComponentRow>(command);
        return row != null ? MapToDomain(row) : null;
    }
    public async Task<CivilComponent?> GetByCodeAsync(string code, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT id, code, name, label, icon, tooltip, category,
                   default_element AS DefaultElement,
                   is_active AS IsActive, is_deleted AS IsDeleted,
                   created_at AS CreatedAt, created_by AS CreatedBy,
                   updated_at AS UpdatedAt, updated_by AS UpdatedBy
            FROM public.civil_components
            WHERE code = @Code AND is_deleted = false;";
        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { Code = code }, cancellationToken: cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<CivilComponentRow>(command);
        return row != null ? MapToDomain(row) : null;
    }
    public async Task<bool> ExistsAsync(string code, Guid? excludeId, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT count(1) 
            FROM public.civil_components 
            WHERE code = @Code 
              AND (@ExcludeId IS NULL OR id <> @ExcludeId)
              AND is_deleted = false;";
        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { Code = code, ExcludeId = excludeId }, cancellationToken: cancellationToken);
        return await connection.ExecuteScalarAsync<int>(command) > 0;
    }
    public async Task CreateAsync(CivilComponent component, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO public.civil_components
                (id, code, name, label, icon, tooltip, category, default_element,
                 is_active, is_deleted, created_at, created_by, updated_at, updated_by)
            VALUES
                (@Id, @Code, @Name, @Label, @Icon, @Tooltip, @Category, @DefaultElement::jsonb,
                 @IsActive, @IsDeleted, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy);";
        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);
        var args = new
        {
            component.Id,
            component.Code,
            component.Name,
            component.Label,
            component.Icon,
            component.Tooltip,
            component.Category,
            DefaultElement = component.DefaultElement,
            component.IsActive,
            component.IsDeleted,
            component.CreatedAt,
            component.CreatedBy,
            component.UpdatedAt,
            component.UpdatedBy
        };
        var command = new CommandDefinition(sql, args, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
    }
    public async Task UpdateAsync(CivilComponent component, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE public.civil_components
            SET code = @Code,
                name = @Name,
                label = @Label,
                icon = @Icon,
                tooltip = @Tooltip,
                category = @Category,
                default_element = @DefaultElement::jsonb,
                is_active = @IsActive,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id AND is_deleted = false;";
        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);
        var args = new
        {
            component.Id,
            component.Code,
            component.Name,
            component.Label,
            component.Icon,
            component.Tooltip,
            component.Category,
            DefaultElement = component.DefaultElement,
            component.IsActive,
            component.UpdatedAt,
            component.UpdatedBy
        };
        var command = new CommandDefinition(sql, args, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
    }
    public async Task DeleteAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE public.civil_components
            SET is_deleted = true,
                is_active = false,
                updated_at = @UpdatedAt
            WHERE id = @Id;";
        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);
        var command = new CommandDefinition(sql, new { Id = id, UpdatedAt = DateTimeOffset.UtcNow }, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
    }
    public async Task<List<CivilComponent>> GetAllAsync(bool? IncludeInactive, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT id, code, name, label, icon, tooltip, category,
                   default_element AS DefaultElement,
                   is_active AS IsActive, is_deleted AS IsDeleted,
                   created_at AS CreatedAt, created_by AS CreatedBy,
                   updated_at AS UpdatedAt, updated_by AS UpdatedBy
            FROM public.civil_components
            WHERE is_deleted = false
            ORDER BY created_at DESC;";
        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);
        var command = new CommandDefinition(sql, cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync<CivilComponentRow>(command);
        return rows.Select(MapToDomain).ToList();
    }
    private static CivilComponent MapToDomain(CivilComponentRow row)
    {
        return CivilComponent.Rehydrate(
            row.Id,
            row.Code,
            row.Name,
            row.Label,
            row.Icon,
            row.Tooltip,
            row.Category,
           row.DefaultElement,
            row.IsActive,
            row.IsDeleted,
            row.CreatedAt,
            row.CreatedBy,
            row.UpdatedAt,
            row.UpdatedBy
        );
    }
    private sealed class CivilComponentRow
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string Label { get; set; } = default!;
        public string Icon { get; set; } = default!;
        public string? Tooltip { get; set; }
        public string Category { get; set; } = default!;
        public string? DefaultElement { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
