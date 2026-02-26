using Npgsql;

namespace AdminService.Infrastructure.Persistence
{
    public static class DatabaseHelper
    {
        public static void EnsureDatabase(string connectionString)
        {
            var builder = new NpgsqlConnectionStringBuilder(connectionString);
            var targetDatabase = builder.Database;

            // Connect to 'postgres' database to check/create the target database
            builder.Database = "postgres";
            var masterConnectionString = builder.ToString();

            using var connection = new NpgsqlConnection(masterConnectionString);
            connection.Open();

            using var command = connection.CreateCommand();
            command.CommandText = $"SELECT 1 FROM pg_database WHERE datname = '{targetDatabase}'";
            var exists = command.ExecuteScalar() != null;

            if (!exists)
            {
                command.CommandText = $"CREATE DATABASE \"{targetDatabase}\"";
                command.ExecuteNonQuery();
            }
        }
    }
}
