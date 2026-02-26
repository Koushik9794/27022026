namespace CatalogService.Domain.Aggregates;

/// <summary>
/// Component Name aggregate root.
/// Represents a specific family name of a component (e.g., "UPRIGHT GXL", "BEAM BOX 75").
/// Linked to a specific Component Type.
/// </summary>
public sealed class ComponentName
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public Guid ComponentTypeId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties (not persisted directly)
    public string? ComponentTypeCode { get; private set; }
    public string? ComponentTypeName { get; private set; }

    // Private constructor for persistence frameworks
    private ComponentName() { }

    /// <summary>
    /// Factory method to create a new ComponentName.
    /// </summary>
    public static ComponentName Create(
        string code,
        string name,
        Guid componentTypeId,
        string? description = null)
    {
        ValidateCode(code);
        ValidateName(name);

        if (componentTypeId == Guid.Empty)
        {
            throw new ArgumentException("Component Type ID cannot be empty.", nameof(componentTypeId));
        }

        return new ComponentName
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            ComponentTypeId = componentTypeId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method for rehydration from database.
    /// </summary>
    public static ComponentName Rehydrate(
        Guid id,
        string code,
        string name,
        string? description,
        Guid componentTypeId,
        bool isActive,
        DateTime createdAt,
        DateTime? updatedAt,
        string? componentTypeCode = null,
        string? componentTypeName = null)
    {
        return new ComponentName
        {
            Id = id,
            Code = code,
            Name = name,
            Description = description,
            ComponentTypeId = componentTypeId,
            IsActive = isActive,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            ComponentTypeCode = componentTypeCode,
            ComponentTypeName = componentTypeName
        };
    }

    public void Update(string name, string? description, Guid componentTypeId)
    {
        ValidateName(name);

        if (componentTypeId == Guid.Empty)
        {
            throw new ArgumentException("Component Type ID cannot be empty.", nameof(componentTypeId));
        }

        Name = name.Trim();
        Description = description?.Trim();
        ComponentTypeId = componentTypeId;
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
            throw new ArgumentException("Component Name code cannot be empty.", nameof(code));
        }

        if (code.Length > 50)
        {
            throw new ArgumentException("Component Name code cannot exceed 50 characters.", nameof(code));
        }
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Component Name cannot be empty.", nameof(name));
        }

        if (name.Length > 255)
        {
            throw new ArgumentException("Component Name cannot exceed 255 characters.", nameof(name));
        }
    }
}
