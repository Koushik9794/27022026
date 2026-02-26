using CatalogService.Infrastructure.Persistence;
using System.Data;
using System.Data.Common;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using Dapper;
using GssCommon.utility;

namespace CatalogService.Infrastructure.Persistence.Repositories;

public class MheRepository(IDbConnectionFactory connectionFactory) : IMheRepository
{
    public async Task<Mhe?> GetByIdAsync(Guid id, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT id,
                   code,
                   name,
                   manufacturer,
                   brand,
                   model,
                   mhe_type       AS MheType,
                   mhe_category   AS MheCategory,
                   glb_file_path  AS GlbFilePath,
                   attributes::text AS AttributesJson,
                   is_active as IsActive,
                   is_deleted as IsDeleted,
                   created_at as CreatedAt,
                   created_by as CreatedBy,
                   updated_at as UpdatedAt,
                   updated_by as UpdatedBy
            FROM public.Mhes
            WHERE id = @Id AND is_deleted = false;";

        using var connection = connectionFactory.CreateConnection();

        // Optional but good: open with cancellation token (if provider supports it)
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);

        var args = new { Id = id };
        CommandDefinition command = new(
            commandText: sql,
            parameters: args,
            cancellationToken: cancellationToken);

        var row = await connection.QuerySingleOrDefaultAsync<MheRow>(command);

