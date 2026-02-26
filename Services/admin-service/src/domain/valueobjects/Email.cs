using System.Text.RegularExpressions;

namespace AdminService.Domain.ValueObjects
{
    /// <summary>
    /// Email Value Object - ensures email validity
    /// </summary>
    public sealed class Email
    {
        public string Value { get; private set; }

        private Email() { }

        public static Email Create(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
                throw new ArgumentException("Email cannot be empty", nameof(email));

            if (!IsValidEmail(email))
                throw new ArgumentException($"Invalid email format: {email}", nameof(email));

            return new Email { Value = email.ToLowerInvariant() };
        }

        private static bool IsValidEmail(string email)
        {
            return Regex.IsMatch(
                email,
                @"^[^@\s]+@[^@\s]+\.[^@\s]+$",
                RegexOptions.IgnoreCase);
        }

        public override bool Equals(object obj) =>
            obj is Email other && Value == other.Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value;
    }
}
