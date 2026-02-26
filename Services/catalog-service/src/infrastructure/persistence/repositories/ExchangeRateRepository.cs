using CatalogService.Domain.Aggregates;
using Dapper;
using CatalogService.Infrastructure.Persistence;

namespace CatalogService.Infrastructure.Persistence.Repositories;

public class ExchangeRateRepository : IExchangeRateRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ExchangeRateRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ExchangeRate>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM exchange_currency";
        
        if (!includeInactive)
        {
            sql += " WHERE is_active = true AND is_delete = false";
        }
        else
        {
            sql += " WHERE is_delete = false";
        }
        
        sql += " ORDER BY valid_from DESC";

        var rows = await connection.QueryAsync<ExchangeRateRow>(sql);

        return rows.Select(e => ExchangeRate.Rehydrate(
            e.id,
            e.base_currency,
            e.quote_currency,
            e.rate,
            e.valid_from,
            e.valid_end,
            e.is_active,
            e.is_delete,
            e.created_at,
            e.created_by,
            e.updated_by,
            e.updated_at
        )).ToList();
    }

    public async Task<ExchangeRate?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM exchange_currency WHERE id = @Id AND is_delete = false";

        var entity = await connection.QuerySingleOrDefaultAsync<ExchangeRateRow>(sql, new { Id = id });

        if (entity == null) return null;

        return ExchangeRate.Rehydrate(
            entity.id,
            entity.base_currency,
            entity.quote_currency,
            entity.rate,
            entity.valid_from,
            entity.valid_end,
            entity.is_active,
            entity.is_delete,
            entity.created_at,
            entity.created_by,
            entity.updated_by,
            entity.updated_at
        );
    }

    public async Task<ExchangeRate?> GetLatestRateAsync(string baseCurrency, string quoteCurrency)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            SELECT * FROM exchange_currency 
            WHERE base_currency = @BaseCurrency 
              AND quote_currency = @QuoteCurrency 
              AND is_active = true 
              AND is_delete = false 
              AND valid_from <= NOW() 
              AND (valid_end IS NULL OR valid_end >= NOW()) 
            ORDER BY valid_from DESC 
            LIMIT 1";

        var entity = await connection.QuerySingleOrDefaultAsync<ExchangeRateRow>(sql, new 
        { 
            BaseCurrency = baseCurrency.ToUpperInvariant(), 
            QuoteCurrency = quoteCurrency.ToUpperInvariant() 
        });

        if (entity == null) return null;

        return ExchangeRate.Rehydrate(
            entity.id,
            entity.base_currency,
            entity.quote_currency,
            entity.rate,
            entity.valid_from,
            entity.valid_end,
            entity.is_active,
            entity.is_delete,
            entity.created_at,
            entity.created_by,
            entity.updated_by,
            entity.updated_at
        );
    }

    public async Task<bool> ExistsOverlappingAsync(string baseCurrency, string quoteCurrency, DateTime validFrom, DateTime? validEnd, Guid? excludeId = null)
    {
        using var connection = _connectionFactory.CreateConnection();
        // Standard overlap check: StartA <= EndB AND StartB <= EndA
        // We use COALESCE with a far future date for NULL valid_end
        const string sql = @"
            SELECT COUNT(1) 
            FROM exchange_currency 
            WHERE base_currency = @BaseCurrency 
              AND quote_currency = @QuoteCurrency 
              AND is_delete = false 
              AND (@ExcludeId IS NULL OR id != @ExcludeId)
              AND valid_from <= COALESCE(@ValidEnd, '9999-12-31')
              AND COALESCE(valid_end, '9999-12-31') >= @ValidFrom";

        return await connection.ExecuteScalarAsync<bool>(sql, new
        {
            BaseCurrency = baseCurrency.ToUpperInvariant(),
            QuoteCurrency = quoteCurrency.ToUpperInvariant(),
            ValidFrom = validFrom.Date,
            ValidEnd = validEnd?.Date,
            ExcludeId = excludeId
        });
    }

    public async Task CreateAsync(ExchangeRate exchangeRate)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO exchange_currency (id, base_currency, quote_currency, rate, valid_from, valid_end, is_active, is_delete, created_at, created_by, updated_at)
            VALUES (@Id, @BaseCurrency, @QuoteCurrency, @Rate, @ValidFrom, @ValidEnd, @IsActive, @IsDelete, @CreatedAt, @CreatedBy, @UpdatedAt)";

        await connection.ExecuteAsync(sql, new
        {
            exchangeRate.Id,
            exchangeRate.BaseCurrency,
            exchangeRate.QuoteCurrency,
            exchangeRate.Rate,
            exchangeRate.ValidFrom,
            exchangeRate.ValidEnd,
            exchangeRate.IsActive,
            exchangeRate.IsDelete,
            exchangeRate.CreatedAt,
            exchangeRate.CreatedBy,
            exchangeRate.UpdatedAt
        });
    }

    public async Task UpdateAsync(ExchangeRate exchangeRate)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE exchange_currency
            SET rate = @Rate,
                valid_from = @ValidFrom,
                valid_end = @ValidEnd,
                is_active = @IsActive,
                is_delete = @IsDelete,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id";

        await connection.ExecuteAsync(sql, new
        {
            exchangeRate.Id,
            exchangeRate.Rate,
            exchangeRate.ValidFrom,
            exchangeRate.ValidEnd,
            exchangeRate.IsActive,
            exchangeRate.IsDelete,
            exchangeRate.UpdatedAt,
            exchangeRate.UpdatedBy
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE exchange_currency SET is_delete = true, updated_at = NOW() WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    private sealed class ExchangeRateRow
    {
        public Guid id { get; set; }
        public string base_currency { get; set; } = default!;
        public string quote_currency { get; set; } = default!;
        public decimal rate { get; set; }
        public DateTime valid_from { get; set; }
        public DateTime? valid_end { get; set; }
        public bool is_active { get; set; }
        public bool is_delete { get; set; }
        public DateTime created_at { get; set; }
        public string? created_by { get; set; }
        public string? updated_by { get; set; }
        public DateTime updated_at { get; set; }
    }
}