        return row != null ? MapToDomain(row) : null;
    }
    public async Task<bool> CodeExistsAsync(Guid? id, string Code, CancellationToken ct)
    {
        const string sql = @"
            select count(1)    from mhes where code = @Code and (@Id is null or id <> @Id); ";

        using var connection = connectionFactory.CreateConnection();


        var args = new { Id = id, code = Code };
        CommandDefinition command = new(
            commandText: sql,
            parameters: args,
            cancellationToken: ct);
        int row = await connection.QuerySingleOrDefaultAsync<int>(command);
        return row > 0;
    }
    public async Task<List<Mhe>> GetAllAsync(bool? IsActive, CancellationToken cancellationToken)
    {
        const string sql = @"
            SELECT id,
                   code,
                   name,
                   manufacturer,
                   brand,
                   model,
                   mhe_type       AS MheType,
                   mhe_category   AS MheCategory,
                   glb_file_path  AS GlbFilePath,
                   attributes::text AS AttributesJson,
                   is_active as IsActive,
                   is_deleted as IsDeleted,
                   created_at as CreatedAt,
                   created_by as CreatedBy,
                   updated_at as UpdatedAt,
                   updated_by as UpdatedBy
            FROM public.Mhes
            WHERE is_deleted = false
            ORDER BY created_at DESC;";

        using var connection = connectionFactory.CreateConnection();
        // Optional but good: open with cancellation token (if provider supports it)
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);


        CommandDefinition command = new(
            commandText: sql,
            parameters: null,
            cancellationToken: cancellationToken);

        var rows = await connection.QueryAsync<MheRow>(command);

        return [.. rows.Select(MapToDomain)];
    }

    public async Task<Guid> CreateAsync(Mhe mhe, CancellationToken cancellationToken)
    {
        const string sql = @"
            INSERT INTO public.Mhes
                (id, code, name, manufacturer, brand, model, mhe_type, mhe_category, glb_file_path,
                 attributes, is_active, is_deleted, created_at, created_by, updated_at, updated_by)
            VALUES
                (@Id, @Code, @Name, @Manufacturer, @Brand, @Model, @MheType, @MheCategory, @GlbFilePath,
                 @Attributes::jsonb, @IsActive, @IsDeleted, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy)
            RETURNING id;";

        using var connection = connectionFactory.CreateConnection();

        // Optional but good: open with cancellation token (if provider supports it)
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);

        var args = new
        {
            mhe.Id,
            mhe.Code,
            mhe.Name,
            mhe.Manufacturer,
            mhe.Brand,
            mhe.Model,
            mhe.MheType,
            mhe.MheCategory,
            mhe.GlbFilePath,
            Attributes = JsonUtil.SerializeDictToJson(mhe.Attributes.ToDictionary(k => k.Key, v => v.Value)),
            mhe.IsActive,
            mhe.IsDeleted,
            // If your domain uses DateTimeOffset, convert accordingly
            mhe.CreatedAt,
            mhe.CreatedBy,
            mhe.UpdatedAt,
            mhe.UpdatedBy

        };
        CommandDefinition command = new(
            commandText: sql,
            parameters: args,
            cancellationToken: cancellationToken);

        return await connection.ExecuteScalarAsync<Guid>(command);
    }

    public async Task<Guid> UpdateAsync(Mhe mhe, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE public.Mhes
            SET code = @Code,
                name = @Name,
                manufacturer = @Manufacturer,
                brand = @Brand,
                model = @Model,
                mhe_type = @MheType,
                mhe_category = @MheCategory,
                glb_file_path = @GlbFilePath,
                attributes = @Attributes::jsonb,
                is_active = @IsActive,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id AND is_deleted = false;";

        using var connection = connectionFactory.CreateConnection();
        // Optional but good: open with cancellation token (if provider supports it)
        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);

        var args = new
        {
            mhe.Id,
            mhe.Code,
            mhe.Name,
            mhe.Manufacturer,
            mhe.Brand,
            mhe.Model,
            mhe.MheType,
            mhe.MheCategory,
            mhe.GlbFilePath,
            Attributes = JsonUtil.SerializeDictToJson(mhe.Attributes.ToDictionary(k => k.Key, v => v.Value)),
            mhe.IsActive,
            UpdatedAt = DateTime.UtcNow,
            mhe.UpdatedBy
        };
        CommandDefinition command = new(
            commandText: sql,
            parameters: args,
            cancellationToken: cancellationToken);
        await connection.ExecuteAsync(command);

        return mhe.Id;
    }

    public async Task<bool> DeleteAsync(Guid id, string? deletedBy, CancellationToken cancellationToken)
    {
        const string sql = @"
            UPDATE public.Mhes
            SET is_deleted = true,
                is_active = false,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id;";

        using var connection = connectionFactory.CreateConnection();

        // Optional but recommended: open connection with cancellation support (if possible)

        if (connection is DbConnection db && db.State != ConnectionState.Open)
            await db.OpenAsync(cancellationToken);

        var args = new
        {
            Id = id,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = deletedBy
        };


        CommandDefinition cmd = new(
            commandText: sql,
            parameters: args,
            cancellationToken: cancellationToken);
        int rowsAffected = await connection.ExecuteAsync(cmd);

        return rowsAffected > 0;
    }

    // -----------------------------
    // Mapping + JSON helpers
    // -----------------------------

    private static Mhe MapToDomain(MheRow row)
    {
        var attributes = row.AttributesJson != null ? JsonUtil.ParseObjectToDict(row.AttributesJson.ToString()) : [];

        // NOTE:
        // This assumes you updated your Mhe.Rehydrate signature to include these fields:
        // (id, code, name, manufacturer, brand, model, mheType, mheCategory, glbFilePath, attributes,
        //  isActive, isDeleted, createdAt, createdBy, updatedAt, updatedBy)
        return Mhe.Rehydrate(
            row.Id,
            row.Code,
            row.Name,
            row.Manufacturer,
            row.Brand,
            row.Model,
            row.MheType,
            row.MheCategory,
            row.GlbFilePath,
            attributes,
            row.IsActive,
            row.IsDeleted,
            row.CreatedAt,
            row.CreatedBy,
          row.UpdatedAt,
            row.UpdatedBy
        );
    }




    // Dapper Row model
    private sealed class MheRow
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;

        public string? Manufacturer { get; set; }
        public string? Brand { get; set; }
        public string? Model { get; set; }

        public string? MheType { get; set; }
        public string? MheCategory { get; set; }
        public string? GlbFilePath { get; set; }

        public string? AttributesJson { get; set; }

        public bool IsActive { get; set; }
        public bool IsDeleted { get; set; }

        public DateTimeOffset CreatedAt { get; set; }
        public string? CreatedBy { get; set; }

        public DateTimeOffset? UpdatedAt { get; set; }
        public string? UpdatedBy { get; set; }
    }
}

