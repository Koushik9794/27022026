using AdminService.Domain.Aggregates;

namespace AdminService.Infrastructure.Persistence
{
    /// <summary>
    /// Repository interface for Dealer aggregate
    /// </summary>
    public interface IDealerRepository
    {
        Task<Dealer?> GetByIdAsync(Guid id);
        Task<Dealer?> GetByCodeAsync(string code);
        Task<List<Dealer>> GetAllAsync();
        Task AddAsync(Dealer dealer);
        Task UpdateAsync(Dealer dealer);
        Task DeleteAsync(Guid id);
    }
}
