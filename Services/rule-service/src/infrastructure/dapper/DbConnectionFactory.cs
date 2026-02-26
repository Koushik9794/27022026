using Npgsql;
using System.Data;

namespace RuleService.Infrastructure.Dapper
{
    /// <summary>
    /// Connection factory for database access
    /// </summary>
    public interface IDbConnectionFactory
    {
        IDbConnection CreateConnection();
    }

    /// <summary>
    /// PostgreSQL connection factory implementation
    /// </summary>
    public class PostgreSqlConnectionFactory : IDbConnectionFactory
    {
        private readonly string _connectionString;

        public PostgreSqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        public IDbConnection CreateConnection()
        {
            return new NpgsqlConnection(_connectionString);
        }
    }
}
