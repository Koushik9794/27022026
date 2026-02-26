using AdminService.Domain.Exceptions;

namespace AdminService.Domain.ValueObjects;

public sealed record EntityName(string Value)
{
    private static readonly string[] ValidEntities = 
    {
        "Access & Pricing", "Standards & Reviews", "General"
    };

    public static EntityName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 150)
            throw new DomainException("Entity name invalid.");

        var trimmed = value.Trim();
        if (!ValidEntities.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
            throw new DomainException($"Entity Name '{trimmed}' is not allowed. Must be one of: {string.Join(", ", ValidEntities)}");

        return new EntityName(trimmed);
    }
}
