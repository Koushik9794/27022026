using System.Data;

namespace ConfigurationService.Infrastructure.Persistence;

/// <summary>
/// Factory for creating database connections.
/// </summary>
public interface IDbConnectionFactory
{
    /// <summary>
    /// Creates a new database connection.
    /// </summary>
    IDbConnection CreateConnection();
}
