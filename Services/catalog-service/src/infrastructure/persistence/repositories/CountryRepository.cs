using CatalogService.Domain.Aggregates;
using Dapper;
using CatalogService.Infrastructure.Persistence;

namespace CatalogService.Infrastructure.Persistence.Repositories;

public class CountryRepository : ICountryRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public CountryRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<Country>> GetAllAsync(bool includeInactive = false)
    {
        using var connection = _connectionFactory.CreateConnection();
        var sql = "SELECT * FROM country";
        
        if (!includeInactive)
        {
            sql += " WHERE is_active = true AND is_delete = false";
        }
        else
        {
            sql += " WHERE is_delete = false";
        }
        
        sql += " ORDER BY country_name";

        var rows = await connection.QueryAsync<CountryRow>(sql);

        return rows.Select(e => Country.Rehydrate(
            e.id,
            e.country_code,
            e.country_name,
            e.currency_code,
            e.is_active,
            e.is_delete,
            e.created_at,
            e.created_by,
            e.updated_by,
            e.updated_at
        )).ToList();
    }

    public async Task<Country?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM country WHERE id = @Id AND is_delete = false";

        var entity = await connection.QuerySingleOrDefaultAsync<CountryRow>(sql, new { Id = id });

        if (entity == null) return null;

        return Country.Rehydrate(
            entity.id,
            entity.country_code,
            entity.country_name,
            entity.currency_code,
            entity.is_active,
            entity.is_delete,
            entity.created_at,
            entity.created_by,
            entity.updated_by,
            entity.updated_at
        );
    }

    public async Task<Country?> GetByCodeAsync(string code)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM country WHERE country_code = @Code AND is_delete = false";

        var entity = await connection.QuerySingleOrDefaultAsync<CountryRow>(sql, new { Code = code });

        if (entity == null) return null;

        return Country.Rehydrate(
            entity.id,
            entity.country_code,
            entity.country_name,
            entity.currency_code,
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
        const string sql = "SELECT COUNT(1) FROM country WHERE country_code = @Code AND is_delete = false";
        return await connection.ExecuteScalarAsync<bool>(sql, new { Code = code });
    }

    public async Task CreateAsync(Country country)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO country (id, country_code, country_name, currency_code, is_active, is_delete, created_at, created_by, updated_at)
            VALUES (@Id, @CountryCode, @CountryName, @CurrencyCode, @IsActive, @IsDelete, @CreatedAt, @CreatedBy, @UpdatedAt)";

        await connection.ExecuteAsync(sql, new
        {
            country.Id,
            country.CountryCode,
            country.CountryName,
            country.CurrencyCode,
            country.IsActive,
            country.IsDelete,
            country.CreatedAt,
            country.CreatedBy,
            country.UpdatedAt
        });
    }

    public async Task UpdateAsync(Country country)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            UPDATE country
            SET country_name = @CountryName,
                currency_code = @CurrencyCode,
                is_active = @IsActive,
                is_delete = @IsDelete,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id";

        await connection.ExecuteAsync(sql, new
        {
            country.Id,
            country.CountryName,
            country.CurrencyCode,
            country.IsActive,
            country.IsDelete,
            country.UpdatedAt,
            country.UpdatedBy
        });
    }

    public async Task DeleteAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "UPDATE country SET is_delete = true, updated_at = NOW() WHERE id = @Id";
        await connection.ExecuteAsync(sql, new { Id = id });
    }

    private sealed class CountryRow
    {
        public Guid id { get; set; }
        public string country_code { get; set; } = default!;
        public string country_name { get; set; } = default!;
        public string currency_code { get; set; } = default!;
        public bool is_active { get; set; }
        public bool is_delete { get; set; }
        public DateTime created_at { get; set; }
        public string? created_by { get; set; }
        public string? updated_by { get; set; }
        public DateTime updated_at { get; set; }
    }
}
