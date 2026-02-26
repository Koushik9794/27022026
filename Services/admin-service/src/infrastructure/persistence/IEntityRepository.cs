using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdminService.Domain.Entities;

namespace AdminService.Infrastructure.Persistence
{
    public interface IEntityRepository
    {
        Task<IEnumerable<AppEntity>> GetAllAsync(CancellationToken ct);

        // NEW: for POST /entities flow
        Task<bool> ExistsAsync(string entityName, CancellationToken ct);
        Task CreateAsync(
            string entityName,
            string? description,
            string createdBy,
            DateTime nowUtc,
            string sourceTable,
            string pkColumn,
            string labelColumn,
            CancellationToken ct);
    }
}

