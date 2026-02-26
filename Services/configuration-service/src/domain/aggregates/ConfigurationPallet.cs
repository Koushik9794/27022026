using System.Text.Json;

namespace ConfigurationService.Domain.Aggregates;

/// <summary>
/// Pallet specification within a ConfigurationVersion.
/// PalletTypeId references catalog-service PalletType which defines the AttributeSchema.
/// Attributes store the actual values conforming to that schema.
/// </summary>
public sealed class ConfigurationPallet
{
    public Guid Id { get; private set; }
    public Guid ConfigurationVersionId { get; private set; }  // Links to version, not configuration
    public Guid? PalletTypeId { get; private set; }  // References catalog-service PalletType
    public string Code { get; private set; } = default!;
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    
    /// <summary>
    /// Flexible attribute values as JSONB. Structure defined by PalletType.AttributeSchema.
    /// </summary>
    public JsonDocument? Attributes { get; private set; }
    
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private ConfigurationPallet() { }

    public static ConfigurationPallet Create(
        Guid configurationVersionId,
        string code,
        string name,
        Guid? palletTypeId = null,
        string? description = null,
        JsonDocument? attributes = null,
        string? createdBy = null)
    {
        if (configurationVersionId == Guid.Empty)
            throw new ArgumentException("Configuration Version ID cannot be empty.", nameof(configurationVersionId));
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Code cannot be empty.", nameof(code));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        return new ConfigurationPallet
        {
            Id = Guid.NewGuid(),
            ConfigurationVersionId = configurationVersionId,
            PalletTypeId = palletTypeId,
            Code = code.Trim().ToUpperInvariant(),
            Name = name.Trim(),
            Description = description?.Trim(),
            Attributes = attributes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static ConfigurationPallet Rehydrate(
        Guid id,
        Guid configurationVersionId,
        Guid? palletTypeId,
        string code,
        string name,
        string? description,
        JsonDocument? attributes,
        bool isActive,
        DateTime createdAt,
        string? createdBy,
        DateTime? updatedAt,
        string? updatedBy)
    {
        return new ConfigurationPallet
        {
            Id = id,
            ConfigurationVersionId = configurationVersionId,
            PalletTypeId = palletTypeId,
            Code = code,
            Name = name,
            Description = description,
            Attributes = attributes,
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
        Guid? palletTypeId,
        JsonDocument? attributes,
        string? updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name.Trim();
        Description = description?.Trim();
        PalletTypeId = palletTypeId;
        Attributes = attributes;
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
