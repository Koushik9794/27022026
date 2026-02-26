using AdminService.Domain.ValueObjects;
using AdminService.Domain.Entities;
using AdminService.Domain.Events;

namespace AdminService.Domain.Aggregates
{
    /// <summary>
    /// User Aggregate Root
    /// Represents a user in the warehouse configurator system (Dealer, Designer, Admin)
    /// </summary>
    public class User
    {
        public Guid Id { get; private set; }
        public Email Email { get; private set; }
        public DisplayName DisplayName { get; private set; }
        public UserRole Role { get; private set; }
        public UserStatus Status { get; private set; }
        public DateTime CreatedAt { get; private set; }
        public DateTime? LastLoginAt { get; private set; }
        public DateTime UpdatedAt { get; private set; }

        private User() { }

        /// <summary>
        /// Register a new user (Factory Method)
        /// </summary>
        public static User Register(Email email, DisplayName displayName, UserRole role)
        {
            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                DisplayName = displayName,
                Role = role,
                Status = UserStatus.PendingActivation,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            return user;
        }

        /// <summary>
        /// Activate user account
        /// </summary>
        public void Activate()
        {
            if (Status == UserStatus.Active)
                throw new InvalidOperationException("User is already active");

            Status = UserStatus.Active;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Deactivate user account
        /// </summary>
        public void Deactivate(string reason)
        {
            if (Status == UserStatus.Deactivated)
                throw new InvalidOperationException("User is already deactivated");

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Deactivation reason is required", nameof(reason));

            Status = UserStatus.Deactivated;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Suspend user account
        /// </summary>
        public void Suspend(string reason)
        {
            if (Status == UserStatus.Suspended)
                throw new InvalidOperationException("User is already suspended");

            if (string.IsNullOrWhiteSpace(reason))
                throw new ArgumentException("Suspension reason is required", nameof(reason));

            Status = UserStatus.Suspended;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Record user login
        /// </summary>
        public void RecordLogin()
        {
            if (Status != UserStatus.Active)
                throw new InvalidOperationException("Only active users can log in");

            LastLoginAt = DateTime.UtcNow;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        public void UpdateProfile(DisplayName newDisplayName)
        {
            if (DisplayName.Equals(newDisplayName))
                return; // No change

            DisplayName = newDisplayName;
            UpdatedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Change user role
        /// </summary>
        public void ChangeRole(UserRole newRole)
        {
            if (Role.Equals(newRole))
                return; // No change

            Role = newRole;
            UpdatedAt = DateTime.UtcNow;
        }
    }
}
