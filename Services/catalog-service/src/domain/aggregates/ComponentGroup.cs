namespace CatalogService.Domain.Aggregates;

/// <summary>
/// Component Group aggregate root.
/// Represents a high-level grouping for parts (e.g., Structural Components, Fasteners).
/// Distinct from ComponentCategory which is used for taxonomy.
/// </summary>
public sealed class ComponentGroup
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public int SortOrder { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Private constructor for persistence frameworks
    private ComponentGroup() { }

    /// <summary>
    /// Factory method to create a new ComponentGroup.
    /// </summary>
    public static ComponentGroup Create(
        string code,
        string name,
        string? description = null,
        int sortOrder = 0)
    {
        ValidateCode(code);
        ValidateName(name);

        return new ComponentGroup
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            SortOrder = sortOrder,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method for rehydration from database.
    /// </summary>
    public static ComponentGroup Rehydrate(
        Guid id,
        string code,
        string name,
        string? description,
        int sortOrder,
        bool isActive,
        DateTime createdAt,
        DateTime? updatedAt)
    {
        return new ComponentGroup
        {
            Id = id,
            Code = code,
            Name = name,
            Description = description,
            SortOrder = sortOrder,
            IsActive = isActive,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt
        };
    }

    public void Update(string name, string? description, int sortOrder)
    {
        ValidateName(name);

        Name = name.Trim();
        Description = description?.Trim();
        SortOrder = sortOrder;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Component Group code cannot be empty.", nameof(code));
        }

        if (code.Length > 50)
        {
            throw new ArgumentException("Component Group code cannot exceed 50 characters.", nameof(code));
        }
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Component Group name cannot be empty.", nameof(name));
        }

        if (name.Length > 255)
        {
            throw new ArgumentException("Component Group name cannot exceed 255 characters.", nameof(name));
        }
    }
}
