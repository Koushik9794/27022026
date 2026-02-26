namespace CatalogService.Domain.Enums;

/// <summary>
/// Defines the physical connection mechanism between components.
/// </summary>
public enum ConnectionType
{
    /// <summary>
    /// Teardrop beam clips into upright slots.
    /// </summary>
    HookIn,

    /// <summary>
    /// Bolted connection with fasteners.
    /// </summary>
    BoltOn,

    /// <summary>
    /// Slides into a channel or track.
    /// </summary>
    SlideIn,

    /// <summary>
    /// Spring clip attachment.
    /// </summary>
    ClipOn,

    /// <summary>
    /// Factory welded connection.
    /// </summary>
    Weld,

    /// <summary>
    /// Panel drops into beam channel.
    /// </summary>
    DropIn,

    /// <summary>
    /// Slot pattern on upright for receiving connections.
    /// </summary>
    Slot,

    /// <summary>
    /// Bolt pattern for anchor connections.
    /// </summary>
    BoltPattern
}
