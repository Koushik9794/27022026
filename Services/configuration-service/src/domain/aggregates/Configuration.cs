using ConfigurationService.Domain.Aggregates;

namespace ConfigurationService.Domain.Aggregates;

/// <summary>
/// Configuration aggregate - represents a design variant within an Enquiry.
/// An Enquiry can have multiple Configurations (e.g., "Option A", "Option B").
/// Each Configuration can have multiple Versions (version history).
/// </summary>
public sealed class Configuration
{
    public Guid Id { get; private set; }
    public Guid EnquiryId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsPrimary { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    // Version history - Configuration contains multiple versions
    private readonly List<ConfigurationVersion> _versions = new();
    public IReadOnlyCollection<ConfigurationVersion> Versions => _versions.AsReadOnly();

    private readonly List<CivilLayout> _civilLayouts = new();
    public IReadOnlyCollection<CivilLayout> civilLayouts => _civilLayouts.AsReadOnly();

    private readonly List<RackLayout> _rackLayouts = new();
    public IReadOnlyCollection<RackLayout> rackLayouts => _rackLayouts.AsReadOnly();

    private Configuration() { }

    public static Configuration Create(
        Guid enquiryId,
        string name,
        string? description = null,
        bool isPrimary = false,
        string? createdBy = null)
    {
        ValidateEnquiryId(enquiryId);
        ValidateName(name);

        var config = new Configuration
        {
            Id = Guid.NewGuid(),
            EnquiryId = enquiryId,
            Name = name.Trim(),
            Description = description?.Trim(),
            IsActive = true,
            IsPrimary = isPrimary,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        // Create initial version
        var version = ConfigurationVersion.Create(config.Id, 1, "Initial version", true, createdBy);
        config._versions.Add(version);

        return config;
    }

    public static Configuration Rehydrate(
        Guid id,
        Guid enquiryId,
        string name,
        string? description,
        bool isActive,
        bool isPrimary,
        DateTime createdAt,
        string? createdBy,
        DateTime? updatedAt,
        string? updatedBy,
        IEnumerable<ConfigurationVersion>? versions = null,
        IEnumerable<CivilLayout>? civil = null,
        IEnumerable<RackLayout>? racks = null)
    {
        var config = new Configuration
        {
            Id = id,
            EnquiryId = enquiryId,
            Name = name,
            Description = description,
            IsActive = isActive,
            IsPrimary = isPrimary,
            CreatedAt = createdAt,
            CreatedBy = createdBy,
            UpdatedAt = updatedAt,
            UpdatedBy = updatedBy
        };

        if (versions != null) config._versions.AddRange(versions);
        if (civil != null) config._civilLayouts.AddRange(civil);
        if (racks != null) config._rackLayouts.AddRange(racks);
        return config;
    }

    // ============ Version Management ============

    public ConfigurationVersion GetCurrentVersion()
    {
        return _versions.FirstOrDefault(v => v.IsCurrent) ?? _versions.OrderByDescending(v => v.VersionNumber).First();
    }

    public ConfigurationVersion? GetVersion(int versionNumber)
    {
        return _versions.FirstOrDefault(v => v.VersionNumber == versionNumber);
    }

    public ConfigurationVersion CreateNewVersion(string? description = null, string? createdBy = null)
    {
        // Clear current flag from existing versions
        foreach (var v in _versions.Where(v => v.IsCurrent))
        {
            v.ClearCurrent();
        }

        var nextVersionNumber = _versions.Max(v => v.VersionNumber) + 1;
        var version = ConfigurationVersion.Create(Id, nextVersionNumber, description, true, createdBy);
        _versions.Add(version);

        UpdatedBy = createdBy;
        UpdatedAt = DateTime.UtcNow;

        return version;
    }

    public void SetCurrentVersion(int versionNumber, string? updatedBy)
    {
        var version = _versions.FirstOrDefault(v => v.VersionNumber == versionNumber);
        if (version == null)
            throw new InvalidOperationException($"Version {versionNumber} not found.");

        foreach (var v in _versions)
        {
            if (v.IsCurrent) v.ClearCurrent();
        }
        version.SetAsCurrent();

        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    // ============ Configuration Management ============

    public void Update(string name, string? description, string? updatedBy)
    {
        ValidateName(name);
        Name = name.Trim();
        Description = description?.Trim();
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void SetAsPrimary(string? updatedBy)
    {
        IsPrimary = true;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ClearPrimary(string? updatedBy)
    {
        IsPrimary = false;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate(string? updatedBy)
    {
        IsActive = false;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateEnquiryId(Guid enquiryId)
    {
        if (enquiryId == Guid.Empty)
            throw new ArgumentException("Enquiry ID cannot be empty.", nameof(enquiryId));
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("Configuration name cannot be empty.", nameof(name));
        if (name.Length > 255)
            throw new ArgumentException("Configuration name cannot exceed 255 characters.", nameof(name));
    }
}
