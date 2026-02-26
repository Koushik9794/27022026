using AdminService.Domain.Aggregates;

namespace AdminService.Infrastructure.Persistence
{
    /// <summary>
    /// Repository interface for User aggregate
    /// </summary>
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User?> GetByEmailAsync(string email);
        Task<List<User>> GetAllAsync();
        Task AddAsync(User user);
        Task UpdateAsync(User user);
        Task DeleteAsync(Guid id);
    }
}
