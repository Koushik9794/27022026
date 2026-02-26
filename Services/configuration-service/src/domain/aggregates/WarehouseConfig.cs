namespace ConfigurationService.Domain.Aggregates;

/// <summary>
/// Warehouse configuration context within a ConfigurationVersion.
/// </summary>
public sealed class WarehouseConfig
{
    public Guid Id { get; private set; }
    public Guid ConfigurationVersionId { get; private set; }  // Links to version
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public decimal LengthM { get; private set; }
    public decimal WidthM { get; private set; }
    public decimal ClearHeightM { get; private set; }
    public string? FloorType { get; private set; }
    public decimal? FloorLoadCapacityKnM2 { get; private set; }
    public string? SeismicZone { get; private set; }
    public string? MheType { get; private set; }
    public string? TemperatureRange { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private WarehouseConfig() { }

    public static WarehouseConfig Create(
        Guid configurationVersionId,
        string name,
        decimal lengthM,
        decimal widthM,
        decimal clearHeightM,
        string? description = null,
        string? floorType = null,
        decimal? floorLoadCapacityKnM2 = null,
        string? seismicZone = null,
        string? mheType = null,
        string? temperatureRange = null,
        string? createdBy = null)
    {
        if (configurationVersionId == Guid.Empty)
            throw new ArgumentException("Configuration Version ID cannot be empty.", nameof(configurationVersionId));
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Name cannot be empty.", nameof(name));
        if (lengthM <= 0) throw new ArgumentException("Length must be positive.", nameof(lengthM));
        if (widthM <= 0) throw new ArgumentException("Width must be positive.", nameof(widthM));
        if (clearHeightM <= 0) throw new ArgumentException("Clear height must be positive.", nameof(clearHeightM));

        return new WarehouseConfig
        {
            Id = Guid.NewGuid(),
            ConfigurationVersionId = configurationVersionId,
            Name = name.Trim(),
            Description = description?.Trim(),
            LengthM = lengthM,
            WidthM = widthM,
            ClearHeightM = clearHeightM,
            FloorType = floorType?.Trim(),
            FloorLoadCapacityKnM2 = floorLoadCapacityKnM2,
            SeismicZone = seismicZone?.Trim(),
            MheType = mheType?.Trim(),
            TemperatureRange = temperatureRange?.Trim(),
            IsActive = true,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static WarehouseConfig Rehydrate(
        Guid id,
        Guid configurationVersionId,
        string name,
        string? description,
        decimal lengthM,
        decimal widthM,
        decimal clearHeightM,
        string? floorType,
        decimal? floorLoadCapacityKnM2,
        string? seismicZone,
        string? mheType,
        string? temperatureRange,
        bool isActive,
        DateTime createdAt,
        string? createdBy,
        DateTime? updatedAt,
        string? updatedBy)
    {
        return new WarehouseConfig
        {
            Id = id,
            ConfigurationVersionId = configurationVersionId,
            Name = name,
            Description = description,
            LengthM = lengthM,
            WidthM = widthM,
            ClearHeightM = clearHeightM,
            FloorType = floorType,
            FloorLoadCapacityKnM2 = floorLoadCapacityKnM2,
            SeismicZone = seismicZone,
            MheType = mheType,
            TemperatureRange = temperatureRange,
            IsActive = isActive,
            CreatedAt = createdAt,
            CreatedBy = createdBy,
            UpdatedAt = updatedAt,
            UpdatedBy = updatedBy
        };
    }

    public void Deactivate(string? updatedBy)
    {
        IsActive = false;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
