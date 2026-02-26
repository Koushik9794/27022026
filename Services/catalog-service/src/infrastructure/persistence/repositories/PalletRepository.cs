using CatalogService.Infrastructure.Persistence;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using Dapper;

namespace CatalogService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Dapper-based repository for Pallet type aggregate.
/// </summary>
public class PalletRepository(IDbConnectionFactory connectionFactory) : IPalletRepository
{
    private readonly IDbConnectionFactory _connectionFactory = connectionFactory;

    public async Task<Pallet?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT id, code, name, description, attribute_schema as attribute_schema, glb_file_path as glb_file,
                   is_active, is_deleted, created_at, created_by, updated_at, updated_by
            FROM public.Pallets
            WHERE id = @Id AND is_deleted = false";

        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<PalletRow>(sql, new { Id = id });

        return row != null ? MapToDomain(row) : null;
    }

    public async Task<Pallet?> GetByCodeAsync(string code)
    {
        const string sql = @"
            SELECT id, code, name, description, attribute_schema as attribute_schema, glb_file_path as glb_file,
                   is_active, is_deleted, created_at, created_by, updated_at, updated_by
            FROM public.Pallets
            WHERE code = @Code AND is_deleted = false";

        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<PalletRow>(sql, new { Code = code });

        return row != null ? MapToDomain(row) : null;
    }

    public async Task<IEnumerable<Pallet>> GetAllAsync(bool includeInactive = false)
    {
        var sql = @"
            SELECT id, code, name, description, attribute_schema as attribute_schema, glb_file_path as glb_file,
                   is_active, is_deleted, created_at, created_by, updated_at, updated_by
            FROM public.Pallets
            WHERE is_deleted = false";

        if (!includeInactive)
        {
            sql += " AND is_active = true";
        }

        sql += " ORDER BY name";

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<PalletRow>(sql);

        return rows.Select(MapToDomain);
    }

    public async Task<Guid> CreateAsync(Pallet pallet)
    {
        const string sql = @"
            INSERT INTO public.Pallets (id, code, name, description, attribute_schema, glb_file_path,
                                  is_active, is_deleted, created_at, created_by)
            VALUES (@Id, @Code, @Name, @Description, @AttributeSchema::jsonb, @GlbFilePath,
                    @IsActive, @IsDeleted, @CreatedAt, @CreatedBy)
            RETURNING id";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<Guid>(sql, new
        {
            pallet.Id,
            pallet.Code,
            pallet.Name,
            pallet.Description,
            AttributeSchema = pallet.AttributeSchema,
            pallet.GlbFilePath,
            pallet.IsActive,
            pallet.IsDeleted,
            pallet.CreatedAt,
            pallet.CreatedBy
        });
    }

    public async Task<bool> UpdateAsync(Pallet pallet)
    {
        const string sql = @"
            UPDATE public.Pallets
            SET name = @Name, description = @Description, 
                attribute_schema = @AttributeSchema::jsonb,
                glb_file_path = @GlbFilePath,
                is_active = @IsActive, updated_at = @UpdatedAt, updated_by = @UpdatedBy
            WHERE id = @Id AND is_deleted = false";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            pallet.Id,
            pallet.Name,
            pallet.Description,
            AttributeSchema = pallet.AttributeSchema,
            pallet.GlbFilePath,
            pallet.IsActive,
            pallet.UpdatedAt,
            pallet.UpdatedBy
        });

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id, string? deletedBy)
    {
        const string sql = @"
            UPDATE public.Pallets
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
        const string sql = @"SELECT COUNT(1) FROM public.Pallets WHERE code = @Code AND is_deleted = false";

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Code = code });

        return count > 0;
    }

    private static Pallet MapToDomain(PalletRow row)
    {
        return Pallet.Rehydrate(
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

    private record PalletRow(
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

