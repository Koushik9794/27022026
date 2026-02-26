using ConfigurationService.Domain.Aggregates;
using ConfigurationService.Domain.Enums;

namespace ConfigurationService.Domain.Aggregates;

/// <summary>
/// ConfigurationVersion - a versioned snapshot of a Configuration's state.
/// Each version contains all the child entities (SKUs, Pallets, Warehouse, Rack).
/// Configuration can have multiple versions (v1, v2, v3).
/// </summary>
public sealed class ConfigurationVersion
{
    public Guid Id { get; private set; }
    public Guid ConfigurationId { get; private set; }
    public int VersionNumber { get; private set; }
    public string? Description { get; private set; }
    public bool IsCurrent { get; private set; }

    public bool IsLocked { get; private set; }
    public EnquiryStatus? Status { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }



    //
    private readonly List<CivilLayout> _civilLayouts = new();
    private readonly List<RackLayout> _rackLayouts = new();

    // Child entity collections for this version
    private readonly List<ConfigurationSku> _skus = new();
    private readonly List<ConfigurationPallet> _pallets = new();
    private readonly List<WarehouseConfig> _warehouseConfigs = new();
    private readonly List<StorageConfiguration> _storageConfigurations = new();
    private readonly List<MheConfig> _mheConfigs = new();

    public IReadOnlyCollection<CivilLayout> CivilLayouts => _civilLayouts.AsReadOnly();
    public IReadOnlyCollection<RackLayout> rackLayouts => _rackLayouts.AsReadOnly();

    public IReadOnlyCollection<ConfigurationSku> Skus => _skus.AsReadOnly();
    public IReadOnlyCollection<ConfigurationPallet> Pallets => _pallets.AsReadOnly();
    public IReadOnlyCollection<WarehouseConfig> WarehouseConfigs => _warehouseConfigs.AsReadOnly();
    public IReadOnlyCollection<StorageConfiguration> StorageConfigurations => _storageConfigurations.AsReadOnly();
    public IReadOnlyCollection<MheConfig> MheConfigs => _mheConfigs.AsReadOnly();

    private ConfigurationVersion() { }

