using CatalogService.Infrastructure.Persistence;
using CatalogService.Domain.Aggregates;
using Dapper;
using CatalogService.Infrastructure.Persistence;

namespace CatalogService.Infrastructure.Persistence.Repositories;

public class ComponentNameRepository : IComponentNameRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ComponentNameRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ComponentName>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT cn.*, ct.code as category_code, ct.name as category_name -- aliasing ct columns to avoid conflicts if creating generic mapper, but here standard manual mapping
            , ct.code as component_type_code, ct.name as component_type_name
            FROM component_names cn
            JOIN component_types ct ON cn.component_type_id = ct.id";
        
        if (!includeInactive)
        {
            sql += " WHERE cn.is_active = true";
        }
        
        sql += " ORDER BY cn.name";

        var entities = await connection.QueryAsync<dynamic>(sql);

        return entities.Select(MapToDomain);
    }

    public async Task<IEnumerable<ComponentName>> GetByTypeIdAsync(Guid componentTypeId, bool includeInactive = false)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT cn.*, ct.code as component_type_code, ct.name as component_type_name
            FROM component_names cn
            JOIN component_types ct ON cn.component_type_id = ct.id
            WHERE cn.component_type_id = @TypeId";
        
        if (!includeInactive)
        {
            sql += " AND cn.is_active = true";
        }
        
        sql += " ORDER BY cn.name";

        var entities = await connection.QueryAsync<dynamic>(sql, new { TypeId = componentTypeId });

        return entities.Select(MapToDomain);
    }

    public async Task<ComponentName?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT cn.*, ct.code as component_type_code, ct.name as component_type_name
            FROM component_names cn
            JOIN component_types ct ON cn.component_type_id = ct.id
            WHERE cn.id = @Id";

        var entity = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Id = id });

        if (entity == null) return null;

        return MapToDomain(entity);
    }

    public async Task<ComponentName?> GetByCodeAsync(string code)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT cn.*, ct.code as component_type_code, ct.name as component_type_name
            FROM component_names cn
            JOIN component_types ct ON cn.component_type_id = ct.id
            WHERE cn.code = @Code";

        var entity = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Code = code });

        if (entity == null) return null;

        return MapToDomain(entity);
    }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM component_names WHERE code = @Code";
        return await connection.ExecuteScalarAsync<bool>(sql, new { Code = code });
    }

    public async Task CreateAsync(ComponentName componentName)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO component_names (id, code, name, description, component_type_id, is_active, created_at)
            VALUES (@Id, @Code, @Name, @Description, @ComponentTypeId, @IsActive, @CreatedAt)";

        await connection.ExecuteAsync(sql, new
        {
            componentName.Id,
            componentName.Code,
            componentName.Name,
            componentName.Description,
            componentName.ComponentTypeId,
            componentName.IsActive,
            componentName.CreatedAt
        });
    }

    public async Task UpdateAsync(ComponentName componentName)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE component_names
            SET name = @Name,
                description = @Description,
                component_type_id = @ComponentTypeId,
                is_active = @IsActive,
                updated_at = @UpdatedAt
            WHERE id = @Id";

        await connection.ExecuteAsync(sql, new
        {
            componentName.Id,
            componentName.Name,
            componentName.Description,
            componentName.ComponentTypeId,
            componentName.IsActive,
            componentName.UpdatedAt
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "DELETE FROM component_names WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    private ComponentName MapToDomain(dynamic entity)
    {
        return ComponentName.Rehydrate(
            entity.id,
            entity.code,
            entity.name,
            entity.description,
            entity.component_type_id,
            entity.is_active,
            entity.created_at,
            entity.updated_at,
            entity.component_type_code,
            entity.component_type_name
        );
    }
}
