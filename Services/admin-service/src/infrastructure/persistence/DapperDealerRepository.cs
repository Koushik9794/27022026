using Dapper;
using AdminService.Domain.Aggregates;
using AdminService.Domain.ValueObjects;
using AdminService.Infrastructure.Dapper;

namespace AdminService.Infrastructure.Persistence
{
    /// <summary>
    /// Dapper-based implementation of IDealerRepository
    /// </summary>
    public sealed class DapperDealerRepository : IDealerRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public DapperDealerRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
        }

        public async Task<Dealer?> GetByIdAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT id, code, name, contact_name, contact_email, contact_phone, 
                       country_code, state, city, address, is_active, is_deleted, 
                       created_by, updated_by, created_at, updated_at
                FROM dealers
                WHERE id = @Id AND is_deleted = false";

            var row = await connection.QuerySingleOrDefaultAsync<DealerRow>(sql, new { Id = id });
            return row?.ToDomain();
        }

        public async Task<Dealer?> GetByCodeAsync(string code)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT id, code, name, contact_name, contact_email, contact_phone, 
                       country_code, state, city, address, is_active, is_deleted, 
                       created_by, updated_by, created_at, updated_at
                FROM dealers
                WHERE code = @Code AND is_deleted = false";

            var row = await connection.QuerySingleOrDefaultAsync<DealerRow>(sql, new { Code = code });
            return row?.ToDomain();
        }

        public async Task<List<Dealer>> GetAllAsync()
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                SELECT id, code, name, contact_name, contact_email, contact_phone, 
                       country_code, state, city, address, is_active, is_deleted, 
                       created_by, updated_by, created_at, updated_at
                FROM dealers
                WHERE is_deleted = false
                ORDER BY created_at DESC";

            var rows = await connection.QueryAsync<DealerRow>(sql);
            return rows.Select(r => r.ToDomain()).ToList();
        }

        public async Task AddAsync(Dealer dealer)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                INSERT INTO dealers (
                    id, code, name, contact_name, contact_email, contact_phone, 
                    country_code, state, city, address, is_active, is_deleted, 
                    created_by, created_at, updated_at
                ) VALUES (
                    @Id, @Code, @Name, @ContactName, @ContactEmail, @ContactPhone, 
                    @CountryCode, @State, @City, @Address, @IsActive, @IsDeleted, 
                    @CreatedBy, @CreatedAt, @UpdatedAt
                )";

            await connection.ExecuteAsync(sql, new
            {
                Id = dealer.Id,
                Code = dealer.Code,
                Name = dealer.Name,
                ContactName = dealer.ContactName,
                ContactEmail = dealer.ContactEmail?.Value,
                ContactPhone = dealer.ContactPhone,
                CountryCode = dealer.CountryCode,
                State = dealer.State,
                City = dealer.City,
                Address = dealer.Address,
                IsActive = dealer.IsActive,
                IsDeleted = dealer.IsDeleted,
                CreatedBy = dealer.CreatedBy,
                CreatedAt = dealer.CreatedAt,
                UpdatedAt = dealer.UpdatedAt
            });
        }

        public async Task UpdateAsync(Dealer dealer)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = @"
                UPDATE dealers
                SET name = @Name,
                    contact_name = @ContactName,
                    contact_email = @ContactEmail,
                    contact_phone = @ContactPhone,
                    country_code = @CountryCode,
                    state = @State,
                    city = @City,
                    address = @Address,
                    is_active = @IsActive,
                    updated_by = @UpdatedBy,
                    updated_at = @UpdatedAt
                WHERE id = @Id";

            await connection.ExecuteAsync(sql, new
            {
                Id = dealer.Id,
                Name = dealer.Name,
                ContactName = dealer.ContactName,
                ContactEmail = dealer.ContactEmail?.Value,
                ContactPhone = dealer.ContactPhone,
                CountryCode = dealer.CountryCode,
                State = dealer.State,
                City = dealer.City,
                Address = dealer.Address,
                IsActive = dealer.IsActive,
                UpdatedBy = dealer.UpdatedBy,
                UpdatedAt = dealer.UpdatedAt
            });
        }

        public async Task DeleteAsync(Guid id)
        {
            using var connection = _connectionFactory.CreateConnection();
            const string sql = "UPDATE dealers SET is_deleted = true WHERE id = @Id";
            await connection.ExecuteAsync(sql, new { Id = id });
        }

        private sealed class DealerRow
        {
            public Guid Id { get; set; }
            public string Code { get; set; } = string.Empty;
            public string Name { get; set; } = string.Empty;
            public string? Contact_Name { get; set; }
            public string? Contact_Email { get; set; }
            public string? Contact_Phone { get; set; }
            public string? Country_Code { get; set; }
            public string? State { get; set; }
            public string? City { get; set; }
            public string? Address { get; set; }
            public bool Is_Active { get; set; }
            public bool Is_Deleted { get; set; }
            public Guid Created_By { get; set; }
            public Guid? Updated_By { get; set; }
            public DateTime Created_At { get; set; }
            public DateTime? Updated_At { get; set; }

            public Dealer ToDomain()
            {
                var dealer = (Dealer)Activator.CreateInstance(typeof(Dealer), true)!;

                typeof(Dealer).GetProperty(nameof(Dealer.Id))!.SetValue(dealer, Id);
                typeof(Dealer).GetProperty(nameof(Dealer.Code))!.SetValue(dealer, Code);
                typeof(Dealer).GetProperty(nameof(Dealer.Name))!.SetValue(dealer, Name);
                typeof(Dealer).GetProperty(nameof(Dealer.ContactName))!.SetValue(dealer, Contact_Name);
                
                if (!string.IsNullOrEmpty(Contact_Email))
                {
                    typeof(Dealer).GetProperty(nameof(Dealer.ContactEmail))!.SetValue(dealer, Email.Create(Contact_Email));
                }

                typeof(Dealer).GetProperty(nameof(Dealer.ContactPhone))!.SetValue(dealer, Contact_Phone);
                typeof(Dealer).GetProperty(nameof(Dealer.CountryCode))!.SetValue(dealer, Country_Code);
                typeof(Dealer).GetProperty(nameof(Dealer.State))!.SetValue(dealer, State);
                typeof(Dealer).GetProperty(nameof(Dealer.City))!.SetValue(dealer, City);
                typeof(Dealer).GetProperty(nameof(Dealer.Address))!.SetValue(dealer, Address);
                typeof(Dealer).GetProperty(nameof(Dealer.IsActive))!.SetValue(dealer, Is_Active);
                typeof(Dealer).GetProperty(nameof(Dealer.IsDeleted))!.SetValue(dealer, Is_Deleted);
                typeof(Dealer).GetProperty(nameof(Dealer.CreatedBy))!.SetValue(dealer, Created_By);
                typeof(Dealer).GetProperty(nameof(Dealer.UpdatedBy))!.SetValue(dealer, Updated_By);
                typeof(Dealer).GetProperty(nameof(Dealer.CreatedAt))!.SetValue(dealer, Created_At);
                typeof(Dealer).GetProperty(nameof(Dealer.UpdatedAt))!.SetValue(dealer, Updated_At);

                return dealer;
            }
        }
    }
}
