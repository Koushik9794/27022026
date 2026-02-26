namespace AdminService.Domain.ValueObjects
{
    /// <summary>
    /// UserRole Value Object - represents user roles in the system
    /// </summary>
    public sealed class UserRole
    {
        public string Value { get; private set; }

        // Predefined roles
        public static readonly UserRole SuperAdmin = new() { Value = "SUPER_ADMIN" };
        public static readonly UserRole Admin = new() { Value = "ADMIN" };
        public static readonly UserRole Dealer = new() { Value = "DEALER" };
        public static readonly UserRole Designer = new() { Value = "DESIGNER" };
        public static readonly UserRole Viewer = new() { Value = "VIEWER" };

        private static readonly HashSet<string> ValidRoles = new()
        {
            "SUPER_ADMIN", "ADMIN", "DEALER", "DESIGNER", "VIEWER"
        };

        private UserRole() { }

        public static UserRole Create(string role)
        {
            if (string.IsNullOrWhiteSpace(role))
                throw new ArgumentException("Role cannot be empty", nameof(role));

            var upperRole = role.ToUpperInvariant();
            if (!ValidRoles.Contains(upperRole))
                throw new ArgumentException(
                    $"Invalid role: {role}. Valid roles: {string.Join(", ", ValidRoles)}",
                    nameof(role));

            return new UserRole { Value = upperRole };
        }

        public override bool Equals(object obj) =>
            obj is UserRole other && Value == other.Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value;
    }
}
