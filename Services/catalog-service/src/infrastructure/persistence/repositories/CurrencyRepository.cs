using CatalogService.Domain.Aggregates;
using Dapper;
using CatalogService.Infrastructure.Persistence;

namespace CatalogService.Infrastructure.Persistence.Repositories;

public class CurrencyRepository : ICurrencyRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CurrencyRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Currency>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM currency";
        
        if (!includeInactive)
        {
            sql += " WHERE is_active = true AND is_delete = false";
        }
        else
        {
            sql += " WHERE is_delete = false";
        }
        
        sql += " ORDER BY currency_code";

        var rows = await connection.QueryAsync<CurrencyRow>(sql);

        return rows.Select(e => Currency.Rehydrate(
            e.id,
            e.currency_code,
            e.currency_name,
            e.currency_value,
            e.decimal_unit,
            e.is_active,
            e.is_delete,
            e.created_at,
            e.created_by,
            e.updated_by,
            e.updated_at
        )).ToList();
    }

    public async Task<Currency?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM currency WHERE id = @Id AND is_delete = false";

        var entity = await connection.QuerySingleOrDefaultAsync<CurrencyRow>(sql, new { Id = id });

        if (entity == null) return null;

        return Currency.Rehydrate(
            entity.id,
            entity.currency_code,
            entity.currency_name,
            entity.currency_value,
            entity.decimal_unit,
            entity.is_active,
            entity.is_delete,
            entity.created_at,
            entity.created_by,
            entity.updated_by,
            entity.updated_at
        );
    }

    public async Task<Currency?> GetByCodeAsync(string code)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM currency WHERE currency_code = @Code AND is_delete = false";

        var entity = await connection.QuerySingleOrDefaultAsync<CurrencyRow>(sql, new { Code = code });

        if (entity == null) return null;

        return Currency.Rehydrate(
            entity.id,
            entity.currency_code,
            entity.currency_name,
            entity.currency_value,
            entity.decimal_unit,
            entity.is_active,
            entity.is_delete,
            entity.created_at,
            entity.created_by,
            entity.updated_by,
            entity.updated_at
        );
    }

    public async Task<bool> ExistsByCodeAsync(string code)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT COUNT(1) FROM currency WHERE currency_code = @Code AND is_delete = false";
        return await connection.ExecuteScalarAsync<bool>(sql, new { Code = code });
    }

    public async Task CreateAsync(Currency currency)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO currency (id, currency_code, currency_name, currency_value, decimal_unit, is_active, is_delete, created_at, created_by, updated_at)
            VALUES (@Id, @CurrencyCode, @CurrencyName, @CurrencyValue, @DecimalUnit, @IsActive, @IsDelete, @CreatedAt, @CreatedBy, @UpdatedAt)";

        await connection.ExecuteAsync(sql, new
        {
            currency.Id,
            currency.CurrencyCode,
            currency.CurrencyName,
            currency.CurrencyValue,
            currency.DecimalUnit,
            currency.IsActive,
            currency.IsDelete,
            currency.CreatedAt,
            currency.CreatedBy,
            currency.UpdatedAt
        });
    }

    public async Task UpdateAsync(Currency currency)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE currency
            SET currency_name = @CurrencyName,
                currency_value = @CurrencyValue,
                decimal_unit = @DecimalUnit,
                is_active = @IsActive,
                is_delete = @IsDelete,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id";

        await connection.ExecuteAsync(sql, new
        {
            currency.Id,
            currency.CurrencyName,
            currency.CurrencyValue,
            currency.DecimalUnit,
            currency.IsActive,
            currency.IsDelete,
            currency.UpdatedAt,
            currency.UpdatedBy
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE currency SET is_delete = true, updated_at = NOW() WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    private sealed class CurrencyRow
    {
        public Guid id { get; set; }
        public string currency_code { get; set; } = default!;
        public string currency_name { get; set; } = default!;
        public string? currency_value { get; set; }
        public short decimal_unit { get; set; }
        public bool is_active { get; set; }
        public bool is_delete { get; set; }
        public DateTime created_at { get; set; }
        public string? created_by { get; set; }
        public string? updated_by { get; set; }
        public DateTime updated_at { get; set; }
    }
}
