using System.Text.Json;
using CatalogService.application.commands.loadchart;
using CatalogService.application.dtos;
using CatalogService.Domain.Aggregates;
using Dapper;

namespace CatalogService.Infrastructure.Persistence.Repositories;

public class LoadChartRepository : ILoadChartRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public LoadChartRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<LoadChart>> GetAllAsync(
        Guid? productGroupId = null,
        string? chartType = null,
        string? componentCode = null,
        Guid? componentTypeId = null,
        bool includeDeleted = false,
        int page = 1,
        int pageSize = 50)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT 
                lc.*,
                pg.name as product_group_name,
                cn.name as component_name
            FROM load_chart lc
            JOIN product_groups pg ON lc.product_group_id = pg.id
            JOIN component_names cn ON lc.component_code = cn.code AND lc.component_type_id = cn.component_type_id
            WHERE 1=1";

        var parameters = new DynamicParameters();

        if (!includeDeleted)
        {
            sql += " AND lc.is_delete = false";
        }

        if (productGroupId.HasValue)
        {
            sql += " AND lc.product_group_id = @ProductGroupId";
            parameters.Add("ProductGroupId", productGroupId);
        }

        if (!string.IsNullOrWhiteSpace(chartType))
        {
            sql += " AND lc.chart_type = @ChartType";
            parameters.Add("ChartType", chartType);
        }

        if (!string.IsNullOrWhiteSpace(componentCode))
        {
            sql += " AND lc.component_code = @ComponentCode";
            parameters.Add("ComponentCode", componentCode);
        }

        if (componentTypeId.HasValue)
        {
            sql += " AND lc.component_type_id = @ComponentTypeId";
            parameters.Add("ComponentTypeId", componentTypeId);
        }

        sql += " ORDER BY lc.created_at DESC";

        // Pagination
        sql += " LIMIT @PageSize OFFSET @Offset";
        parameters.Add("PageSize", pageSize);
        parameters.Add("Offset", (page - 1) * pageSize);

        var entities = await connection.QueryAsync<dynamic>(sql, parameters);

        return entities.Select(MapToDomain);
    }

    public async Task<IEnumerable<LoadChart>> GetByChartTypeAsync(string chartType)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                lc.*,
                pg.name as product_group_name,
                cn.name as component_name
            FROM load_chart lc
            JOIN product_groups pg ON lc.product_group_id = pg.id
            JOIN component_names cn ON lc.component_code = cn.code AND lc.component_type_id = cn.component_type_id
            WHERE lc.chart_type = @ChartType AND lc.is_delete = false
            ORDER BY lc.created_at DESC";

        var entities = await connection.QueryAsync<dynamic>(sql, new { ChartType = chartType });
        return entities.Select(MapToDomain);
    }

    public async Task<LoadChart?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                lc.*,
                pg.name as product_group_name,
                cn.name as component_name
            FROM load_chart lc
            JOIN product_groups pg ON lc.product_group_id = pg.id
            JOIN component_names cn ON lc.component_code = cn.code AND lc.component_type_id = cn.component_type_id
            WHERE lc.id = @Id";

        var entity = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Id = id });

        if (entity == null) return null;

        return MapToDomain(entity);
    }

    public async Task<Guid> CreateAsync(LoadChart loadChart)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO load_chart (
                id, product_group_id, chart_type, component_code, component_type_id,
                attributes, is_active, is_delete, 
                created_by, updated_by, created_at, updated_at
            )
            VALUES (
                @Id, @ProductGroupId, @ChartType, @ComponentCode, @ComponentTypeId,
                CAST(@AttributesJson AS JSONB), 
                @IsActive, @IsDelete, @CreatedBy, @UpdatedBy, @CreatedAt, @UpdatedAt
            )
            RETURNING id";

        return await connection.ExecuteScalarAsync<Guid>(sql, new
        {
            loadChart.Id,
            loadChart.ProductGroupId,
            loadChart.ChartType,
            loadChart.ComponentCode,
            loadChart.ComponentTypeId,
            AttributesJson = JsonSerializer.Serialize(loadChart.Attributes),
            loadChart.IsActive,
            loadChart.IsDelete,
            loadChart.CreatedBy,
            loadChart.UpdatedBy,
            loadChart.CreatedAt,
            loadChart.UpdatedAt
        });
    }

    public async Task<bool> UpdateAsync(LoadChart loadChart)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE load_chart
            SET 
                product_group_id = @ProductGroupId,
                chart_type = @ChartType,
                component_code = @ComponentCode,
                component_type_id = @ComponentTypeId,
                attributes = CAST(@AttributesJson AS JSONB),
                is_active = @IsActive,
                is_delete = @IsDelete,
                updated_by = @UpdatedBy,
                updated_at = @UpdatedAt
            WHERE id = @Id";

        var rowsAffected = await connection.ExecuteAsync(sql, new
        {
            loadChart.Id,
            loadChart.ProductGroupId,
            loadChart.ChartType,
            loadChart.ComponentCode,
            loadChart.ComponentTypeId,
            AttributesJson = JsonSerializer.Serialize(loadChart.Attributes),
            loadChart.IsActive,
            loadChart.IsDelete,
            loadChart.UpdatedBy,
            loadChart.UpdatedAt
        });

        return rowsAffected > 0;
    }

    public async Task<bool> DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE load_chart SET is_delete = true, updated_at = @UpdatedAt WHERE id = @Id";
        var rowsAffected = await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
        return rowsAffected > 0;
    }

    private LoadChart MapToDomain(dynamic entity)
    {
        Dictionary<string, JsonElement> attributes = DeserializeJson(entity.attributes);

        return LoadChart.Rehydrate(
            entity.id,
            entity.product_group_id,
            entity.chart_type,
            entity.component_code,
            entity.component_type_id,
            attributes,
            entity.is_active,
            entity.is_delete,
            entity.created_by,
            entity.updated_by,
            entity.created_at,
            entity.updated_at,
            entity.product_group_name,
            entity.component_name
        );
    }

    private Dictionary<string, JsonElement> DeserializeJson(dynamic? jsonField)
    {
        if (jsonField == null) return [];

        if (jsonField is string jsonStr && !string.IsNullOrWhiteSpace(jsonStr))
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonStr) ?? [];
            }
            catch { return []; }
        }
        else if (jsonField is JsonElement element && element.ValueKind == JsonValueKind.Object)
        {
            try
            {
                return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(element.GetRawText()) ?? [];
            }
            catch { return []; }
        }
        else if (jsonField is Dictionary<string, object> dict)
        {
            try
            {
                var json = JsonSerializer.Serialize(dict);
                return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? [];
            }
            catch { return []; }
        }

        return [];
    }

    public async Task<IEnumerable<LoadChartCandidateDto>> GetLoadchartbysearch(string request) {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
WITH
payload AS (
  SELECT @payload::jsonb AS j
),

requested_types AS (
  SELECT upper(x.value) AS chart_type
  FROM payload p
  CROSS JOIN LATERAL jsonb_array_elements_text(p.j->'chart_type') AS x(value)
),


cfg_levels AS (
  SELECT
    e.key AS level_key,
    regexp_replace(e.key, '\D', '', 'g')::int AS level_no,
    (e.value->>'USL')::int AS req_usl,
    (e.value->>'Capacity')::numeric AS req_level_load
  FROM payload p
  CROSS JOIN LATERAL jsonb_each(p.j->'levelConfigs') AS e(key, value)
),

req AS (
  SELECT
    (p.j->>'prodctgroup')::varchar AS prodctgroup,
    (p.j->>'beamSpan')::int AS beam_span,
    COALESCE((p.j->>'IsStiffenerenable')::boolean, false) AS stiffener_required,
    COALESCE(SUM(l.req_level_load), 0) AS total_req_upright_load
  FROM payload p
  LEFT JOIN cfg_levels l ON true
  GROUP BY p.j
),

upright_candidates_per_level AS (
  SELECT
    'UPRIGHT'::text AS component_type,
    l.level_key,
    l.level_no,
    l.req_usl,

    lc.attributes->>'profileText' AS profile_text,

    CASE
      WHEN COALESCE(lc.attributes->>'bracing','') = 'Normal Bracing'
           AND COALESCE((lc.attributes->>'hasStiffener')::boolean, false) = false THEN 'D'
      WHEN COALESCE(lc.attributes->>'bracing','') = 'X Bracing'
           AND COALESCE((lc.attributes->>'hasStiffener')::boolean, false) = false THEN 'X'
      WHEN COALESCE(lc.attributes->>'bracing','') = 'Normal Bracing+Stiffener'
           AND COALESCE((lc.attributes->>'hasStiffener')::boolean, false) = true THEN 'D+S'
      WHEN COALESCE(lc.attributes->>'bracing','') = 'X Bracing+Stiffener'
           AND COALESCE((lc.attributes->>'hasStiffener')::boolean, false) = true THEN 'X+S'
      ELSE NULL
    END AS Bracing_Type,

    (lc.attributes->>'capacity')::numeric AS chart_capacity,

      r.total_req_upright_load AS req_load,

    ROUND(
      100 * (r.total_req_upright_load / NULLIF((lc.attributes->>'capacity')::numeric, 0)),
      2
    ) AS utilization_pct

  FROM cfg_levels l
  CROSS JOIN req r
  JOIN product_groups pg ON pg.code = r.prodctgroup
  JOIN load_chart lc ON lc.product_group_id = pg.id

  WHERE lc.chart_type = 'UPRIGHT'
    AND (lc.attributes->>'usl')::int = l.req_usl
    AND (lc.attributes->>'capacity')::numeric >= r.total_req_upright_load

    /* Keep your stiffener rule */
    AND (
      r.stiffener_required = true
      OR COALESCE((lc.attributes->>'hasStiffener')::boolean, false) = false
    )
),

beam_candidates_per_level AS (
  SELECT
    'BEAM'::text AS component_type,
    l.level_key,
    l.level_no,
    l.req_usl, -- kept only for reference/output

    lc.attributes->>'profileText' AS profile_text,
    '' AS Bracing_Type,

    (lc.attributes->>'capacity')::numeric AS chart_capacity,

    l.req_level_load AS req_load,

    ROUND(
      100 * (l.req_level_load / NULLIF((lc.attributes->>'capacity')::numeric, 0)),
      2
    ) AS utilization_pct

  FROM cfg_levels l
  CROSS JOIN req r
  JOIN product_groups pg ON pg.code = r.prodctgroup
  JOIN load_chart lc ON lc.product_group_id = pg.id
  WHERE lc.chart_type = 'BEAM'
    AND COALESCE(
          lc.attributes->>'beamSpan',
          lc.attributes->>'Beamspan',
          lc.attributes->>'span'
        )::int = r.beam_span
    AND (lc.attributes->>'capacity')::numeric >= l.req_level_load
),
all_options AS (
  SELECT * FROM upright_candidates_per_level
  UNION ALL
  SELECT * FROM beam_candidates_per_level
)

SELECT * FROM all_options ao WHERE EXISTS (
SELECT 1 FROM requested_types rt WHERE rt.chart_type = ao.component_type
)
group by level_key,level_no,  component_type,  Bracing_Type,  utilization_pct,profile_text,req_usl,chart_capacity,req_load
ORDER BY   component_type, level_no, Bracing_Type, utilization_pct DESC, profile_text;";

        var cmd = new CommandDefinition(
             sql,
             new { payload = request });

        var entity = await connection.QueryAsync<LoadChartCandidateDto>(cmd);

        if (entity == null) return null;
        return entity;
    }
}
