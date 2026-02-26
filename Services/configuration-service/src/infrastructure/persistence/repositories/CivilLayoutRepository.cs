using System.Diagnostics;
using ConfigurationService.Application.Dtos;
using ConfigurationService.Domain.Aggregates;
using ConfigurationService.Domain.Enums;
using ConfigurationService.Infrastructure.Persistence;
using Dapper;

namespace ConfigurationService.Infrastructure.Persistence.Repositories;

public class CivilLayoutRepository : ICivilLayoutRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CivilLayoutRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<RackLayout?> GetRackLayoutByIdAsync(Guid Id)
    {
        const string sql = @"
            SELECT id, configuration_version_id, rack_json, configuration_layout, 
                   created_at, created_by, updated_at, updated_by
            FROM rack_layout
            WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QueryFirstOrDefaultAsync<RackLayoutRow>(sql, new { Id = Id });

        if (row == null) return null;

        return RackLayout.Rehydrate(
            row.id, row.civil_layout_id, row.configuration_version_id ,row.rack_json,
            row.configuration_layout != null ? System.Text.Json.JsonDocument.Parse(row.configuration_layout) : null,
            row.created_at, row.created_by, row.updated_at, row.updated_by
        );
    }
    public async Task<Configuration?> GetRackLayoutByVersionIdAsync(Guid id, int VersionNo)
    {
        const string sql = @"
            SELECT id, enquiry_id, name, description,
                   is_active, is_primary, created_at, created_by, updated_at, updated_by
            FROM configurations
            WHERE id = @Id;

            SELECT id, configuration_id, version_number, description, is_current, created_at, created_by
            FROM configuration_versions
            WHERE configuration_id = @Id AND version_number=@VersionNo;

            SELECT C.id,C.configuration_id,C.warehouse_type, C.Source_file,C.civil_json,C.versionno,
			C.created_at,C.created_by,C.updated_at,C.updated_by 
            FROM Rack_layout R Join civil_Layout C
			on R.civil_layout_id=C.id 
			WHERE configuration_version_id IN (
			SELECT id FROM configuration_versions WHERE Configuration_id=@Id
            AND version_number=@VersionNo);

            SELECT id,configuration_version_id,civil_layout_id,rack_json,configuration_layout,created_at,created_by,updated_at,updated_by  
            FROM rack_layout WHERE configuration_version_id IN (SELECT id FROM configuration_versions WHERE configuration_id = @Id
            AND version_number=@VersionNo);";

        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(sql, new { Id = id, VersionNo= VersionNo });

        var configRow = await multi.ReadFirstOrDefaultAsync<ConfigurationRow>();
        if (configRow == null) return null;

        var versionRows = (await multi.ReadAsync<ConfigurationVersionRow>()).ToList();
        var civilrows = (await multi.ReadAsync<CivilLayoutRow>()).ToList();
        var rackrows = (await multi.ReadAsync<RackLayoutRow>()).ToList();


        var civilLayouts = civilrows.Select(row => CivilLayout.Rehydrate(
            row.id, row.configuration_id, row.warehouse_type, row.source_file,
            row.civil_json, row.versionno, row.created_at, row.created_by, row.updated_at, row.updated_by
        )).ToList();

        var rackLayouts = rackrows.Select(row => RackLayout.Rehydrate(
            row.id, row.civil_layout_id, row.configuration_version_id,
            row.rack_json, 
            row.configuration_layout != null ? System.Text.Json.JsonDocument.Parse(row.configuration_layout) : null,
            row.created_at, row.created_by, row.updated_at, row.updated_by
        )).ToList();





        var versions = versionRows.Select(v => ConfigurationVersion.Rehydrate(
            v.id, v.configuration_id, v.version_number, v.description, v.is_current,v.is_locked,v.status, v.created_at, v.created_by,
            storageConfigurations: null,
            mheConfigs: null
        ));

        return Configuration.Rehydrate(
            configRow.id, configRow.enquiry_id, configRow.name, configRow.description,
            configRow.is_active, configRow.is_primary, configRow.created_at, configRow.created_by, configRow.updated_at, configRow.updated_by,
            versions, civilLayouts, rackLayouts
        );
    }

    public async Task<CivilLayout?> GetCivilLayoutByIdAsync(Guid Id)
    {
        const string sql = @"
            SELECT id, configuration_id, warehouse_type, source_file, civil_json, versionno, 
                   created_at, created_by, updated_at, updated_by
            FROM civil_layout
            WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QueryFirstOrDefaultAsync<CivilLayoutRow>(sql, new { Id = Id });

        if (row == null) return null;

        return CivilLayout.Rehydrate(
            row.id, row.configuration_id, row.warehouse_type, row.source_file, row.civil_json, row.versionno,
            row.created_at, row.created_by, row.updated_at, row.updated_by
        );
    }
    public async Task<RevisionIdsDto> GetRevisionIdAsync(Guid ConfigId, int ConfigversionNo,int CivilVersionNo) {
        const string sql = @"
            SELECT id
            FROM Configuration_versions
            WHERE Configuration_id = @ConfigId and version_number=@ConfigversionNo;

            SELECT id, configuration_id, warehouse_type, source_file, civil_json, versionno, 
                   created_at, created_by, updated_at, updated_by
            FROM civil_layout
            WHERE configuration_id = @ConfigId and versionno=@CivilVersionNo
";

        using var connection = _connectionFactory.CreateConnection();
        var args = new { ConfigId, ConfigversionNo, CivilVersionNo };
        using var multi = await connection.QueryMultipleAsync(sql, args);


        var configurationVersionId = await multi.ReadSingleOrDefaultAsync<Guid>();
        var civilLayoutId = await multi.ReadSingleOrDefaultAsync<Guid>();

        return new RevisionIdsDto(configurationVersionId, civilLayoutId);
    }
    public async Task<ConfigurationDto?> GetCivilLayoutByConfigurationIdAsync(Guid ConfigId)
    {
        var sql = @"
            SELECT id, enquiry_id, name, description,
            is_active, is_primary, created_at, created_by, updated_at, updated_by
            FROM configurations
            WHERE id = @ConfigId;

            SELECT id, configuration_id, warehouse_type, source_file, civil_json, versionno, 
                   created_at, created_by, updated_at, updated_by
            FROM civil_layout
            WHERE configuration_id = @ConfigId";


        using var connection = _connectionFactory.CreateConnection();
        var args = new { ConfigId };
        using var multi = await connection.QueryMultipleAsync(sql, args);

        var config = await multi.ReadSingleOrDefaultAsync<ConfigurationRow>();
        var Civils = (await multi.ReadAsync<CivilLayoutRow>()).ToList();

        return new ConfigurationDto(
            Id: config.id,
            EnquiryId: config.enquiry_id,
            Name: config.name,
            Description: config.description,
            IsActive: config.is_active,
            IsPrimary: config.is_primary,
            CreatedAt: config.created_at,
            CreatedBy: config.created_by,
            Civil: Civils.Select(c => new CivilLayoutDto(
                c.id, c.configuration_id, c.warehouse_type, c.source_file, c.civil_json, c.versionno,
                c.created_at, c.created_by, c.updated_at, c.updated_by


           )).ToList());



    }
    public async Task<int> CreateCivilLayoutAsync(CivilLayout civilLayout)
    {
        const string sql = @"
            INSERT INTO civil_layout (id, configuration_id, warehouse_type, source_file, civil_json, versionno, 
             created_at, created_by, updated_at, updated_by)
            VALUES (@Id, @ConfigurationId, @WarehouseType, @SourceFile, @CivilJson,
            COALESCE((SELECT MAX(cl.versionno) + 1 FROM civil_layout cl WHERE cl.configuration_id = @ConfigurationId),1),
            @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy)
            
            RETURNING versionno;";

        using var connection = _connectionFactory.CreateConnection();
        var versionNo= await connection.QuerySingleAsync<int>(sql, new
        {
            civilLayout.Id,
            civilLayout.ConfigurationId,
            civilLayout.WarehouseType,
            civilLayout.SourceFile,
            civilLayout.CivilJson,
            civilLayout.VersionNo,
            civilLayout.CreatedAt,
            civilLayout.CreatedBy,
            civilLayout.UpdatedAt,
            civilLayout.UpdatedBy
        });
        
        return versionNo;
    }
    public async Task<Guid> UpsertCivilLayoutAsync(CivilLayout civilLayout)
    {
        const string sql = @"
          Update civil_layout  SET warehouse_type = @WarehouseType,
                source_file = @SourceFile,
                civil_json = @CivilJson,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
                Where id=@Id;";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            civilLayout.WarehouseType,
            civilLayout.SourceFile,
            civilLayout.CivilJson,
            civilLayout.UpdatedAt,
            civilLayout.UpdatedBy,
            civilLayout.Id

        });

        return civilLayout.Id;
    }

    public async Task<RackLayout?> GetRackLayoutByVersionIdAsync(Guid versionId)
    {
        const string sql = @"
            SELECT id, civil_layout_id, configuration_version_id, rack_json, configuration_layout,
                   created_at, created_by, updated_at, updated_by
            FROM rack_layout
            WHERE configuration_version_id = @VersionId";

        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QueryFirstOrDefaultAsync<RackLayoutRow>(sql, new { VersionId = versionId });

        if (row == null) return null;

        return RackLayout.Rehydrate(
            row.id, row.civil_layout_id, row.configuration_version_id, row.rack_json,
            row.configuration_layout != null ? System.Text.Json.JsonDocument.Parse(row.configuration_layout) : null,
            row.created_at, row.created_by, row.updated_at, row.updated_by
        );
    }

    public async Task<Guid> CreateRackLayoutAsync(RackLayout rackLayout)
    {
        const string sql = @"
            INSERT INTO rack_layout (id, civil_layout_id, configuration_version_id, rack_json, configuration_layout,
                                    created_at, created_by, updated_at, updated_by)
            VALUES (@Id, @CivilLayoutId, @ConfigurationVersionId, @RackJson, @ConfigurationLayout::jsonb,
                    @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy)
            RETURNING id;";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            rackLayout.Id,
            rackLayout.CivilLayoutId,
            rackLayout.ConfigurationVersionId,
            rackLayout.RackJson,
            ConfigurationLayout = rackLayout.ConfigurationLayout?.RootElement.GetRawText(),
            rackLayout.CreatedAt,
            rackLayout.CreatedBy,
            rackLayout.UpdatedAt,
            rackLayout.UpdatedBy
        });

        return rackLayout.Id;
    }

    public async Task<Guid> UpdateRackLayoutAsync(RackLayout rackLayout)
    {
        const string sql = @"
          Update rack_layout  SET rack_json = @RackJson,
                configuration_layout = @ConfigurationLayout::jsonb,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
                Where id=@Id;";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            rackLayout.Id,
            rackLayout.RackJson,
            ConfigurationLayout = rackLayout.ConfigurationLayout?.RootElement.GetRawText(),
            rackLayout.UpdatedAt,
            rackLayout.UpdatedBy
        });

        return rackLayout.Id;
    }

    private class CivilLayoutRow
    {
        public Guid id { get; set; }
        public Guid configuration_id { get; set; }
        public Guid? warehouse_type { get; set; }
        public string? source_file { get; set; }
        public string? civil_json { get; set; }
        public int versionno { get; set; }
        public DateTime created_at { get; set; }
        public string? created_by { get; set; }
        public DateTime? updated_at { get; set; }
        public string? updated_by { get; set; }
    }

    private class RackLayoutRow
    {
        public Guid id { get; set; }
        public Guid civil_layout_id { get; set; }
        public Guid configuration_version_id { get; set; }
        public string? rack_json { get; set; }
        public string? configuration_layout { get; set; }
        public DateTime created_at { get; set; }
        public string? created_by { get; set; }
        public DateTime? updated_at { get; set; }
        public string? updated_by { get; set; }
    }

    private class ConfigurationRow
    {
        public Guid id { get; set; }
        public Guid enquiry_id { get; set; }
        public string name { get; set; } = default!;
        public string? description { get; set; }
        public bool is_active { get; set; }
        public bool is_primary { get; set; }
        public DateTime created_at { get; set; }
        public string? created_by { get; set; }
        public DateTime? updated_at { get; set; }
        public string? updated_by { get; set; }
    }
    private class ConfigurationVersionRow
    {
        public Guid id { get; set; }
        public Guid configuration_id { get; set; }
        public int version_number { get; set; }
        public string? description { get; set; }
        public bool is_current { get; set; }
        public bool is_locked { get; set; }
        public EnquiryStatus status { get; set; }
        public DateTime created_at { get; set; }
        public string? created_by { get; set; }
    }
}
