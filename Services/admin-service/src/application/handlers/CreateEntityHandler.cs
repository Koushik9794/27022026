using System;
using System.Threading;
using System.Threading.Tasks;
using AdminService.Application.Commands;
using AdminService.Infrastructure.Persistence;

using MediatR;

namespace AdminService.Application.Handlers
{
    public sealed class CreateEntityHandler : IRequestHandler<CreateEntityCommand, string>
    {
        private readonly IEntityRepository _repo;

        public CreateEntityHandler(IEntityRepository repo)
        {
            _repo = repo;
        }

        public async Task<string> Handle(CreateEntityCommand cmd, CancellationToken ct)
        {
            var name        = cmd.EntityName.Trim();
            var createdBy   = cmd.CreatedBy.Trim();
            var sourceTable = cmd.SourceTable.Trim();
            var pkColumn    = cmd.PkColumn.Trim();
            var labelColumn = cmd.LabelColumn.Trim();
            var description = cmd.Description;

            // Basic guards
            if (name.Length == 0)        throw new ArgumentException("EntityName is required.", nameof(cmd.EntityName));
            if (createdBy.Length == 0)   throw new ArgumentException("CreatedBy is required.", nameof(cmd.CreatedBy));
            if (sourceTable.Length == 0) throw new ArgumentException("SourceTable is required.", nameof(cmd.SourceTable));
            if (pkColumn.Length == 0)    throw new ArgumentException("PkColumn is required.", nameof(cmd.PkColumn));
            if (labelColumn.Length == 0) throw new ArgumentException("LabelColumn is required.", nameof(cmd.LabelColumn));

            // Idempotent existence check
            var exists = await _repo.ExistsAsync(name, ct);
            if (exists) throw new InvalidOperationException($"Entity '{name}' already exists.");

            await _repo.CreateAsync(name, description, createdBy, DateTime.UtcNow, sourceTable, pkColumn, labelColumn, ct);
            return name;
        }
    }
}
