using System.Text.Json;
using CatalogService.Domain.Aggregates;
using Dapper;

namespace CatalogService.Infrastructure.Persistence.Repositories;

public class ComponentMasterRepository : IComponentMasterRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ComponentMasterRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ComponentMaster>> GetAllAsync(
        string? countryCode = null,
        Guid? componentGroupId = null,
        Guid? componentTypeId = null,
        bool? isActive = true,
        bool includeDeleted = false,
        int page = 1,
        int pageSize = 50)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = @"
            SELECT 
                cm.*,
                cg.code as component_group_code, cg.name as component_group_name,
                ct.code as component_type_code, ct.name as component_type_name,
                cn.code as component_name_code, cn.name as component_name_name
            FROM component_masters cm
            JOIN component_groups cg ON cm.component_group_id = cg.id
            JOIN component_types ct ON cm.component_type_id = ct.id
            LEFT JOIN component_names cn ON cm.component_name_id = cn.id
            WHERE 1=1";

        var parameters = new DynamicParameters();

        if (!includeDeleted)
        {
            sql += " AND cm.is_deleted = false";
        }

        if (!string.IsNullOrWhiteSpace(countryCode))
        {
            sql += " AND cm.country_code = @CountryCode";
            parameters.Add("CountryCode", countryCode);
        }

        if (componentGroupId.HasValue)
        {
            sql += " AND cm.component_group_id = @GroupId";
            parameters.Add("GroupId", componentGroupId);
        }

        if (componentTypeId.HasValue)
        {
            sql += " AND cm.component_type_id = @TypeId";
            parameters.Add("TypeId", componentTypeId);
        }

        if (isActive.HasValue)
        {
            sql += " AND cm.status = @Status";
            parameters.Add("Status", isActive.Value ? "ACTIVE" : "INACTIVE");
        }

        sql += " ORDER BY cm.created_at DESC";

        sql += " LIMIT @PageSize OFFSET @Offset";
        parameters.Add("PageSize", pageSize);
        parameters.Add("Offset", (page - 1) * pageSize);

        var entities = await connection.QueryAsync<dynamic>(sql, parameters);

        return entities.Select(MapToDomain);
    }

    public async Task<ComponentMaster?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                cm.*,
                cg.code as component_group_code, cg.name as component_group_name,
                ct.code as component_type_code, ct.name as component_type_name,
                cn.code as component_name_code, cn.name as component_name_name
            FROM component_masters cm
            JOIN component_groups cg ON cm.component_group_id = cg.id
            JOIN component_types ct ON cm.component_type_id = ct.id
            LEFT JOIN component_names cn ON cm.component_name_id = cn.id
            WHERE cm.id = @Id";

        var entity = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { Id = id });

        if (entity == null) return null;

        return MapToDomain(entity);
    }

    public async Task<ComponentMaster?> GetByCodeAndCountryAsync(string componentMasterCode, string countryCode)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT 
                cm.*,
                cg.code as component_group_code, cg.name as component_group_name,
                ct.code as component_type_code, ct.name as component_type_name,
                cn.code as component_name_code, cn.name as component_name_name
            FROM component_masters cm
            JOIN component_groups cg ON cm.component_group_id = cg.id
            JOIN component_types ct ON cm.component_type_id = ct.id
            LEFT JOIN component_names cn ON cm.component_name_id = cn.id
            WHERE cm.component_master_code = @ComponentMasterCode AND cm.country_code = @CountryCode";

        var entity = await connection.QuerySingleOrDefaultAsync<dynamic>(sql, new { ComponentMasterCode = componentMasterCode, CountryCode = countryCode });

        if (entity == null) return null;

        return MapToDomain(entity);
    }

    public async Task<bool> ExistsByCodeAndCountryAsync(string componentMasterCode, string countryCode)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM component_masters WHERE component_master_code = @ComponentMasterCode AND country_code = @CountryCode";
        return await connection.ExecuteScalarAsync<bool>(sql, new { ComponentMasterCode = componentMasterCode, CountryCode = countryCode });
    }

    public async Task CreateAsync(ComponentMaster cm)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO component_masters (
                id, component_master_code, country_code, unspsc_code, 
                component_group_id, component_type_id, component_name_id, 
                colour, powder_code, gfa_flag, 
                unit_basic_price, cbm, 
                short_description, description, 
                drawing_no, rev_no, installation_ref_no, 
                attributes, glb_filepath, image_url, status, is_deleted, 
                created_at, created_by
            )
            VALUES (
                @Id, @ComponentMasterCode, @CountryCode, @UnspscCode, 
                @ComponentGroupId, @ComponentTypeId, @ComponentNameId, 
                @Colour, @PowderCode, @GfaFlag, 
                @UnitBasicPrice, @Cbm, 
                @ShortDescription, @Description, 
                @DrawingNo, @RevNo, @InstallationRefNo, 
                CAST(@AttributesJson AS JSONB), @GlbFilepath, @ImageUrl, @Status, @IsDeleted, 
                @CreatedAt, @CreatedBy
            )";

        await connection.ExecuteAsync(sql, new
        {
            cm.Id,
            cm.ComponentMasterCode,
            cm.CountryCode,
            cm.UnspscCode,
            cm.ComponentGroupId,
            cm.ComponentTypeId,
            cm.ComponentNameId,
            cm.Colour,
            cm.PowderCode,
            cm.GfaFlag,
            cm.UnitBasicPrice,
            cm.Cbm,
            cm.ShortDescription,
            cm.Description,
            cm.DrawingNo,
            cm.RevNo,
            cm.InstallationRefNo,
            AttributesJson = JsonSerializer.Serialize(cm.Attributes),
            cm.GlbFilepath,
            cm.ImageUrl,
            cm.Status,
            cm.IsDeleted,
            cm.CreatedAt,
            cm.CreatedBy
        });
    }

    public async Task UpdateAsync(ComponentMaster cm)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE component_masters
            SET 
                unspsc_code = @UnspscCode,
                component_group_id = @ComponentGroupId,
                component_type_id = @ComponentTypeId,
                component_name_id = @ComponentNameId,
                colour = @Colour,
                powder_code = @PowderCode,
                gfa_flag = @GfaFlag,
                unit_basic_price = @UnitBasicPrice,
                cbm = @Cbm,
                short_description = @ShortDescription,
                description = @Description,
                drawing_no = @DrawingNo,
                rev_no = @RevNo,
                installation_ref_no = @InstallationRefNo,
                attributes = CAST(@AttributesJson AS JSONB),
                glb_filepath = @GlbFilepath,
                image_url = @ImageUrl,
                status = @Status,
                is_deleted = @IsDeleted,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id";

        await connection.ExecuteAsync(sql, new
        {
            cm.Id,
            cm.UnspscCode,
            cm.ComponentGroupId,
            cm.ComponentTypeId,
            cm.ComponentNameId,
            cm.Colour,
            cm.PowderCode,
            cm.GfaFlag,
            cm.UnitBasicPrice,
            cm.Cbm,
            cm.ShortDescription,
            cm.Description,
            cm.DrawingNo,
            cm.RevNo,
            cm.InstallationRefNo,
            AttributesJson = JsonSerializer.Serialize(cm.Attributes),
            cm.GlbFilepath,
            cm.ImageUrl,
            cm.Status,
            cm.IsDeleted,
            cm.UpdatedAt,
            cm.UpdatedBy
        });
    }

    private ComponentMaster MapToDomain(dynamic entity)
    {
        Dictionary<string, JsonElement> attributes = [];
        if (entity.attributes != null)
        {
            if (entity.attributes is string jsonStr && !string.IsNullOrWhiteSpace(jsonStr))
            {
                try { attributes = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(jsonStr) ?? []; } catch { }
            }
            else if (entity.attributes is JsonElement element && element.ValueKind == JsonValueKind.Object)
            {
                try { attributes = JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(element.GetRawText()) ?? []; } catch { }
            }
        }

        return ComponentMaster.Rehydrate(
            entity.id,
            entity.component_master_code,
            entity.country_code,
            entity.unspsc_code,
            entity.component_group_id,
            entity.component_type_id,
            entity.component_name_id,
            entity.colour,
            entity.powder_code,
            entity.gfa_flag,
            entity.unit_basic_price,
            entity.cbm,
            entity.short_description,
            entity.description,
            entity.drawing_no,
            entity.rev_no,
            entity.installation_ref_no,
            attributes,
            entity.glb_filepath,
            entity.image_url,
            entity.status,
            entity.is_deleted,
            entity.created_at,
            entity.updated_at,
            entity.created_by,
            entity.updated_by,
            entity.component_group_code,
            entity.component_group_name,
            entity.component_type_code,
            entity.component_type_name,
            entity.component_name_code,
            entity.component_name_name
        );
    }
}
