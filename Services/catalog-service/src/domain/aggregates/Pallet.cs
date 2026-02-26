using System.Text.Json;

namespace CatalogService.Domain.Aggregates;

/// <summary>
/// Domain model for Pallet type.
/// </summary>
public class Pallet
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }

    /// <summary>
    /// JSON schema defining what attributes this pallet type requires.
    /// Example: { "width": { "type": "number", "unit": "mm" }, "depth": {...} }
    /// </summary>
    public string? AttributeSchema { get; private set; }
    public string? GlbFilePath { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDeleted { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private Pallet() { }

    public static Pallet Create(
        string code,
        string name,
        string? description,
        string? glbFilePath,
        Dictionary<string, object>? attributeSchema = null,
        bool isActive = true,
        string? createdBy = null)
    {
        if (string.IsNullOrWhiteSpace(code)) throw new ArgumentException("Code is required.");
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.");

        return new Pallet
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Description = description,
            AttributeSchema = attributeSchema != null ? JsonSerializer.Serialize(attributeSchema) : null,
            GlbFilePath = glbFilePath,
            IsActive = isActive,
            IsDeleted = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public void Update(
        string name,
        string? description,
        Dictionary<string, object>? attributeSchema,
        string? glbFilePath,
        bool isActive,
        string? updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Name is required.");

        Name = name;
        Description = description;
        AttributeSchema = attributeSchema != null ? JsonSerializer.Serialize(attributeSchema) : null;
        GlbFilePath = glbFilePath;
        IsActive = isActive;
        UpdatedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
    }

    public static Pallet Rehydrate(
        Guid id,
        string code,
        string name,
        string? description,
        string? attributeSchema,
        string? glbFilePath,
        bool isActive,
        bool isDeleted,
        DateTime createdAt,
        string? createdBy,
        DateTime? updatedAt,
        string? updatedBy)
    {
        return new Pallet
        {
            Id = id,
            Code = code,
            Name = name,
            Description = description,
            AttributeSchema = attributeSchema,
            GlbFilePath = glbFilePath,
            IsActive = isActive,
            IsDeleted = isDeleted,
            CreatedAt = createdAt,
            CreatedBy = createdBy,
            UpdatedAt = updatedAt,
            UpdatedBy = updatedBy
        };
    }


    /// <summary>
    /// Gets attribute schema as a dictionary.
    /// </summary>
    /// <summary>
    /// Gets attribute schema as a dictionary.
    /// </summary>
    public Dictionary<string, object>? GetAttributeSchemaDictionary()
    {
        if (string.IsNullOrEmpty(AttributeSchema))
            return null;

        return JsonSerializer.Deserialize<Dictionary<string, object>>(AttributeSchema);
    }
}
