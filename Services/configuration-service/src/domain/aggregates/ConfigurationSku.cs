using System.Text.Json;

namespace ConfigurationService.Domain.Aggregates;

/// <summary>
/// SKU specification within a ConfigurationVersion.
/// SkuTypeId references catalog-service SkuType which defines the AttributeSchema.
/// Attributes store the actual values conforming to that schema.
/// </summary>
public sealed class ConfigurationSku
{
    public Guid Id { get; private set; }
    public Guid ConfigurationVersionId { get; private set; }  // Links to version, not configuration
    public Guid? SkuTypeId { get; private set; }  // References catalog-service SkuType
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    
    /// <summary>
    /// Flexible attribute values as JSONB. Structure defined by SkuType.AttributeSchema.
    /// </summary>
    public JsonDocument? Attributes { get; private set; }
    
    public int? UnitsPerLayer { get; private set; }
    public int? LayersPerPallet { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private ConfigurationSku() { }

    public static ConfigurationSku Create(
        Guid configurationVersionId,
        string code,
        string name,
        Guid? skuTypeId = null,
        string? description = null,
        JsonDocument? attributes = null,
        int? unitsPerLayer = null,
        int? layersPerPallet = null,
        string? createdBy = null)
    {
        if (configurationVersionId == Guid.Empty)
            throw new ArgumentException("Configuration Version ID cannot be empty.", nameof(configurationVersionId));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        return new ConfigurationSku
        {
            Id = Guid.NewGuid(),
            ConfigurationVersionId = configurationVersionId,
            SkuTypeId = skuTypeId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Attributes = attributes,
            UnitsPerLayer = unitsPerLayer,
            LayersPerPallet = layersPerPallet,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static ConfigurationSku Rehydrate(
        Guid id,
        Guid configurationVersionId,
        Guid? skuTypeId,
        string code,
        string name,
        string? description,
        JsonDocument? attributes,
        int? unitsPerLayer,
        int? layersPerPallet,
        bool isActive,
        DateTime createdAt,
        string? createdBy,
        DateTime? updatedAt,
        string? updatedBy)
    {
        return new ConfigurationSku
        {
            Id = id,
            ConfigurationVersionId = configurationVersionId,
            SkuTypeId = skuTypeId,
            Code = code,
            Name = name,
            Description = description,
            Attributes = attributes,
            UnitsPerLayer = unitsPerLayer,
            LayersPerPallet = layersPerPallet,
            IsActive = isActive,
            CreatedAt = createdAt,
            CreatedBy = createdBy,
            UpdatedAt = updatedAt,
            UpdatedBy = updatedBy
        };
    }

    public void Update(
        string name,
        string? description,
        Guid? skuTypeId,
        JsonDocument? attributes,
        int? unitsPerLayer,
        int? layersPerPallet,
        string? updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        SkuTypeId = skuTypeId;
        Attributes = attributes;
        UnitsPerLayer = unitsPerLayer;
        LayersPerPallet = layersPerPallet;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate(string? updatedBy)
    {
        IsActive = false;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
