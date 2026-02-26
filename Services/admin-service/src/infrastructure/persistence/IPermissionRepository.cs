// src/application/infrastructure/persistence/IPermissionRepository.cs
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdminService.Domain.Aggregates;

namespace AdminService.Infrastructure.Persistence
{
    public interface IPermissionRepository
    {
        Task<Guid> CreateAsync(Permission permission, CancellationToken ct);
        Task<Permission?> GetByIdAsync(Guid id, CancellationToken ct);
        Task<IEnumerable<Permission>> GetAllAsync(string? module, string? entityName, CancellationToken ct);
        Task UpdateAsync(Permission permission, CancellationToken ct);
        Task SoftDeleteAsync(Guid id, string modifiedBy, DateTime nowUtc, CancellationToken ct);
    }
}
