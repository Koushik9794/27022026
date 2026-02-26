using System.Text.Json;

namespace CatalogService.Domain.Aggregates;

/// <summary>
/// Component Type aggregate root.
/// Represents a physical component type in the warehouse taxonomy (e.g., UPRIGHT, BEAM, BASE_PLATE).
/// </summary>
public sealed class ComponentType
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public Guid ComponentGroupId { get; private set; }
    public Guid? ParentTypeId { get; private set; }
    public JsonDocument? AttributeSchema { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation properties (not persisted directly)
    public string? ComponentGroupCode { get; private set; }
    public string? ComponentGroupName { get; private set; }
    public string? ParentTypeCode { get; private set; }

    // Private constructor for persistence frameworks
    private ComponentType() { }

    /// <summary>
    /// Factory method to create a new ComponentType.
    /// </summary>
    public static ComponentType Create(
        string code,
        string name,
        Guid componentGroupId,
        string? description = null,
        Guid? parentTypeId = null,
        JsonDocument? attributeSchema = null)
    {
        ValidateCode(code);
        ValidateName(name);

        if (componentGroupId == Guid.Empty)
        {
            throw new ArgumentException("Component Group ID cannot be empty.", nameof(componentGroupId));
        }

        return new ComponentType
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            ComponentGroupId = componentGroupId,
            ParentTypeId = parentTypeId,
            AttributeSchema = attributeSchema,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method for rehydration from database.
    /// </summary>
    public static ComponentType Rehydrate(
        Guid id,
        string code,
        string name,
        string? description,
        Guid componentGroupId,
        Guid? parentTypeId,
        JsonDocument? attributeSchema,
        bool isActive,
        DateTime createdAt,
        DateTime? updatedAt,
        string? componentGroupCode = null,
        string? componentGroupName = null,
        string? parentTypeCode = null)
    {
        return new ComponentType
        {
            Id = id,
            Code = code,
            Name = name,
            Description = description,
            ComponentGroupId = componentGroupId,
            ParentTypeId = parentTypeId,
            AttributeSchema = attributeSchema,
            IsActive = isActive,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            ComponentGroupCode = componentGroupCode,
            ComponentGroupName = componentGroupName,
            ParentTypeCode = parentTypeCode
        };
    }

    public void Update(
        string name,
        string? description,
        Guid componentGroupId,
        Guid? parentTypeId,
        JsonDocument? attributeSchema)
    {
        ValidateName(name);

        if (componentGroupId == Guid.Empty)
        {
            throw new ArgumentException("Component Group ID cannot be empty.", nameof(componentGroupId));
        }

        // Prevent circular reference
        if (parentTypeId.HasValue && parentTypeId.Value == Id)
        {
            throw new ArgumentException("Component type cannot be its own parent.", nameof(parentTypeId));
        }

        Name = name.Trim();
        Description = description?.Trim();
        ComponentGroupId = componentGroupId;
        ParentTypeId = parentTypeId;
        AttributeSchema = attributeSchema;
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
            throw new ArgumentException("Component type code cannot be empty.", nameof(code));
        }

        if (code.Length > 50)
        {
            throw new ArgumentException("Component type code cannot exceed 50 characters.", nameof(code));
        }
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Component type name cannot be empty.", nameof(name));
        }

        if (name.Length > 255)
        {
            throw new ArgumentException("Component type name cannot exceed 255 characters.", nameof(name));
        }
    }
}
