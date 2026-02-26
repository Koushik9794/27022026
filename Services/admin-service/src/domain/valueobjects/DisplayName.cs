namespace AdminService.Domain.ValueObjects
{
    /// <summary>
    /// DisplayName Value Object - ensures display name validity
    /// </summary>
    public sealed class DisplayName
    {
        public string Value { get; private set; }

        private DisplayName() { }

        public static DisplayName Create(string displayName)
        {
            if (string.IsNullOrWhiteSpace(displayName))
                throw new ArgumentException("Display name cannot be empty", nameof(displayName));

            if (displayName.Length < 2 || displayName.Length > 100)
                throw new ArgumentException("Display name must be between 2 and 100 characters", nameof(displayName));

            return new DisplayName { Value = displayName.Trim() };
        }

        public override bool Equals(object obj) =>
            obj is DisplayName other && Value == other.Value;

        public override int GetHashCode() => Value.GetHashCode();

        public override string ToString() => Value;
    }
}
