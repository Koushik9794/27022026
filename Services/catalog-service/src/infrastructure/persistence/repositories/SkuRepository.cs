using CatalogService.Infrastructure.Persistence;
using System.Data;
using Dapper;
using CatalogService.Domain.Aggregates;

namespace CatalogService.Infrastructure.Persistence.Repositories;

public class SkuRepository(IDbConnectionFactory connectionFactory) : ISkuRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<Sku?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT id, code, name, description, attribute_schema as attribute_schema, glb_file_path as glb_file,
                   is_active, is_deleted, created_at, created_by, updated_at, updated_by
            FROM public.skus
            WHERE id = @Id AND is_deleted = false";

        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<SkuRow>(sql, new { Id = id });

        return row != null ? MapToDomain(row) : null;
    }

    public async Task<Sku?> GetByCodeAsync(string code)
    {
        const string sql = @"
            SELECT id, code, name, description, attribute_schema as attribute_schema, glb_file_path as glb_file,
                   is_active, is_deleted, created_at, created_by, updated_at, updated_by
            FROM public.skus
            WHERE code = @Code AND is_deleted = false";

        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<SkuRow>(sql, new { Code = code });

        return row != null ? MapToDomain(row) : null;
    }

    public async Task<List<Sku>> GetAllAsync()
    {
        const string sql = @"
            SELECT id, code, name, description, attribute_schema as attribute_schema, glb_file_path as glb_file,
                   is_active, is_deleted, created_at, created_by, updated_at, updated_by
            FROM public.skus
            WHERE is_deleted = false
            ORDER BY name";

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<SkuRow>(sql);

        return rows.Select(MapToDomain).ToList();
    }

    public async Task<Guid> CreateAsync(Sku sku)
    {
        const string sql = @"
            INSERT INTO public.skus (id, code, name, description, attribute_schema, glb_file_path,
                                is_active, is_deleted, created_at, created_by)
            VALUES (@Id, @Code, @Name, @Description, @AttributeSchema::jsonb, @GlbFilePath,
                    @IsActive, @IsDeleted, @CreatedAt, @CreatedBy)
            RETURNING id";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<Guid>(sql, new
        {
            sku.Id,
            sku.Code,
            sku.Name,
            sku.Description,
            AttributeSchema = sku.AttributeSchema,
            sku.GlbFilePath,
            sku.IsActive,
            sku.IsDeleted,
            sku.CreatedAt,
            sku.CreatedBy
        });
    }

    public async Task<bool> UpdateAsync(Sku sku)
    {
        const string sql = @"
            UPDATE public.skus
            SET name = @Name, description = @Description, 
                attribute_schema = @AttributeSchema::jsonb,
                glb_file_path = @GlbFilePath,
                is_active = @IsActive, updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @Id AND is_deleted = false";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            sku.Id,
            sku.Name,
            sku.Description,
            AttributeSchema = sku.AttributeSchema,
            sku.GlbFilePath,
            sku.IsActive,
            sku.UpdatedAt,
            sku.UpdatedBy
        });

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, string? deletedBy)
    {
        const string sql = @"
            UPDATE public.skus
            SET is_deleted = true, is_active = false, updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            Id = id,
            UpdatedAt = DateTime.UtcNow,
            UpdatedBy = deletedBy
        });

        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(string code)
    {
        const string sql = @"SELECT COUNT(1) FROM public.skus WHERE code = @Code AND is_deleted = false";

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Code = code });

        return count > 0;
    }

    private static Sku MapToDomain(SkuRow row)
    {
        return Sku.Rehydrate(
            row.id,
            row.code,
            row.name,
            row.description,
            row.attribute_schema ?? "{}",
            row.glb_file,
            row.is_active,
            row.is_deleted,
            row.created_at,
            row.created_by,
            row.updated_at,
            row.updated_by
        );
    }

    private record SkuRow(
        Guid id,
        string code,
        string name,
        string? description,
        string? attribute_schema,
        string? glb_file,
        bool is_active,
        bool is_deleted,
        DateTime created_at,
        string? created_by,
        DateTime? updated_at,
        string? updated_by
    );
}

