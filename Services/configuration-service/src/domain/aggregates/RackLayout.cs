using System.Text.Json;

namespace ConfigurationService.Domain.Aggregates;

/// <summary>
/// Rack layout for a specific configuration version, linked to a civil layout.
/// Stores the storage/rack placement data in JSON format.
/// </summary>
public sealed class RackLayout
{
    public Guid Id { get; private set; }
    public Guid CivilLayoutId { get; private set; }
    public Guid ConfigurationVersionId { get; private set; }
    public string? RackJson { get; private set; }
    public JsonDocument? ConfigurationLayout { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private RackLayout() { }

    public static RackLayout Create(
        Guid civilLayoutId,
        Guid configurationVersionId,
        string? rackJson,
        JsonDocument? configurationLayout = null,
        string? createdBy = null)
    {
        if (civilLayoutId == Guid.Empty)
            throw new ArgumentException("Civil Layout ID cannot be empty.", nameof(civilLayoutId));
        if (configurationVersionId == Guid.Empty)
            throw new ArgumentException("Configuration Version ID cannot be empty.", nameof(configurationVersionId));

        return new RackLayout
        {
            Id = Guid.NewGuid(),
            CivilLayoutId = civilLayoutId,
            ConfigurationVersionId = configurationVersionId,
            RackJson = rackJson,
            ConfigurationLayout = configurationLayout,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    public static RackLayout Rehydrate(
        Guid id,
        Guid civilLayoutId,
        Guid configurationVersionId,
        string? rackJson,
        JsonDocument? configurationLayout,
        DateTime createdAt,
        string? createdBy,
        DateTime? updatedAt,
        string? updatedBy)
    {
        return new RackLayout
        {
            Id = id,
            CivilLayoutId = civilLayoutId,
            ConfigurationVersionId = configurationVersionId,
            RackJson = rackJson,
            ConfigurationLayout = configurationLayout,
            CreatedAt = createdAt,
            CreatedBy = createdBy,
            UpdatedAt = updatedAt,
            UpdatedBy = updatedBy
        };
    }

    public void Update(string? rackJson, JsonDocument? configurationLayout, string? updatedBy)
    {
        RackJson = rackJson;
        ConfigurationLayout = configurationLayout;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }
}

