using System.Data;
using Npgsql;

namespace ConfigurationService.Infrastructure.Persistence;

/// <summary>
/// PostgreSQL connection factory implementation.
/// </summary>
public class PostgresConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public PostgresConnectionFactory(string connectionString)
    {
        _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_connectionString);
    }
}
