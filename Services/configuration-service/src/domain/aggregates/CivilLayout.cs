namespace ConfigurationService.Domain.Aggregates;

/// <summary>
/// Civil layout for a specific configuration.
/// Stores the core building/structural layout in JSON format.
/// </summary>
public sealed class CivilLayout
{
    public Guid Id { get; private set; }
    public Guid ConfigurationId { get; private set; }
    public Guid? WarehouseType { get; private set; }
    public string? SourceFile { get; private set; }
    public string? CivilJson { get; private set; }
    public int VersionNo { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private CivilLayout() { }

    public static CivilLayout Create(
        Guid configurationId,
        Guid? warehouseType,
        string? sourceFile,
        string? civilJson,

        string? createdBy = null)
    {
        if (configurationId == Guid.Empty)
            throw new ArgumentException("Configuration ID cannot be empty.", nameof(configurationId));

        return new CivilLayout
        {
            Id = Guid.NewGuid(),
            ConfigurationId = configurationId,
            WarehouseType = warehouseType,
            SourceFile = sourceFile,
            CivilJson = civilJson,

            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static CivilLayout Rehydrate(
        Guid id,
        Guid configurationId,
        Guid? warehouseType,
        string? sourceFile,
        string? civilJson,
        int versionNo,
        DateTime createdAt,
        string? createdBy,
        DateTime? updatedAt,
        string? updatedBy)
    {
        return new CivilLayout
        {
            Id = id,
            ConfigurationId = configurationId,
            WarehouseType = warehouseType,
            SourceFile = sourceFile,
            CivilJson = civilJson,
            VersionNo = versionNo,
            CreatedAt = createdAt,
            CreatedBy = createdBy,
            UpdatedAt = updatedAt,
            UpdatedBy = updatedBy
        };
    }

    public void Update(Guid? warehouseType, string? sourceFile, string? civilJson,  string? updatedBy)
    {
        WarehouseType = warehouseType;
        SourceFile = sourceFile;
        CivilJson = civilJson;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}
