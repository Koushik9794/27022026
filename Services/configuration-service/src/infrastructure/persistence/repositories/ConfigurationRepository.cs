using ConfigurationService.Application.Dtos;
using ConfigurationService.Domain.Aggregates;
using ConfigurationService.Domain.Enums;
using Dapper;
using Spectre.Console;

namespace ConfigurationService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Dapper-based repository for Configuration aggregate.
/// Loads Configuration with its child ConfigurationVersions.
/// </summary>
public class ConfigurationRepository : IConfigurationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ConfigurationRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Configuration?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT id, enquiry_id, name, description,
                   is_active, is_primary, created_at, created_by, updated_at, updated_by
            FROM configurations
            WHERE id = @Id;

            SELECT id, configuration_id, version_number, description, is_current, created_at, created_by
            FROM configuration_versions
            WHERE configuration_id = @Id
            ORDER BY version_number;

            SELECT id,configuration_id,warehouse_type, Source_file,civil_json,versionno,created_at,created_by,updated_at,updated_by 
            FROM civil_Layout WHERE configuration_id=@Id;

            SELECT id,configuration_version_id,civil_layout_id,rack_json,configuration_layout,created_at,created_by,updated_at,updated_by  
            FROM rack_layout WHERE configuration_version_id IN (SELECT id FROM configuration_versions WHERE configuration_id = @Id);

            SELECT id, configuration_version_id, floor_id, name, description, product_group,
                   design_data, last_saved_at, is_active, created_at, created_by, updated_at, updated_by
            FROM storage_configurations
            WHERE configuration_version_id IN (SELECT id FROM configuration_versions WHERE configuration_id = @Id);

            SELECT id, configuration_version_id, mhe_type_id, name, description,
                   attributes,is_active, created_at, created_by, updated_at, updated_by
            FROM mhe_configs
            WHERE configuration_version_id IN (SELECT id FROM configuration_versions WHERE configuration_id = @Id);";

        using var connection = _connectionFactory.CreateConnection();
        using var multi = await connection.QueryMultipleAsync(sql, new { Id = id });

        var configRow = await multi.ReadFirstOrDefaultAsync<ConfigurationRow>();
        if (configRow == null) return null;

        var versionRows = (await multi.ReadAsync<ConfigurationVersionRow>()).ToList();
        var civilrows= (await multi.ReadAsync<CivilLayoutRow>()).ToList();
        var rackrows= (await multi.ReadAsync<RackLayoutRow>()).ToList();

        var storageRows = (await multi.ReadAsync<StorageConfigurationRow>()).ToList();
        var mheRows = (await multi.ReadAsync<MheConfigRow>()).ToList();

        var civilLayouts = civilrows.Select(row => CivilLayout.Rehydrate(
            row.id, row.configuration_id, row.warehouse_type, row.source_file,
            row.civil_json,row.versionno, row.created_at, row.created_by, row.updated_at, row.updated_by
        )).ToList();

        var rackLayouts = rackrows.Select(row => RackLayout.Rehydrate(
            row.id, row.civil_layout_id, row.configuration_version_id,
            row.rack_json, 
            row.configuration_layout != null ? System.Text.Json.JsonDocument.Parse(row.configuration_layout) : null,
            row.created_at, row.created_by, row.updated_at, row.updated_by
        )).ToList();

        var storageConfigs = storageRows.Select(row => StorageConfiguration.Rehydrate(
            row.id, row.configuration_version_id, row.floor_id,
            row.name, row.description, row.product_group,
            row.design_data != null ? System.Text.Json.JsonDocument.Parse(row.design_data) : null,
            row.last_saved_at, row.is_active, row.created_at, row.created_by, row.updated_at, row.updated_by
        )).ToList();

        var mheConfigs = mheRows.Select(row => MheConfig.Rehydrate(
            row.id, row.configuration_version_id, row.mhe_type_id,
            row.name, row.description,
            row.attributes != null ? System.Text.Json.JsonDocument.Parse(row.attributes) : null,
            row.is_active, row.created_at, row.created_by, row.updated_at, row.updated_by
        )).ToList();

       

        var versions = versionRows.Select(v => ConfigurationVersion.Rehydrate(
            v.id, v.configuration_id, v.version_number, v.description, v.is_current,v.is_locked,v.Status, v.created_at, v.created_by,
            storageConfigurations: storageConfigs.Where(s => s.ConfigurationVersionId == v.id),
            mheConfigs: mheConfigs.Where(m => m.ConfigurationVersionId == v.id)
        ));

        return Configuration.Rehydrate(
            configRow.id, configRow.enquiry_id, configRow.name, configRow.description,
            configRow.is_active, configRow.is_primary, configRow.created_at, configRow.created_by, configRow.updated_at, configRow.updated_by,
            versions, civilLayouts, rackLayouts
        );
    }

    public async Task<IEnumerable<Configuration>> GetByEnquiryIdAsync(Guid enquiryId, bool includeInactive = false)
    {
        var sql = @"
            SELECT id, enquiry_id, name, description,
                   is_active, is_primary, created_at, created_by, updated_at, updated_by
            FROM configurations
            WHERE enquiry_id = @EnquiryId";

        if (!includeInactive)
        {
            sql += " AND is_active = true";
        }

        sql += " ORDER BY is_primary DESC, created_at";

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<ConfigurationRow>(sql, new { EnquiryId = enquiryId });

        // For listing, we don't load versions - they can be loaded on demand
        return rows.Select(row => Configuration.Rehydrate(
            row.id, row.enquiry_id, row.name, row.description,
            row.is_active, row.is_primary, row.created_at, row.created_by, row.updated_at, row.updated_by
        ));
    }
    public async Task<EnquiryDto?> GetListByEnquiryIdAsync(Guid enquiryId, bool includeInactive = false)
    {
        var sql = @"
            SELECT id, external_enquiry_id, name, description,enquiry_no,customername,customercontact,customeremail,product_group, ""Billing_details"",
            source, dealerid, status, version,is_deleted, created_at, created_by, updated_at, updated_by
            FROM enquiries WHERE id= @EnquiryId;

            SELECT id, enquiry_id, name, description,
            is_active, is_primary, created_at, created_by, updated_at, updated_by
            FROM configurations
            WHERE enquiry_id = @EnquiryId";

        if (!includeInactive)
        {
            sql += " AND is_active = true";
        }

        sql += " ORDER BY is_primary DESC, created_at";

        using var connection = _connectionFactory.CreateConnection();
        var args = new { EnquiryId = enquiryId };
        using var multi = await connection.QueryMultipleAsync(sql, args);

        var enquiry = await multi.ReadSingleOrDefaultAsync<EnquiryRow>();
        var configs = (await multi.ReadAsync<ConfigurationRow>()).ToList();


        return new EnquiryDto(
               Id: enquiry.id,
              ExternalEnquiryId: enquiry.external_enquiry_id,
              Name: enquiry.name,
              Description: enquiry.description,
              EnquiryNo: enquiry.enquiry_no,
              CustomerName: enquiry.customername,
              CustomerContact: enquiry.customercontact,
              CustomerMail: enquiry.customeremail,
              ProductGroup: enquiry.product_group,
              BillingDetails: enquiry.Billing_details,
              Source: enquiry.source,
              DealerId: enquiry.dealerid,
              Status: enquiry.status,
              Version: enquiry.version,
              CreatedAt: enquiry.created_at,
              CreatedBy: enquiry.created_by,
                Configurations: configs.Select(r => new ConfigurationDto(
                    Id: r.id,
                    EnquiryId: r.enquiry_id,
                    Name: r.name,
                    Description: r.description,
                    IsActive: r.is_active,
                    IsPrimary: r.is_primary,
                    CreatedAt: r.created_at,
                    CreatedBy: r.created_by
                )).ToList()

                        );

    }
    public async Task<Configuration?> GetPrimaryByEnquiryIdAsync(Guid enquiryId)
    {
        const string sql = @"
            SELECT id, enquiry_id, name, description,
                   is_active, is_primary, created_at, created_by, updated_at, updated_by
            FROM configurations
            WHERE enquiry_id = @EnquiryId AND is_primary = true AND is_active = true";

        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QueryFirstOrDefaultAsync<ConfigurationRow>(sql, new { EnquiryId = enquiryId });
        return row == null ? null : Configuration.Rehydrate(
            row.id, row.enquiry_id, row.name, row.description,
            row.is_active, row.is_primary, row.created_at, row.created_by, row.updated_at, row.updated_by
        );
    }

    public async Task<Guid?> SetversionLockAsync(Guid ConfigId, int version, bool isLocked)
    {
        const string sql = @"
            Update configuration_versions
            Set is_locked=@isLocked
            WHERE configuration_id = @ConfigId AND version_number = @version";

        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QueryFirstOrDefaultAsync<Guid>(sql, new {  ConfigId, version, isLocked });
        return ConfigId;
    }

    public async Task<Configuration> CreateAsync(Configuration configuration)
    {
        const string configSql = @"
            INSERT INTO configurations (id, enquiry_id, name, description,
                                        is_active, is_primary, created_at, created_by, updated_at, updated_by)
            VALUES (@Id, @EnquiryId, @Name, @Description,
                    @IsActive, @IsPrimary, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy)";

        const string versionSql = @"
            INSERT INTO configuration_versions (id, configuration_id, version_number, description, is_current, created_at, created_by)
            VALUES (@Id, @ConfigurationId, @VersionNumber, @Description, @IsCurrent, @CreatedAt, @CreatedBy)";

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            await connection.ExecuteAsync(configSql, new
            {
                configuration.Id,
                configuration.EnquiryId,
                configuration.Name,
                configuration.Description,
                configuration.IsActive,
                configuration.IsPrimary,
                configuration.CreatedAt,
                configuration.CreatedBy,
                configuration.UpdatedAt,
                configuration.UpdatedBy
            }, transaction);

            foreach (var version in configuration.Versions)
            {
                await connection.ExecuteAsync(versionSql, new
                {
                    version.Id,
                    version.ConfigurationId,
                    version.VersionNumber,
                    version.Description,
                    version.IsCurrent,
                    version.CreatedAt,
                    version.CreatedBy
                }, transaction);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        return configuration;
    }

    public async Task<Configuration> UpdateAsync(Configuration configuration)
    {
        const string configSql = @"
            UPDATE configurations
            SET name = @Name,
                description = @Description,
                is_active = @IsActive,
                is_primary = @IsPrimary,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id";

        const string versionUpsertSql = @"
            INSERT INTO configuration_versions (id, configuration_id, version_number, description, is_current, created_at, created_by)
            VALUES (@Id, @ConfigurationId, @VersionNumber, @Description, @IsCurrent, @CreatedAt, @CreatedBy)
            ON CONFLICT (configuration_id, version_number) DO UPDATE
            SET is_current = @IsCurrent";

        using var connection = _connectionFactory.CreateConnection();
        connection.Open();
        using var transaction = connection.BeginTransaction();

        try
        {
            await connection.ExecuteAsync(configSql, new
            {
                configuration.Id,
                configuration.Name,
                configuration.Description,
                configuration.IsActive,
                configuration.IsPrimary,
                configuration.UpdatedAt,
                configuration.UpdatedBy
            }, transaction);

            foreach (var version in configuration.Versions)
            {
                await connection.ExecuteAsync(versionUpsertSql, new
                {
                    version.Id,
                    version.ConfigurationId,
                    version.VersionNumber,
                    version.Description,
                    version.IsCurrent,
                    version.CreatedAt,
                    version.CreatedBy
                }, transaction);
            }

            transaction.Commit();
        }
        catch
        {
            transaction.Rollback();
            throw;
        }

        return configuration;
    }

    public async Task DeleteAsync(Guid id)
    {
        const string sql = @"
            UPDATE configurations
            SET is_active = false, updated_at = @UpdatedAt
            WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        const string sql = "SELECT COUNT(1) FROM configurations WHERE id = @Id AND is_active = true";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new { Id = id }) > 0;
    }

    public async Task ClearPrimaryForEnquiryAsync(Guid enquiryId)
    {
        const string sql = @"
            UPDATE configurations
            SET is_primary = false, updated_at = @UpdatedAt
            WHERE enquiry_id = @EnquiryId";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { EnquiryId = enquiryId, UpdatedAt = DateTime.UtcNow });
    }

    // ============ Storage Configuration Methods ============

    public async Task<StorageConfiguration?> GetStorageConfigurationByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT id, configuration_version_id, floor_id, name, description, product_group,
                   design_data, last_saved_at, is_active, created_at, created_by, updated_at, updated_by
            FROM storage_configurations
            WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QueryFirstOrDefaultAsync<StorageConfigurationRow>(sql, new { Id = id });
        if (row == null) return null;

        return StorageConfiguration.Rehydrate(
            row.id, row.configuration_version_id, row.floor_id,
            row.name, row.description, row.product_group,
            row.design_data != null ? System.Text.Json.JsonDocument.Parse(row.design_data) : null,
            row.last_saved_at, row.is_active, row.created_at, row.created_by, row.updated_at, row.updated_by
        );
    }

    public async Task UpdateStorageConfigurationAsync(StorageConfiguration storageConfig)
    {
        const string sql = @"
            UPDATE storage_configurations
            SET design_data = @DesignData::jsonb,
                last_saved_at = @LastSavedAt,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            storageConfig.Id,
            DesignData = storageConfig.DesignData?.RootElement.GetRawText(),
            storageConfig.LastSavedAt,
            UpdatedAt = DateTime.UtcNow,
            storageConfig.UpdatedBy
        });
    }

    public async Task AddStorageConfigurationAsync(StorageConfiguration storageConfig)
    {
        const string sql = @"
            INSERT INTO storage_configurations (
                id, configuration_version_id, floor_id, name, description, product_group,
                design_data, last_saved_at, is_active, created_at, created_by
            ) VALUES (
                @Id, @ConfigurationVersionId, @FloorId, @Name, @Description, @ProductGroup,
                @DesignData::jsonb, @LastSavedAt, @IsActive, @CreatedAt, @CreatedBy
            )";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            storageConfig.Id,
            storageConfig.ConfigurationVersionId,
            storageConfig.FloorId,
            storageConfig.Name,
            storageConfig.Description,
            storageConfig.ProductGroup,
            DesignData = storageConfig.DesignData?.RootElement.GetRawText(),
            storageConfig.LastSavedAt,
            storageConfig.IsActive,
            storageConfig.CreatedAt,
            storageConfig.CreatedBy
        });
    }

    // ============ MHE Configuration Methods ============

    public async Task<MheConfig?> GetMheConfigByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT id, configuration_version_id, mhe_type_id, name, description,
                   attributes,
                   is_active, created_at, created_by, updated_at, updated_by
            FROM mhe_configs
            WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QueryFirstOrDefaultAsync<MheConfigRow>(sql, new { Id = id });
        if (row == null) return null;

        return MheConfig.Rehydrate(
            row.id, row.configuration_version_id, row.mhe_type_id,
            row.name, row.description,
            row.attributes != null ? System.Text.Json.JsonDocument.Parse(row.attributes) : null,
            row.is_active, row.created_at, row.created_by, row.updated_at, row.updated_by
        );
    }

    public async Task UpdateMheConfigAsync(MheConfig mheConfig)
    {
        const string sql = @"
            UPDATE mhe_configs
            SET name = @Name,
                mhe_type_id = @MheTypeId,
                description = @Description,
                attributes = @Attributes::jsonb,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            mheConfig.Id,
            mheConfig.Name,
            mheConfig.MheTypeId,
            mheConfig.Description,
            Attributes = mheConfig.Attributes?.RootElement.GetRawText(),
            UpdatedAt = DateTime.UtcNow,
            mheConfig.UpdatedBy
        });
    }

    public async Task AddMheConfigAsync(MheConfig mheConfig)
    {
        const string sql = @"
            INSERT INTO mhe_configs (
                id, configuration_version_id, mhe_type_id, name, description,
                attributes, is_active, created_at, created_by
            ) VALUES (
                @Id, @ConfigurationVersionId, @MheTypeId, @Name, @Description,
                @Attributes::jsonb, @IsActive, @CreatedAt, @CreatedBy
            )";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            mheConfig.Id,
            mheConfig.ConfigurationVersionId,
            mheConfig.MheTypeId,
            mheConfig.Name,
            mheConfig.Description,
            Attributes = mheConfig.Attributes?.RootElement.GetRawText(),
            mheConfig.IsActive,
            mheConfig.CreatedAt,
            mheConfig.CreatedBy
        });
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

    private class EnquiryRow
    {
        public Guid id { get; set; }
        public string external_enquiry_id { get; set; } = default!;
        public string name { get; set; } = default!;
        public string? description { get; set; }
        public string? enquiry_no { get; private set; }
        public string? customername { get; private set; }
        public long? customercontact { get; private set; }
        public string? customeremail { get; private set; }
        public string? product_group { get; private set; }
        public string? Billing_details { get; set; }
        public string? source { get; set; }
        public Guid? dealerid { get; set; }
        public string status { get; set; } = default!;
        public int version { get; set; }
        public bool is_deleted { get; set; }
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
        public EnquiryStatus Status { get; set; }
    public DateTime created_at { get; set; }
        public string? created_by { get; set; }
    }

    private class StorageConfigurationRow
    {
        public Guid id { get; set; }
        public Guid configuration_version_id { get; set; }
        public Guid? floor_id { get; set; }
        public string name { get; set; } = default!;
        public string? description { get; set; }
        public string product_group { get; set; } = default!;
        public string? design_data { get; set; }
        public DateTime? last_saved_at { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
        public string? created_by { get; set; }
        public DateTime? updated_at { get; set; }
        public string? updated_by { get; set; }
    }

    private class MheConfigRow
    {
        public Guid id { get; set; }
        public Guid configuration_version_id { get; set; }
        public Guid? mhe_type_id { get; set; }
        public string name { get; set; } = default!;
        public string? description { get; set; }

        public string? attributes { get; set; }
        public bool is_active { get; set; }
        public DateTime created_at { get; set; }
        public string? created_by { get; set; }
        public DateTime? updated_at { get; set; }
        public string? updated_by { get; set; }
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
}
