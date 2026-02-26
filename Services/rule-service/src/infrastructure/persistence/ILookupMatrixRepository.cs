using System;
using System.Threading.Tasks;
using RuleService.Domain.Entities;

namespace RuleService.Infrastructure.Persistence
{
    public interface ILookupMatrixRepository
    {
        Task<LookupMatrix?> GetByNameAsync(string name);
        Task<Guid> SaveAsync(LookupMatrix matrix);
        
        /// <summary>
        /// Update a specific cell in the JSONB document atomically
        /// </summary>
        Task UpdateCellAsync(Guid matrixId, string[] path, object newValue);
        
        /// <summary>
        /// Fetch a specific node from the JSONB document using PostgreSQL path operators
        /// </summary>
        Task<string?> GetNodeByPathAsync(string matrixName, string[] path);

        /// <summary>
        /// Fetch metadata for all available matrices
        /// </summary>
        Task<List<LookupMatrix>> GetAllMetadataAsync();
    }
}
