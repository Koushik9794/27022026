using CatalogService.Infrastructure.Persistence;
using CatalogService.Domain.Aggregates;
using Dapper;
using CatalogService.Infrastructure.Persistence;

namespace CatalogService.Infrastructure.Persistence.Repositories;

public class ComponentGroupRepository : IComponentGroupRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ComponentGroupRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ComponentGroup>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM component_groups";
        
        if (!includeInactive)
        {
            sql += " WHERE is_active = true";
        }
        
        sql += " ORDER BY sort_order";

        var entities = await connection.QueryAsync<dynamic>(sql);

        return entities.Select(e => (ComponentGroup)ComponentGroup.Rehydrate(
            e.id,
            e.code,
            e.name,
            e.description,
            e.sort_order,
            e.is_active,
            e.created_at,
            e.updated_at
        )).ToList();
    }

    public async Task<ComponentGroup?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM component_groups WHERE id = @Id";

        var entity = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Id = id });

        if (entity == null) return null;

        return ComponentGroup.Rehydrate(
            entity.id,
            entity.code,
            entity.name,
            entity.description,
            entity.sort_order,
            entity.is_active,
            entity.created_at,
            entity.updated_at
        );
    }

    public async Task<ComponentGroup?> GetByCodeAsync(string code)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM component_groups WHERE code = @Code";

        var entity = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Code = code });

        if (entity == null) return null;

        return ComponentGroup.Rehydrate(
            entity.id,
            entity.code,
            entity.name,
            entity.description,
            entity.sort_order,
            entity.is_active,
            entity.created_at,
            entity.updated_at
        );
    }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM component_groups WHERE code = @Code";
        return await connection.ExecuteScalarAsync<bool>(sql, new { Code = code });
    }

    public async Task CreateAsync(ComponentGroup componentGroup)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO component_groups (id, code, name, description, sort_order, is_active, created_at)
            VALUES (@Id, @Code, @Name, @Description, @SortOrder, @IsActive, @CreatedAt)";

        await connection.ExecuteAsync(sql, new
        {
            componentGroup.Id,
            componentGroup.Code,
            componentGroup.Name,
            componentGroup.Description,
            componentGroup.SortOrder,
            componentGroup.IsActive,
            componentGroup.CreatedAt
        });
    }

    public async Task UpdateAsync(ComponentGroup componentGroup)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE component_groups
            SET name = @Name,
                description = @Description,
                sort_order = @SortOrder,
                is_active = @IsActive,
                updated_at = @UpdatedAt
            WHERE id = @Id";

        await connection.ExecuteAsync(sql, new
        {
            componentGroup.Id,
            componentGroup.Name,
            componentGroup.Description,
            componentGroup.SortOrder,
            componentGroup.IsActive,
            componentGroup.UpdatedAt
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM component_groups WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }
}
