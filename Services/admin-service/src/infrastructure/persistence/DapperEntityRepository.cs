using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper;

using AdminService.Domain.Entities;
using AdminService.Infrastructure.Dapper;

namespace AdminService.Infrastructure.Persistence
{
    public sealed class DapperEntityRepository : IEntityRepository
    {
        private readonly IDbConnectionFactory _factory;
        public DapperEntityRepository(IDbConnectionFactory factory) => _factory = factory;

        public async Task<IEnumerable<AppEntity>> GetAllAsync(CancellationToken ct)
        {
            const string sql = @"
SELECT id, source_table, pk_column, label_column, entity_name, is_active
FROM public.app_entities
WHERE is_active = true;";

            using var conn = _factory.CreateConnection();
            var rows = await conn.QueryAsync(new CommandDefinition(sql, cancellationToken: ct));

            return rows.Select(row => new AppEntity
            {
                Id          = (Guid)row.id,
                SourceTable = (string)row.source_table,
                PkColumn    = (string)row.pk_column,
                LabelColumn = (string)row.label_column,
                EntityName  = (string)row.entity_name,
                IsActive    = (bool)row.is_active
            });
        }

        // NEW: check if an entity exists by name
        public async Task<bool> ExistsAsync(string entityName, CancellationToken ct)
        {
            const string sql = @"SELECT 1 FROM public.app_entities WHERE entity_name = @Name LIMIT 1;";
            using var conn = _factory.CreateConnection();
            var result = await conn.ExecuteScalarAsync<int?>(
                new CommandDefinition(sql, new { Name = entityName }, cancellationToken: ct));
            return result.HasValue;
        }

        // NEW: create an entity row
        public async Task CreateAsync(
            string entityName,
            string? description,
            string createdBy,
            DateTime nowUtc,
            string sourceTable,
            string pkColumn,
            string labelColumn,
            CancellationToken ct)
        {
            const string sql = @"
INSERT INTO public.app_entities
(id, entity_name, description, is_active, is_deleted, created_by, created_at, modified_by, modified_at, source_table, pk_column, label_column)
VALUES (@Id, @Name, @Description, TRUE, FALSE, @CreatedBy, @CreatedAt, NULL, NULL, @SourceTable, @PkColumn, @LabelColumn);";

            using var conn = _factory.CreateConnection();
            await conn.ExecuteAsync(new CommandDefinition(sql, new
            {
                Id          = Guid.NewGuid(),
                Name        = entityName,
                Description = description,
                CreatedBy   = createdBy,
                CreatedAt   = nowUtc,
                SourceTable = sourceTable,
                PkColumn    = pkColumn,
                LabelColumn = labelColumn
            }, cancellationToken: ct));
        }
    }
}

