using CatalogService.Infrastructure.Persistence;
using System.Text.Json;
using Dapper;
using CatalogService.Domain.Aggregates;

namespace CatalogService.Infrastructure.Persistence.Repositories;

public class ComponentTypeRepository : IComponentTypeRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ComponentTypeRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<ComponentType?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT ct.id, ct.code, ct.name, ct.description, ct.component_group_id, ct.parent_type_id,
                   ct.attribute_schema, ct.is_active, ct.created_at, ct.updated_at,
                   cg.code as component_group_code, cg.name as component_group_name,
                   pt.code as parent_type_code
            FROM component_types ct
            JOIN component_groups cg ON ct.component_group_id = cg.id
            LEFT JOIN component_types pt ON ct.parent_type_id = pt.id
            WHERE ct.id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<TypeRow>(sql, new { Id = id });

        return row != null ? MapToDomain(row) : null;
    }

    public async Task<ComponentType?> GetByCodeAsync(string code)
    {
        const string sql = @"
            SELECT ct.id, ct.code, ct.name, ct.description, ct.component_group_id, ct.parent_type_id,
                   ct.attribute_schema, ct.is_active, ct.created_at, ct.updated_at,
                   cg.code as component_group_code, cg.name as component_group_name,
                   pt.code as parent_type_code
            FROM component_types ct
            JOIN component_groups cg ON ct.component_group_id = cg.id
            LEFT JOIN component_types pt ON ct.parent_type_id = pt.id
            WHERE ct.code = @Code";

        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<TypeRow>(sql, new { Code = code.ToUpperInvariant() });

        return row != null ? MapToDomain(row) : null;
    }

    public async Task<List<ComponentType>> GetAllAsync(string? componentGroupCode = null, Guid? componentGroupId = null, bool includeInactive = false)
    {
        var sql = @"
            SELECT ct.id, ct.code, ct.name, ct.description, ct.component_group_id, ct.parent_type_id,
                   ct.attribute_schema, ct.is_active, ct.created_at, ct.updated_at,
                   cg.code as component_group_code, cg.name as component_group_name,
                   pt.code as parent_type_code
            FROM component_types ct
            JOIN component_groups cg ON ct.component_group_id = cg.id
            LEFT JOIN component_types pt ON ct.parent_type_id = pt.id
            WHERE 1=1";

        if (!string.IsNullOrEmpty(componentGroupCode))
        {
            sql += " AND cg.code = @ComponentGroupCode";
        }

        if (componentGroupId.HasValue)
        {
            sql += " AND ct.component_group_id = @ComponentGroupId";
        }

        if (!includeInactive)
        {
            sql += " AND ct.is_active = true";
        }

        sql += " ORDER BY cg.sort_order, ct.name";

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<TypeRow>(sql, new 
        { 
            ComponentGroupCode = componentGroupCode?.ToUpperInvariant(),
            ComponentGroupId = componentGroupId
        });

        return rows.Select(MapToDomain).ToList();
    }

    public async Task<List<ComponentType>> GetByGroupIdAsync(Guid componentGroupId)
    {
        const string sql = @"
            SELECT ct.id, ct.code, ct.name, ct.description, ct.component_group_id, ct.parent_type_id,
                   ct.attribute_schema, ct.is_active, ct.created_at, ct.updated_at,
                   cg.code as component_group_code, cg.name as component_group_name,
                   pt.code as parent_type_code
            FROM component_types ct
            JOIN component_groups cg ON ct.component_group_id = cg.id
            LEFT JOIN component_types pt ON ct.parent_type_id = pt.id
            WHERE ct.component_group_id = @ComponentGroupId AND ct.is_active = true
            ORDER BY ct.name";

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<TypeRow>(sql, new { ComponentGroupId = componentGroupId });

        return rows.Select(MapToDomain).ToList();
    }

    public async Task<Guid> CreateAsync(ComponentType componentType)
    {
        const string sql = @"
            INSERT INTO component_types (id, code, name, description, component_group_id, parent_type_id, attribute_schema, is_active, created_at)
            VALUES (@Id, @Code, @Name, @Description, @ComponentGroupId, @ParentTypeId, @AttributeSchema::jsonb, @IsActive, @CreatedAt)
            RETURNING id";

        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<Guid>(sql, new
        {
            componentType.Id,
            componentType.Code,
            componentType.Name,
            componentType.Description,
            componentType.ComponentGroupId,
            componentType.ParentTypeId,
            AttributeSchema = componentType.AttributeSchema?.RootElement.GetRawText(),
            componentType.IsActive,
            componentType.CreatedAt
        });
    }

    public async Task<bool> UpdateAsync(ComponentType componentType)
    {
        const string sql = @"
            UPDATE component_types
            SET name = @Name, description = @Description, component_group_id = @ComponentGroupId,
                parent_type_id = @ParentTypeId, attribute_schema = @AttributeSchema::jsonb,
                is_active = @IsActive, updated_at = @UpdatedAt
            WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            componentType.Id,
            componentType.Name,
            componentType.Description,
            componentType.ComponentGroupId,
            componentType.ParentTypeId,
            AttributeSchema = componentType.AttributeSchema?.RootElement.GetRawText(),
            componentType.IsActive,
            componentType.UpdatedAt
        });

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        const string sql = "DELETE FROM component_types WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id });

        return rowsAffected > 0;
    }

    public async Task<bool> ExistsAsync(string code)
    {
        const string sql = "SELECT COUNT(1) FROM component_types WHERE code = @Code";

        using var connection = _connectionFactory.CreateConnection();
        var count = await connection.ExecuteScalarAsync<int>(sql, new { Code = code.ToUpperInvariant() });

        return count > 0;
    }

    private static ComponentType MapToDomain(TypeRow row)
    {
        JsonDocument? attributeSchema = null;
        if (!string.IsNullOrEmpty(row.AttributeSchema))
        {
            attributeSchema = JsonDocument.Parse(row.AttributeSchema);
        }

        return ComponentType.Rehydrate(
            row.Id,
            row.Code,
            row.Name,
            row.Description,
            row.ComponentGroupId,
            row.ParentTypeId,
            attributeSchema,
            row.IsActive,
            row.CreatedAt,
            row.UpdatedAt,
            row.ComponentGroupCode,
            row.ComponentGroupName,
            row.ParentTypeCode
        );
    }

    private class TypeRow
    {
        public Guid Id { get; set; }
        public string Code { get; set; } = default!;
        public string Name { get; set; } = default!;
        public string? Description { get; set; }
        public Guid ComponentGroupId { get; set; }
        public Guid? ParentTypeId { get; set; }
        public string? AttributeSchema { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public string? ComponentGroupCode { get; set; }
        public string? ComponentGroupName { get; set; }
        public string? ParentTypeCode { get; set; }
    }
}
