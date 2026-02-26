namespace AdminService.Domain.ValueObjects;

public sealed record PermissionName(string Value)
{
    public static PermissionName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length is < 2 or > 150)
            throw new ArgumentException("Permission name length must be 2-150.");
        return new PermissionName(value.Trim());
    }
}
