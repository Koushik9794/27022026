using ConfigurationService.Domain.Aggregates;
using ConfigurationService.Infrastructure.Persistence;
using Dapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;

namespace ConfigurationService.Infrastructure.Persistence.Repositories;

public class RackConfigurationRepository : IRackConfigurationRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public RackConfigurationRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task AddAsync(RackConfiguration config)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            INSERT INTO rack_configurations (
                id, name, configuration_layout, product_code, scope, enquiry_id, 
                is_approved_by_admin, approved_by, approved_on, is_active, 
                created_on, created_by, updated_on, updated_by
            ) VALUES (
                @Id, @Name, @ConfigurationLayout::jsonb, @ProductCode, @Scope, @EnquiryId, 
                @IsApprovedByAdmin, @ApprovedBy, @ApprovedOn, @IsActive, 
                @CreatedOn, @CreatedBy, @UpdatedOn, @UpdatedBy
            )";

        await connection.ExecuteAsync(sql, new
        {
            config.Id,
            config.Name,
            ConfigurationLayout = config.ConfigurationLayout.RootElement.ToString(),
            config.ProductCode,
            config.Scope,
            config.EnquiryId,
            config.IsApprovedByAdmin,
            config.ApprovedBy,
            config.ApprovedOn,
            config.IsActive,
            config.CreatedOn,
            config.CreatedBy,
            config.UpdatedOn,
            config.UpdatedBy
        });
    }

    public async Task<RackConfiguration?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QuerySingleOrDefaultAsync<dynamic>(
            "SELECT * FROM rack_configurations WHERE id = @Id", new { Id = id });

        if (row == null) return null;

        return RackConfiguration.Rehydrate(
            row.id,
            row.name,
            JsonDocument.Parse((string)row.configuration_layout),
            row.product_code,
            row.scope,
            row.enquiry_id,
            row.is_approved_by_admin,
            row.approved_by,
            row.approved_on,
            row.is_active,
            row.created_on,
            row.created_by,
            row.updated_on,
            row.updated_by
        );
    }

    public async Task UpdateAsync(RackConfiguration config)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = @"
            UPDATE rack_configurations SET
                name = @Name,
                configuration_layout = @ConfigurationLayout::jsonb,
                product_code = @ProductCode,
                scope = @Scope,
                enquiry_id = @EnquiryId,
                is_approved_by_admin = @IsApprovedByAdmin,
                approved_by = @ApprovedBy,
                approved_on = @ApprovedOn,
                is_active = @IsActive,
                updated_on = @UpdatedOn,
                updated_by = @UpdatedBy
            WHERE id = @Id";

        await connection.ExecuteAsync(sql, new
        {
            config.Id,
            config.Name,
            ConfigurationLayout = config.ConfigurationLayout.RootElement.ToString(),
            config.ProductCode,
            config.Scope,
            config.EnquiryId,
            config.IsApprovedByAdmin,
            config.ApprovedBy,
            config.ApprovedOn,
            config.IsActive,
            config.UpdatedOn,
            config.UpdatedBy
        });
    }

    public async Task DeleteAsync(Guid id, string? updatedBy)
    {
        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(
            "UPDATE rack_configurations SET is_active = false, updated_on = @UpdatedOn, updated_by = @UpdatedBy WHERE id = @Id",
            new { Id = id, UpdatedOn = DateTime.UtcNow, UpdatedBy = updatedBy });
    }

    public async Task<IEnumerable<RackConfiguration>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM rack_configurations";
        if (!includeInactive) sql += " WHERE is_active = true";
        
        var rows = await connection.QueryAsync<dynamic>(sql);
        var result = new List<RackConfiguration>();
        foreach (var row in rows)
        {
            result.Add(MapRow(row));
        }
        return result;
    }

    public async Task<IEnumerable<RackConfiguration>> GetByEnquiryIdAsync(Guid enquiryId, bool includeInactive = false)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM rack_configurations WHERE enquiry_id = @EnquiryId";
        if (!includeInactive) sql += " AND is_active = true";

        var rows = await connection.QueryAsync<dynamic>(sql, new { EnquiryId = enquiryId });
        var result = new List<RackConfiguration>();
        foreach (var row in rows)
        {
            result.Add(MapRow(row));
        }
        return result;
    }

    private RackConfiguration MapRow(dynamic row)
    {
        return RackConfiguration.Rehydrate(
            row.id,
            row.name,
            JsonDocument.Parse((string)row.configuration_layout),
            row.product_code,
            row.scope,
            row.enquiry_id,
            row.is_approved_by_admin,
            row.approved_by,
            row.approved_on,
            row.is_active,
            row.created_on,
            row.created_by,
            row.updated_on,
            row.updated_by
        );
    }
}
