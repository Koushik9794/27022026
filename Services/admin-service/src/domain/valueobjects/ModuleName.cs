using AdminService.Domain.Exceptions;

namespace AdminService.Domain.ValueObjects;

public sealed record ModuleName(string Value)
{
    private static readonly string[] ValidModules = 
    {
        "Product Group", "Sub Products", "Weight", "Price", "Review", "BOM", 
        "Outputs", "Standard Type", "Design", "Rules", "Audit", "BOM Type", "Generate Outputs"
    };

    public static ModuleName Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 150)
            throw new DomainException("Module name invalid.");

        var trimmed = value.Trim();
        if (!ValidModules.Contains(trimmed, StringComparer.OrdinalIgnoreCase))
            throw new DomainException($"Module Name '{trimmed}' is not allowed. Must be one of: {string.Join(", ", ValidModules)}");

        return new ModuleName(trimmed);
    }
}
