using System.Text.RegularExpressions;

namespace AdminService.Domain.ValueObjects
{
    public sealed record RoleName
    {
        public string Value { get; }

        private RoleName(string value) => Value = value;

        public static RoleName Create(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new ArgumentException("Role name cannot be empty.");

            var trimmed = value.Trim();
            if (trimmed.Length < 3 || trimmed.Length > 50)
                throw new ArgumentException("Role name must be between 3 and 50 characters.");

            if (!Regex.IsMatch(trimmed, @"^[a-zA-Z0-9\s\-_]+$"))
                throw new ArgumentException("Role name contains invalid characters.");

            return new RoleName(trimmed);
        }
    }
}
