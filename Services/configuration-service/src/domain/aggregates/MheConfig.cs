using System.Text.Json;

namespace ConfigurationService.Domain.Aggregates;

/// <summary>
/// Material Handling Equipment configuration within a ConfigurationVersion.
/// References MHE types from the catalog-service taxonomy.
/// </summary>
public sealed class MheConfig
{
    public Guid Id { get; private set; }
    public Guid ConfigurationVersionId { get; private set; }
    public Guid? MheTypeId { get; private set; }  // References catalog-service MHE type
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public JsonDocument? Attributes { get; private set; }  // Additional MHE-specific attributes
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private MheConfig() { }

    public static MheConfig Create(
        Guid configurationVersionId,
        string name,
        Guid? mheTypeId = null,
        string? description = null,
        JsonDocument? attributes = null,
        string? createdBy = null)
    {
        if (configurationVersionId == Guid.Empty)
            throw new ArgumentException("Configuration Version ID cannot be empty.", nameof(configurationVersionId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        return new MheConfig
        {
            Id = Guid.NewGuid(),
            ConfigurationVersionId = configurationVersionId,
            MheTypeId = mheTypeId,
            Name = name.Trim(),
            Description = description?.Trim(),
            Attributes = attributes,
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static MheConfig Rehydrate(
        Guid id,
        Guid configurationVersionId,
        Guid? mheTypeId,
        string name,
        string? description,
        JsonDocument? attributes,
        bool isActive,
        DateTime createdAt,
        string? createdBy,
        DateTime? updatedAt,
        string? updatedBy)
    {
        return new MheConfig
        {
            Id = id,
            ConfigurationVersionId = configurationVersionId,
            MheTypeId = mheTypeId,
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
        Guid? mheTypeId,
        string? description,
        JsonDocument? attributes,
        string? updatedBy)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));

        Name = name.Trim();
        MheTypeId = mheTypeId;
        Description = description?.Trim();
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
