using CatalogService.Infrastructure.Persistence;
using Dapper;
using CatalogService.Domain.Aggregates;

namespace CatalogService.Infrastructure.Persistence.Repositories;

public class ProductGroupRepository : IProductGroupRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ProductGroupRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ProductGroup?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT pg.id, pg.code, pg.name, pg.description, pg.parent_group_id, 
                   pg.is_active, pg.created_at, pg.updated_at,
                   ppg.code as parent_group_code
            FROM product_groups pg
            LEFT JOIN product_groups ppg ON pg.parent_group_id = ppg.id
            WHERE pg.id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<ProductGroupRow>(sql, new { Id = id });

        return row != null ? MapToDomain(row) : null;
    }

    public async Task<ProductGroup?> GetByCodeAsync(string code)
    {
        const string sql = @"
            SELECT pg.id, pg.code, pg.name, pg.description, pg.parent_group_id, 
                   pg.is_active, pg.created_at, pg.updated_at,
                   ppg.code as parent_group_code
            FROM product_groups pg
            LEFT JOIN product_groups ppg ON pg.parent_group_id = ppg.id
            WHERE pg.code = @Code";

        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<ProductGroupRow>(sql, new { Code = code.ToUpperInvariant() });

        return row != null ? MapToDomain(row) : null;
    }

    public async Task<List<ProductGroup>> GetAllAsync(bool includeInactive = false)
    {
        var sql = @"
            SELECT pg.id, pg.code, pg.name, pg.description, pg.parent_group_id, 
                   pg.is_active, pg.created_at, pg.updated_at,
                   ppg.code as parent_group_code
            FROM product_groups pg
            LEFT JOIN product_groups ppg ON pg.parent_group_id = ppg.id";

        if (!includeInactive)
        {
            sql += " WHERE pg.is_active = true";
        }

        sql += " ORDER BY pg.parent_group_id NULLS FIRST, pg.name";

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<ProductGroupRow>(sql);

        return rows.Select(MapToDomain).ToList();
    }

    public async Task<List<ProductGroup>> GetVariantsAsync(Guid parentGroupId)
    {
        const string sql = @"
            SELECT pg.id, pg.code, pg.name, pg.description, pg.parent_group_id, 
                   pg.is_active, pg.created_at, pg.updated_at,
                   ppg.code as parent_group_code
            FROM product_groups pg
            LEFT JOIN product_groups ppg ON pg.parent_group_id = ppg.id
            WHERE pg.parent_group_id = @ParentGroupId AND pg.is_active = true
            ORDER BY pg.name";

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<ProductGroupRow>(sql, new { ParentGroupId = parentGroupId });

        return rows.Select(MapToDomain).ToList();
    }

    public async Task<Guid> CreateAsync(ProductGroup productGroup)
    {
        const string sql = @"
            INSERT INTO product_groups (id, code, name, description, parent_group_id, is_active, created_at)
            VALUES (@Id, @Code, @Name, @Description, @ParentGroupId, @IsActive, @CreatedAt)
            RETURNING id";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<Guid>(sql, new
        {
            productGroup.Id,
            productGroup.Code,
            productGroup.Name,
            productGroup.Description,
            productGroup.ParentGroupId,
            productGroup.IsActive,
            productGroup.CreatedAt
        });
    }

    public async Task<bool> UpdateAsync(ProductGroup productGroup)
    {
        const string sql = @"
            UPDATE product_groups
            SET name = @Name, description = @Description, parent_group_id = @ParentGroupId,
                is_active = @IsActive, updated_at = @UpdatedAt
            WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            productGroup.Id,
            productGroup.Name,
            productGroup.Description,
            productGroup.ParentGroupId,
            productGroup.IsActive,
            productGroup.UpdatedAt
        });

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        const string sql = "DELETE FROM product_groups WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(string code)
    {
        const string sql = "SELECT COUNT(1) FROM product_groups WHERE code = @Code";

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Code = code.ToUpperInvariant() });

        return count > 0;
    }

    private static ProductGroup MapToDomain(ProductGroupRow row)
    {
        return ProductGroup.Rehydrate(
            row.Id,
            row.Code,
            row.Name,
            row.Description,
            row.ParentGroupId,
            row.IsActive,
            row.CreatedAt,
            row.UpdatedAt,
            row.ParentGroupCode
        );
    }

    private class ProductGroupRow
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public Guid? ParentGroupId { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ParentGroupCode { get; set; }
    }
}
