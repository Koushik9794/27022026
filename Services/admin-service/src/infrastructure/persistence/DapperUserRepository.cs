using Dapper;
using AdminService.Domain.Aggregates;
using AdminService.Domain.ValueObjects;
using AdminService.Domain.Entities;
using AdminService.Infrastructure.Dapper;

namespace AdminService.Infrastructure.Persistence
{
    /// <summary>
    /// Dapper-based implementation of IUserRepository
    /// </summary>
    public sealed class DapperUserRepository : IUserRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DapperUserRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                SELECT id, email, display_name, role, status, created_at, last_login_at, updated_at
                FROM users
                WHERE id = @Id";

            var row = await connection.QuerySingleOrDefaultAsync<UserRow>(sql, new { Id = id });
            return row?.ToDomain();
        }

        public async Task<User?> GetByEmailAsync(string email)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                SELECT id, email, display_name, role, status, created_at, last_login_at, updated_at
                FROM users
                WHERE email = @Email";

            var row = await connection.QuerySingleOrDefaultAsync<UserRow>(sql, new { Email = email });
            return row?.ToDomain();
        }

        public async Task<List<User>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                SELECT id, email, display_name, role, status, created_at, last_login_at, updated_at
                FROM users
                ORDER BY created_at DESC";

            var rows = await connection.QueryAsync<UserRow>(sql);
            return rows.Select(r => r.ToDomain()).ToList();
        }

        public async Task AddAsync(User user)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                INSERT INTO users (id, email, display_name, role, status, created_at, updated_at)
                VALUES (@Id, @Email, @DisplayName, @Role, @Status, @CreatedAt, @UpdatedAt)";

            await connection.ExecuteAsync(sql, new
            {
                Id = user.Id,
                Email = user.Email.Value,
                DisplayName = user.DisplayName.Value,
                Role = user.Role.Value,
                Status = user.Status.ToString(),
                CreatedAt = user.CreatedAt,
                UpdatedAt = user.UpdatedAt
            });
        }

        public async Task UpdateAsync(User user)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = @"
                UPDATE users
                SET display_name = @DisplayName,
                    role = @Role,
                    status = @Status,
                    last_login_at = @LastLoginAt,
                    updated_at = @UpdatedAt
                WHERE id = @Id";

            await connection.ExecuteAsync(sql, new
            {
                Id = user.Id,
                DisplayName = user.DisplayName.Value,
                Role = user.Role.Value,
                Status = user.Status.ToString(),
                LastLoginAt = user.LastLoginAt,
                UpdatedAt = user.UpdatedAt
            });
        }

        public async Task DeleteAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();

            const string sql = "DELETE FROM users WHERE id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        // DTO for Dapper mapping
        private sealed class UserRow
        {
            public Guid Id { get; set; }
            public string Email { get; set; } = string.Empty;
            public string Display_Name { get; set; } = string.Empty;
            public string Role { get; set; } = string.Empty;
            public string Status { get; set; } = string.Empty;
            public DateTime Created_At { get; set; }
            public DateTime? Last_Login_At { get; set; }
            public DateTime Updated_At { get; set; }

            public User ToDomain()
            {
                // Use reflection to reconstruct aggregate (private constructor)
                var user = (User)Activator.CreateInstance(typeof(User), true)!;
                
                typeof(User).GetProperty(nameof(User.Id))!.SetValue(user, Id);
                typeof(User).GetProperty(nameof(User.Email))!.SetValue(user, AdminService.Domain.ValueObjects.Email.Create(Email));
                typeof(User).GetProperty(nameof(User.DisplayName))!.SetValue(user, DisplayName.Create(Display_Name));
                typeof(User).GetProperty(nameof(User.Role))!.SetValue(user, UserRole.Create(Role));
                typeof(User).GetProperty(nameof(User.Status))!.SetValue(user, Enum.Parse<UserStatus>(Status));
                typeof(User).GetProperty(nameof(User.CreatedAt))!.SetValue(user, Created_At);
                typeof(User).GetProperty(nameof(User.LastLoginAt))!.SetValue(user, Last_Login_At);
                typeof(User).GetProperty(nameof(User.UpdatedAt))!.SetValue(user, Updated_At);

                return user;
            }
        }
    }
}
