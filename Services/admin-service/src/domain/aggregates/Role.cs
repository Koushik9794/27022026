using AdminService.Domain.ValueObjects;
using VO = AdminService.Domain.ValueObjects;

namespace AdminService.Domain.Aggregates
{
    /// <summary>
    /// Role aggregate root — maps to app_roles table and encapsulates behavior.
    /// </summary>
    public sealed class Role
    {
        // Identity
        public Guid Id { get; private set; }

        // Core fields
        public string RoleName { get; private set; } = default!;
        public string? Description { get; private set; }

        // State flags
        public bool IsActive { get; private set; } = true;
        public bool IsDeleted { get; private set; } = false;

        // Audit
        public string? CreatedBy { get; private set; }
        public DateTimeOffset CreatedAt { get; private set; }
        public string? ModifiedBy { get; private set; }
        public DateTimeOffset? ModifiedAt { get; private set; }
        public int PermissionCount { get; private set; }

        private Role() { } // For Dapper/materialization

        public static Role Create(string roleName, string? description, string? createdBy)
        {
            return new Role
            {
                Id = Guid.NewGuid(),
                RoleName = roleName,
                Description = description,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = createdBy,
                CreatedAt = DateTimeOffset.UtcNow
            };
        }

        public static Role Create(string roleName, string? description, string? createdBy, DateTime createdAt)
        {
            return new Role
            {
                Id = Guid.NewGuid(),
                RoleName = roleName,
                Description = description,
                IsActive = true,
                IsDeleted = false,
                CreatedBy = createdBy,
                CreatedAt = createdAt
            };
        }

        public static Role Rehydrate(
            Guid id,
            string roleName,
            string? description,
            bool isActive,
            bool isDeleted,
            string? createdBy,
            DateTimeOffset createdAt,
            string? modifiedBy,
            DateTimeOffset? modifiedAt,
            int permissionCount = 0)
        {
            return new Role
            {
                Id = id,
                RoleName = roleName,
                Description = description,
                IsActive = isActive,
                IsDeleted = isDeleted,
                CreatedBy = createdBy,
                CreatedAt = createdAt,
                ModifiedBy = modifiedBy,
                ModifiedAt = modifiedAt,
                PermissionCount = permissionCount
            };
        }

        public void Update(string roleName, string? description, string? modifiedBy)
        {
            RoleName = roleName;
            Description = description;
            ModifiedBy = modifiedBy;
            ModifiedAt = DateTimeOffset.UtcNow;
        }

        public void Activate(string? modifiedBy)
        {
            if (!IsActive)
            {
                IsActive = true;
                ModifiedBy = modifiedBy;
                ModifiedAt = DateTimeOffset.UtcNow;
            }
        }

        public void Deactivate(string? modifiedBy)
        {
            if (IsActive)
            {
                IsActive = false;
                ModifiedBy = modifiedBy;
                ModifiedAt = DateTimeOffset.UtcNow;
            }
        }

        public void SoftDelete(string? modifiedBy)
        {
            if (!IsDeleted)
            {
                IsDeleted = true;
                ModifiedBy = modifiedBy;
                ModifiedAt = DateTimeOffset.UtcNow;
            }
        }
    }
}
