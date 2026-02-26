using System.Text.Json;
using ConfigurationService.Domain.Aggregates;
using ConfigurationService.Domain.Enums;
using Dapper;

namespace ConfigurationService.Infrastructure.Persistence.Repositories;

/// <summary>
/// Dapper-based repository for Enquiry aggregate.
/// </summary>
public class EnquiryRepository : IEnquiryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public EnquiryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<Enquiry?> GetByIdAsync(Guid id)
    {
        const string sql = @"
            SELECT id, external_enquiry_id, name, description,enquiry_no,customername,customercontact,customeremail,product_group, ""Billing_details"",
            source, dealerid, status, version,is_deleted, created_at, created_by, updated_at, updated_by
            FROM enquiries WHERE id = @Id AND is_deleted = false";

        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QueryFirstOrDefaultAsync<EnquiryRow>(sql, new { Id = id });
        return row == null ? null : MapToEntity(row);
    }

    public async Task<Enquiry?> GetByExternalIdAsync(string externalEnquiryId)
    {
        const string sql = @"
            SELECT id, external_enquiry_id, name, description,enquiry_no, customername,customercontact,customeremail,product_group, ""Billing_details"",
            source, dealerid, status, version,is_deleted, created_at, created_by, updated_at, updated_by
            FROM enquiries WHERE external_enquiry_id = @ExternalEnquiryId AND is_deleted = false";

        using var connection = _connectionFactory.CreateConnection();
        var row = await connection.QueryFirstOrDefaultAsync<EnquiryRow>(sql, new { ExternalEnquiryId = externalEnquiryId });
        return row == null ? null : MapToEntity(row);
    }

    public async Task<IEnumerable<Enquiry>> GetAllAsync(bool includeDeleted = false)
    {
        var sql = @"
            SELECT id, external_enquiry_id, name, description,enquiry_no,customername,customercontact,customeremail,product_group, ""Billing_details"",
            source, dealerid, status, version, is_deleted, created_at, created_by, updated_at, updated_by
            FROM enquiries";

        if (!includeDeleted)
        {
            sql += " WHERE is_deleted = false";
        }

        sql += " ORDER BY created_at DESC";

        using var connection = _connectionFactory.CreateConnection();
        var rows = await connection.QueryAsync<EnquiryRow>(sql);
        return rows.Select(MapToEntity);
    }

    public async Task<Enquiry> CreateAsync(Enquiry enquiry)
    {
        const string sql = @"
            INSERT INTO enquiries (id, external_enquiry_id, name, description,enquiry_no,customername,customercontact,customeremail,product_group, ""Billing_details"",
            source, dealerid, status, version, is_deleted, created_at, created_by, updated_at, updated_by)
            VALUES (@Id, @ExternalEnquiryId, @Name, @Description,@EnquiryNo, @CustomerName,@CustomerContact,@CustomerMail,@ProductGroup,@BillingDetails,
            @Source, @DealerId, @Status, @Version,@IsDeleted, @CreatedAt, @CreatedBy, @UpdatedAt, @UpdatedBy)";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            enquiry.Id,
            enquiry.ExternalEnquiryId,
            enquiry.Name,
            enquiry.Description,
            enquiry.EnquiryNo,
            enquiry.CustomerName,
            enquiry.CustomerContact,
            enquiry.CustomerMail,
            enquiry.ProductGroup,
            enquiry.BillingDetails,
            enquiry.Source,
            enquiry.DealerId,
            Status = enquiry.Status.ToString(),
            enquiry.Version,
            enquiry.IsDeleted,
            enquiry.CreatedAt,
            enquiry.CreatedBy,
            enquiry.UpdatedAt,
            enquiry.UpdatedBy
        });

        return enquiry;
    }

    public async Task<Enquiry> UpdateAsync(Enquiry enquiry)
    {
        const string sql = @"
            UPDATE enquiries
            SET name = @Name,
                description = @Description,
                enquiry_no=@EnquiryNo,
                customername=@CustomerName,
                customercontact=@CustomerContact,
                customeremail=@CustomerMail,
                product_group=@ProductGroup,
                ""Billing_details""=@BillingDetails,
                source=@Source,
                dealerid=@DealerId,
                status = @Status,
                version = @Version,
                is_deleted = @IsDeleted,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new
        {
            enquiry.Id,
            enquiry.Name,
            enquiry.Description,
            enquiry.EnquiryNo,
            enquiry.CustomerName,
            enquiry.CustomerContact,
            enquiry.CustomerMail,
            enquiry.ProductGroup,
            enquiry.BillingDetails,
            enquiry.Source,
            enquiry.DealerId,
            Status = enquiry.Status.ToString(),
            enquiry.Version,
            enquiry.IsDeleted,
            enquiry.UpdatedAt,
            enquiry.UpdatedBy
        });

        return enquiry;
    }

    public async Task DeleteAsync(Guid id)
    {
        const string sql = @"
            UPDATE enquiries
            SET is_deleted = true, status = 'Archived', updated_at = @UpdatedAt
            WHERE id = @Id";

        using var connection = _connectionFactory.CreateConnection();
        await connection.ExecuteAsync(sql, new { Id = id, UpdatedAt = DateTime.UtcNow });
    }

    public async Task<bool> ExistsAsync(Guid id)
    {
        const string sql = "SELECT COUNT(1) FROM enquiries WHERE id = @Id AND is_deleted = false";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new { Id = id }) > 0;
    }

    public async Task<bool> ExternalIdExistsAsync(string externalEnquiryId)
    {
        const string sql = "SELECT COUNT(1) FROM enquiries WHERE external_enquiry_id = @ExternalEnquiryId";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new { ExternalEnquiryId = externalEnquiryId }) > 0;
    }

    public async Task<bool> ExistsEnquiryNoAsync(string? EnquiryNo)
    {
        const string sql = "SELECT COUNT(1) FROM enquiries WHERE enquiry_no = @EnquiryNo";
        using var connection = _connectionFactory.CreateConnection();
        return await connection.ExecuteScalarAsync<int>(sql, new { EnquiryNo = EnquiryNo }) > 0;
    }

    private static Enquiry MapToEntity(EnquiryRow row)
    {
        return Enquiry.Rehydrate(
            row.id,
            row.external_enquiry_id,
            row.name,
            row.description,
            row.enquiry_no,
            row.customername,
            row.customercontact,
            row.customeremail,
            row.product_group,
            row.Billing_details,
            row.source,
            row.dealerid,
            Enum.Parse<EnquiryStatus>(row.status),
            row.version,
            row.created_at,
            row.created_by,
            row.updated_at,
            row.updated_by,
            row.is_deleted
        );
    }

    private class EnquiryRow
    {
        public Guid id { get; set; }
        public string external_enquiry_id { get; set; } = default!;
        public string name { get; set; } = default!;
        public string? description { get; set; }
        public string? enquiry_no { get; set; }
        public string? customername { get; set; }
        public long? customercontact { get; set; }
        public string? customeremail { get; set; }
        public string? product_group { get; set; }
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
}
