using System.Text.Json;

namespace ConfigurationService.Domain.Aggregates;

/// <summary>
/// Named snapshot of an enquiry's configuration state.
/// </summary>
public sealed class EnquirySnapshot
{
    public Guid Id { get; private set; }
    public Guid EnquiryId { get; private set; }
    public string Name { get; private set; } = default!;
    public string? Description { get; private set; }
    public JsonDocument ConfigurationState { get; private set; } = default!;
    public int VersionAtSnapshot { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }

    // Private constructor for ORM
    private EnquirySnapshot() { }

    /// <summary>
    /// Factory method to create a new snapshot.
    /// </summary>
    public static EnquirySnapshot Create(
        Guid enquiryId,
        string name,
        string? description,
        JsonDocument configurationState,
        int versionAtSnapshot,
        string? createdBy = null)
    {
        if (enquiryId == Guid.Empty)
        {
            throw new ArgumentException("Enquiry ID cannot be empty.", nameof(enquiryId));
        }

        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Snapshot name cannot be empty.", nameof(name));
        }

        if (configurationState == null)
        {
            throw new ArgumentNullException(nameof(configurationState));
        }

        return new EnquirySnapshot
        {
            Id = Guid.NewGuid(),
            EnquiryId = enquiryId,
            Name = name.Trim(),
            Description = description?.Trim(),
            ConfigurationState = configurationState,
            VersionAtSnapshot = versionAtSnapshot,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

    /// <summary>
    /// Factory method for rehydration from database.
    /// </summary>
    public static EnquirySnapshot Rehydrate(
        Guid id,
        Guid enquiryId,
        string name,
        string? description,
        JsonDocument configurationState,
        int versionAtSnapshot,
        DateTime createdAt,
        string? createdBy)
    {
        return new EnquirySnapshot
        {
            Id = id,
            EnquiryId = enquiryId,
            Name = name,
            Description = description,
            ConfigurationState = configurationState,
            VersionAtSnapshot = versionAtSnapshot,
            CreatedAt = createdAt,
            CreatedBy = createdBy
        };
    }
}
