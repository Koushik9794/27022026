using System.Text.Json;

namespace ConfigurationService.Domain.Aggregates;

/// <summary>
/// Storage configuration for a specific floor within a ConfigurationVersion.
/// Contains the design layout JSON (civil data + constraints + product group placements).
/// Each floor can have its own storage configuration layer.
/// </summary>
public sealed class StorageConfiguration
{
    public Guid Id { get; private set; }
    public Guid ConfigurationVersionId { get; private set; }
    public Guid? FloorId { get; private set; }  // Links to warehouse_floors (optional if not yet assigned)
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public string ProductGroup { get; private set; } = default!;
    public JsonDocument? DesignData { get; private set; }  // Civil layout + constraints + storage placements
    public DateTime? LastSavedAt { get; private set; }  // Autosave tracking
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private StorageConfiguration() { }

    public static StorageConfiguration Create(
        Guid configurationVersionId,
        string name,
        string productGroup,
        string? description = null,
        Guid? floorId = null,
        JsonDocument? designData = null,
        string? createdBy = null)
    {
        if (configurationVersionId == Guid.Empty)
            throw new ArgumentException("Configuration Version ID cannot be empty.", nameof(configurationVersionId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (string.IsNullOrWhiteSpace(productGroup))
            throw new ArgumentException("Product group cannot be empty.", nameof(productGroup));

        return new StorageConfiguration
        {
            Id = Guid.NewGuid(),
            ConfigurationVersionId = configurationVersionId,
            FloorId = floorId,
            Name = name.Trim(),
            Description = description?.Trim(),
            ProductGroup = productGroup.Trim().ToUpperInvariant(),
            DesignData = designData,
            LastSavedAt = designData != null ? DateTime.UtcNow : null,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static StorageConfiguration Rehydrate(
        Guid id,
        Guid configurationVersionId,
        Guid? floorId,
        string name,
        string? description,
        string productGroup,
        JsonDocument? designData,
        DateTime? lastSavedAt,
        bool isActive,
        DateTime createdAt,
        string? createdBy,
        DateTime? updatedAt,
        string? updatedBy)
    {
        return new StorageConfiguration
        {
            Id = id,
            ConfigurationVersionId = configurationVersionId,
            FloorId = floorId,
            Name = name,
            Description = description,
            ProductGroup = productGroup,
            DesignData = designData,
            LastSavedAt = lastSavedAt,
            IsActive = isActive,
            CreatedAt = createdAt,
            CreatedBy = createdBy,
            UpdatedAt = updatedAt,
            UpdatedBy = updatedBy
        };
    }

    /// <summary>
    /// Updates the design data (used for autosave from UI).
    /// </summary>
    public void UpdateDesignData(JsonDocument? designData, string? updatedBy)
    {
        DesignData = designData;
        LastSavedAt = DateTime.UtcNow;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Assigns or updates the floor reference.
    /// </summary>
    public void AssignFloor(Guid floorId, string? updatedBy)
    {
        if (floorId == Guid.Empty)
            throw new ArgumentException("Floor ID cannot be empty.", nameof(floorId));

        FloorId = floorId;
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
