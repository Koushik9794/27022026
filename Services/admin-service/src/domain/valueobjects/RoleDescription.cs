namespace AdminService.Domain.ValueObjects
{
    public static class RoleDescription
    {
        public static string? Normalize(string? description)
        {
            if (string.IsNullOrWhiteSpace(description))
                return null;

            var trimmed = description.Trim();
            return trimmed.Length > 500 ? trimmed[..500] : trimmed;
        }
    }
}
