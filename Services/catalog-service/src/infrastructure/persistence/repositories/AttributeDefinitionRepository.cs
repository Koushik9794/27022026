using CatalogService.Infrastructure.Persistence;
using System.Data;
using System.Data.Common;
using System.Text.Json;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Enums;
using CatalogService.Infrastructure.Persistence;
using Dapper;
using GssCommon.utility;

namespace CatalogService.Infrastructure.Persistence.Repositories;

public class AttributeDefinitionRepository(IDbConnectionFactory connectionFactory) : IAttributeDefinitionRepository
{
    public async Task<AttributeDefinition?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT id,
                   attribute_key  AS AttributeKey,
                   display_name   AS DisplayName,
                   unit,
                   data_type      AS DataType,
                   min_value      AS MinValue,
                   max_value      AS MaxValue,
                   default_value  AS DefaultValueStr,
                   is_required    AS IsRequired,
                   allowed_values AS AllowedValuesStr,
                   description    As Description,
                   screen         As Screen,
                   is_active      AS IsActive,
                   is_deleted     AS IsDeleted,
                   created_at     AS CreatedAt,
                   created_by     AS CreatedBy,
                   updated_at     AS UpdatedAt,
                   updated_by     AS UpdatedBy
            FROM public.attribute_definitions
            WHERE id = @Id AND is_deleted = false;";

        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);

        var command = new CommandDefinition(sql, new { Id = id }, cancellationToken: cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<AttributeDefinitionRow>(command);

        return row != null ? MapToDomain(row) : null;
    }

    public async Task<List<AttributeDefinition>> GetAllAsync(bool? isActive, CancellationToken cancellationToken)
    {
        string sql = @"
            SELECT id,
                   attribute_key  AS AttributeKey,
                   display_name   AS DisplayName,
                   unit,
                   data_type      AS DataType,
                   min_value      AS MinValue,
                   max_value      AS MaxValue,
                   default_value  AS DefaultValueStr,
                   is_required    AS IsRequired,
                   allowed_values AS AllowedValuesStr,
                   description    As Description,
                   screen         As Screen,
                   is_active      AS IsActive,
                   is_deleted     AS IsDeleted,
                   created_at     AS CreatedAt,
                   created_by     AS CreatedBy,
                   updated_at     AS UpdatedAt,
                   updated_by     AS UpdatedBy
            FROM public.attribute_definitions
            WHERE is_deleted = false";

        if (isActive.HasValue)
        {
            sql += " AND is_active = @IsActive";
        }

        sql += " ORDER BY created_at DESC;";

        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);

        var command = new CommandDefinition(sql, new { IsActive = isActive }, cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync<AttributeDefinitionRow>(command);

        return [.. rows.Select(MapToDomain)];
    }

    public async Task<List<AttributeDefinition>> GetByScreenAsync(AttributeScreen Screne, CancellationToken cancellationToken)
    {
        string sql = @"
            SELECT id,
                   attribute_key  AS AttributeKey,
                   display_name   AS DisplayName,
                   unit,
                   data_type      AS DataType,
                   min_value      AS MinValue,
                   max_value      AS MaxValue,
                   default_value  AS DefaultValueStr,
                   is_required    AS IsRequired,
                   allowed_values AS AllowedValuesStr,
                   description    As Description,
                   screen         As Screen,
                   is_active      AS IsActive,
                   is_deleted     AS IsDeleted,
                   created_at     AS CreatedAt,
                   created_by     AS CreatedBy,
                   updated_at     AS UpdatedAt,
                   updated_by     AS UpdatedBy
            FROM public.attribute_definitions
            WHERE is_deleted = false AND is_active=true AND screen=@Screne;";


        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);

        var command = new CommandDefinition(sql, new { Screne = Screne.ToString() }, cancellationToken: cancellationToken);
        var rows = await connection.QueryAsync<AttributeDefinitionRow>(command);

        return [.. rows.Select(MapToDomain)];
    }

    public async Task<AttributeDefinition?> GetByKeyAsync(string attributeKey, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT id,
                   attribute_key  AS AttributeKey,
                   display_name   AS DisplayName,
                   unit,
                   data_type      AS DataType,
                   min_value      AS MinValue,
                   max_value      AS MaxValue,
                   default_value  AS DefaultValueStr,
                   is_required    AS IsRequired,
                   allowed_values AS AllowedValuesStr,
                   description    As Description,
                   screen         As Screen,
                   is_active      AS IsActive,
                   is_deleted     AS IsDeleted,
                   created_at     AS CreatedAt,
                   created_by     AS CreatedBy,
                   updated_at     AS UpdatedAt,
                   updated_by     AS UpdatedBy
            FROM public.attribute_definitions
            WHERE attribute_key = @AttributeKey AND is_deleted = false;";

        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);

        var command = new CommandDefinition(sql, new { AttributeKey = attributeKey }, cancellationToken: cancellationToken);
        var row = await connection.QuerySingleOrDefaultAsync<AttributeDefinitionRow>(command);

        return row != null ? MapToDomain(row) : null;
    }

    public async Task<Guid> CreateAsync(AttributeDefinition attr, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO public.attribute_definitions
                (id, attribute_key, display_name, unit, data_type, min_value, max_value, 
                 default_value, is_required, allowed_values, description,screen,
                 is_active, is_deleted, created_at, created_by, updated_at, updated_by)
            VALUES
                (@Id, @AttributeKey, @DisplayName, @Unit, @DataType, @MinValue, @MaxValue,
                 @DefaultValue::jsonb, @IsRequired, @AllowedValues::jsonb, @Description, @Screen,
                 @IsActive, @IsDeleted, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy)
            RETURNING id;";

        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);

        var args = new
        {
            attr.Id,
            attr.AttributeKey,
            attr.DisplayName,
            attr.Unit,
            DataType = attr.DataType.ToString(),
            attr.MinValue,
            attr.MaxValue,
            DefaultValue = attr.DefaultValue?.GetRawText(),
            attr.IsRequired,
            AllowedValues = attr.AllowedValues?.GetRawText(),
            attr.Description,
            Screen = attr.Screen?.ToString(),
            attr.IsActive,
            attr.IsDeleted,
            attr.CreatedAt,
            attr.CreatedBy,
            attr.UpdatedAt,
            attr.UpdatedBy
        };

        var command = new CommandDefinition(sql, args, cancellationToken: cancellationToken);
        return await connection.ExecuteScalarAsync<Guid>(command);
    }

    public async Task<Guid> UpdateAsync(AttributeDefinition attr, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE public.attribute_definitions
            SET attribute_key = @AttributeKey,
                display_name = @DisplayName,
                unit = @Unit,
                data_type = @DataType,
                min_value = @MinValue,
                max_value = @MaxValue,
                default_value = @DefaultValue::jsonb,
                is_required = @IsRequired,
                allowed_values = @AllowedValues::jsonb,
                description = @Description,
                screen = @Screen,
                is_active = @IsActive,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id AND is_deleted = false;";

        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);

        var args = new
        {
            attr.Id,
            attr.AttributeKey,
            attr.DisplayName,
            attr.Unit,
            DataType = attr.DataType.ToString(),
            attr.MinValue,
            attr.MaxValue,
            DefaultValue = attr.DefaultValue?.GetRawText(),
            attr.IsRequired,
            AllowedValues = attr.AllowedValues?.GetRawText(),
            attr.Description,
            attr.Screen,
            attr.IsActive,
            UpdatedAt = DateTimeOffset.UtcNow,
            attr.UpdatedBy
        };

        var command = new CommandDefinition(sql, args, cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);
        return attr.Id;
    }

    public async Task<bool> DeleteAsync(Guid id, string? deletedBy, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE public.attribute_definitions
            SET is_deleted = true,
                is_active = false,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id;";

        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);

        var args = new
        {
            Id = id,
            UpdatedAt = DateTimeOffset.UtcNow,
            UpdatedBy = deletedBy
        };

        var command = new CommandDefinition(sql, args, cancellationToken: cancellationToken);
        var rowsAffected = await connection.ExecuteAsync(command);
        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(string attributeKey, Guid? excludeId, AttributeScreen Screen, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT count(1) 
            FROM public.attribute_definitions 
            WHERE attribute_key = @AttributeKey
              AND screen=@Screen
              AND (@ExcludeId IS NULL OR id <> @ExcludeId)
              AND is_deleted = false;";

        using var connection = connectionFactory.CreateConnection();
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);

        var args = new { AttributeKey = attributeKey, ExcludeId = excludeId, Screen = Screen.ToString() };
        var command = new CommandDefinition(sql, args, cancellationToken: cancellationToken);

        var count = await connection.ExecuteScalarAsync<int>(command);
        return count > 0;
    }

    private static AttributeDefinition MapToDomain(AttributeDefinitionRow row)
    {
        Enum.TryParse<AttributeDataType>(row.DataType, out var dataType);
        Enum.TryParse(row.Screen, out AttributeScreen Screen);
        return AttributeDefinition.Rehydrate(
            row.Id,
            row.AttributeKey,
            row.DisplayName,
            row.Unit,
            dataType,
            row.MinValue,
            row.MaxValue,
            JsonUtil.ParseElement(row.DefaultValue),
            row.IsRequired,
            JsonUtil.ParseElement(row.AllowedValues),
            row.Description,
            Screen,
            row.IsActive,
            row.IsDeleted,
            row.CreatedAt,
            row.CreatedBy,
            row.UpdatedAt,
            row.UpdatedBy
        );
    }

    private sealed class AttributeDefinitionRow
    {
        public Guid Id { get; set; }
        public string AttributeKey { get; set; } = default!;
        public string DisplayName { get; set; } = default!;
        public string? Unit { get; set; }
        public string DataType { get; set; } = default!;
        public decimal? MinValue { get; set; }
        public decimal? MaxValue { get; set; }
        public string? DefaultValue { get; set; }
        public bool IsRequired { get; set; }
        public string? AllowedValues { get; set; }
        public string? Description { get; set; }

        public string? Screen { get; set; }
        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }
        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }
        public DateTimeOffset? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}
