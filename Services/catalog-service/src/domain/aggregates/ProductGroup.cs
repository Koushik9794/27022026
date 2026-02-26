namespace CatalogService.Domain.Aggregates;

/// <summary>
/// Product Group aggregate root.
/// Represents a product group (e.g., SPR, Cantilever, ASRS) with optional variants.
/// </summary>
public sealed class ProductGroup
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public Guid? ParentGroupId { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Navigation property (not persisted directly)
    public string? ParentGroupCode { get; private set; }

    // Private constructor for persistence frameworks
    private ProductGroup() { }

    /// <summary>
    /// Factory method to create a new ProductGroup.
    /// </summary>
    public static ProductGroup Create(
        string code,
        string name,
        string? description = null,
        Guid? parentGroupId = null)
    {
        ValidateCode(code);
        ValidateName(name);

        return new ProductGroup
        {
            Id = Guid.NewGuid(),
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            ParentGroupId = parentGroupId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Factory method for rehydration from database.
    /// </summary>
    public static ProductGroup Rehydrate(
        Guid id,
        string code,
        string name,
        string? description,
        Guid? parentGroupId,
        bool isActive,
        DateTime createdAt,
        DateTime? updatedAt,
        string? parentGroupCode = null)
    {
        return new ProductGroup
        {
            Id = id,
            Code = code,
            Name = name,
            Description = description,
            ParentGroupId = parentGroupId,
            IsActive = isActive,
            CreatedAt = createdAt,
            UpdatedAt = updatedAt,
            ParentGroupCode = parentGroupCode
        };
    }

    public void Update(string name, string? description, Guid? parentGroupId)
    {
        ValidateName(name);

        // Prevent circular reference
        if (parentGroupId.HasValue && parentGroupId.Value == Id)
        {
            throw new ArgumentException("Product group cannot be its own parent.", nameof(parentGroupId));
        }

        Name = name.Trim();
        Description = description?.Trim();
        ParentGroupId = parentGroupId;
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

    /// <summary>
    /// Checks if this product group is a variant (has a parent).
    /// </summary>
    public bool IsVariant => ParentGroupId.HasValue;

    private static void ValidateCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code))
        {
            throw new ArgumentException("Product group code cannot be empty.", nameof(code));
        }

        if (code.Length > 50)
        {
            throw new ArgumentException("Product group code cannot exceed 50 characters.", nameof(code));
        }
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Product group name cannot be empty.", nameof(name));
        }

        if (name.Length > 255)
        {
            throw new ArgumentException("Product group name cannot exceed 255 characters.", nameof(name));
        }
    }
}