    public static ConfigurationVersion Create(
        Guid configurationId,
        int versionNumber,
        string? description = null,
        bool isCurrent = true,
        string? createdBy = null)
    {
        if (configurationId == Guid.Empty)
            throw new ArgumentException("Configuration ID cannot be empty.", nameof(configurationId));
        if (versionNumber < 1)
            throw new ArgumentException("Version number must be at least 1.", nameof(versionNumber));

        return new ConfigurationVersion
        {
            Id = Guid.NewGuid(),
            ConfigurationId = configurationId,
            VersionNumber = versionNumber,
            Description = description?.Trim(),
            IsCurrent = isCurrent,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static ConfigurationVersion Rehydrate(
        Guid id,
        Guid configurationId,
        int versionNumber,
        string? description,
        bool isCurrent,
        bool isLocked,
        EnquiryStatus? status,
        DateTime createdAt,
        string? createdBy,
        IEnumerable<ConfigurationSku>? skus = null,
        IEnumerable<ConfigurationPallet>? pallets = null,
        IEnumerable<WarehouseConfig>? warehouseConfigs = null,
        IEnumerable<StorageConfiguration>? storageConfigurations = null,
        IEnumerable<MheConfig>? mheConfigs = null)
    {
        var version = new ConfigurationVersion
        {
            Id = id,
            ConfigurationId = configurationId,
            VersionNumber = versionNumber,
            Description = description,
            IsCurrent = isCurrent,
            Status  =status,
            IsLocked= isLocked,
            CreatedAt = createdAt,
            CreatedBy = createdBy
        };

        if (skus != null) version._skus.AddRange(skus);
        if (pallets != null) version._pallets.AddRange(pallets);
        if (warehouseConfigs != null) version._warehouseConfigs.AddRange(warehouseConfigs);
        if (storageConfigurations != null) version._storageConfigurations.AddRange(storageConfigurations);
        if (mheConfigs != null) version._mheConfigs.AddRange(mheConfigs);

        return version;
    }

    // ============ SKU Management ============

    public ConfigurationSku AddSku(
        string code,
        string name,
        Guid? skuTypeId = null,
        string? description = null,
        System.Text.Json.JsonDocument? attributes = null,
        int? unitsPerLayer = null,
        int? layersPerPallet = null,
        string? createdBy = null)
    {
        if (_skus.Any(s => s.Code.Equals(code.Trim().ToUpperInvariant(), StringComparison.OrdinalIgnoreCase) && s.IsActive))
            throw new InvalidOperationException($"SKU code '{code}' already exists in this version.");

        var sku = ConfigurationSku.Create(Id, code, name, skuTypeId, description, attributes, unitsPerLayer, layersPerPallet, createdBy);
        _skus.Add(sku);
        return sku;
    }

    public void RemoveSku(Guid skuId, string? updatedBy)
    {
        var sku = _skus.FirstOrDefault(s => s.Id == skuId);
        sku?.Deactivate(updatedBy);
    }

    // ============ Pallet Management ============

    public ConfigurationPallet AddPallet(
        string code,
        string name,
        Guid? palletTypeId = null,
        string? description = null,
        System.Text.Json.JsonDocument? attributes = null,
        string? createdBy = null)
    {
        if (_pallets.Any(p => p.Code.Equals(code.Trim().ToUpperInvariant(), StringComparison.OrdinalIgnoreCase) && p.IsActive))
            throw new InvalidOperationException($"Pallet code '{code}' already exists in this version.");

        var pallet = ConfigurationPallet.Create(Id, code, name, palletTypeId, description, attributes, createdBy);
        _pallets.Add(pallet);
        return pallet;
    }

    public void RemovePallet(Guid palletId, string? updatedBy)
    {
        var pallet = _pallets.FirstOrDefault(p => p.Id == palletId);
        pallet?.Deactivate(updatedBy);
    }

    // ============ Warehouse Config Management ============

    public WarehouseConfig AddWarehouseConfig(
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
        var warehouseConfig = WarehouseConfig.Create(
            Id, name, lengthM, widthM, clearHeightM, description, floorType,
            floorLoadCapacityKnM2, seismicZone, mheType, temperatureRange, createdBy);
        _warehouseConfigs.Add(warehouseConfig);
        return warehouseConfig;
    }

    public void RemoveWarehouseConfig(Guid warehouseConfigId, string? updatedBy)
    {
        var wc = _warehouseConfigs.FirstOrDefault(w => w.Id == warehouseConfigId);
        wc?.Deactivate(updatedBy);
    }

    // ============ Storage Configuration Management ============

    public StorageConfiguration AddStorageConfiguration(
        string name,
        string productGroup,
        string? description = null,
        Guid? floorId = null,
        System.Text.Json.JsonDocument? designData = null,
        string? createdBy = null)
    {
        var storageConfig = StorageConfiguration.Create(Id, name, productGroup, description, floorId, designData, createdBy);
        _storageConfigurations.Add(storageConfig);
        return storageConfig;
    }

    public void RemoveStorageConfiguration(Guid storageConfigId, string? updatedBy)
    {
        var sc = _storageConfigurations.FirstOrDefault(s => s.Id == storageConfigId);
        sc?.Deactivate(updatedBy);
    }

    // ============ MHE Configuration Management ============

    public MheConfig AddMheConfig(
        string name,
        Guid? mheTypeId = null,
        string? description = null,
        System.Text.Json.JsonDocument? attributes = null,
        string? createdBy = null)
    {
        var mheConfig = MheConfig.Create(Id, name, mheTypeId, description, attributes, createdBy);
        _mheConfigs.Add(mheConfig);
        return mheConfig;
    }

    public void RemoveMheConfig(Guid mheConfigId, string? updatedBy)
    {
        var mc = _mheConfigs.FirstOrDefault(m => m.Id == mheConfigId);
        mc?.Deactivate(updatedBy);
    }

    public void SetAsCurrent()
    {
        IsCurrent = true;
    }
    public void SeLocked()
    {
        IsLocked = true;
    }
    public void ClearCurrent()
    {
        IsCurrent = false;
    }
    public void Clearlock()
    {
        IsLocked = false;
    }
}
