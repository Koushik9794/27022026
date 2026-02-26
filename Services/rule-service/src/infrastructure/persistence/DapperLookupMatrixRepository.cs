using System;
using System.Data;
using System.Threading.Tasks;
using Dapper;
using Npgsql;
using RuleService.Domain.Entities;
using Microsoft.Extensions.Configuration;

namespace RuleService.Infrastructure.Persistence
{
    public class DapperLookupMatrixRepository : ILookupMatrixRepository
    {
        private readonly string _connectionString;

        public DapperLookupMatrixRepository(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection") ?? throw new ArgumentNullException("ConnectionStrings:DefaultConnection");
        }

        // Constructor for testing
        public DapperLookupMatrixRepository(string connectionString)
        {
            _connectionString = connectionString ?? throw new ArgumentNullException(nameof(connectionString));
        }

        private IDbConnection CreateConnection() => new NpgsqlConnection(_connectionString);

        public async Task<LookupMatrix?> GetByNameAsync(string name)
        {
            const string sql = @"
                SELECT id, name, category, data as DataJson, metadata as MetadataJson, version, created_at, updated_at
                FROM lookup_matrices
                WHERE name = @name";

            using var connection = CreateConnection();
            return await connection.QueryFirstOrDefaultAsync<LookupMatrix>(sql, new { name });
        }

        public async Task<Guid> SaveAsync(LookupMatrix matrix)
        {
            const string sql = @"
                INSERT INTO lookup_matrices (id, name, category, data, metadata, version, created_at, updated_at)
                VALUES (@Id, @Name, @Category, @DataJson::jsonb, @MetadataJson::jsonb, @Version, @CreatedAt, @UpdatedAt)
                ON CONFLICT (name) DO UPDATE SET
                    data = EXCLUDED.data,
                    metadata = EXCLUDED.metadata,
                    version = lookup_matrices.version + 1,
                    updated_at = EXCLUDED.updated_at
                RETURNING id";

            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<Guid>(sql, matrix);
        }

        public async Task UpdateCellAsync(Guid matrixId, string[] path, object newValue)
        {
            // PostgreSQL syntax: jsonb_set(target, path, new_value)
            // path is text[] e.g. '{uprights, ST20, 2700, HEM_80}'
            
            const string sql = @"
                UPDATE lookup_matrices
                SET data = jsonb_set(data, @Path::text[], @Value::jsonb),
                    version = version + 1,
                    updated_at = NOW()
                WHERE id = @Id";

            using var connection = CreateConnection();
            var jsonValue = System.Text.Json.JsonSerializer.Serialize(newValue);
            await connection.ExecuteAsync(sql, new { Id = matrixId, Path = path, Value = jsonValue });
        }

        public async Task<string?> GetNodeByPathAsync(string matrixName, string[] path)
        {
            // PostgreSQL syntax: data #> '{path,to,node}'
            const string sql = @"
                SELECT data #> @Path::text[]
                FROM lookup_matrices
                WHERE name = @Name";

            using var connection = CreateConnection();
            return await connection.ExecuteScalarAsync<string>(sql, new { Name = matrixName, Path = path });
        }

        public async Task<List<LookupMatrix>> GetAllMetadataAsync()
        {
            const string sql = @"
                SELECT id, name, category, version, created_at, updated_at
                FROM lookup_matrices
                ORDER BY name";

            using var connection = CreateConnection();
            return (await connection.QueryAsync<LookupMatrix>(sql)).ToList();
        }
    }
}
