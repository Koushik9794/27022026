namespace CatalogService.Domain.Enums;

/// <summary>
/// Defines the role of a component interface in a connection.
/// </summary>
public enum InterfaceRole
{
    /// <summary>
    /// Component that connects INTO something (e.g., beam hooks into upright).
    /// </summary>
    Plug,

    /// <summary>
    /// Component that RECEIVES a connection (e.g., upright slots receive beam hooks).
    /// </summary>
    Socket,

    /// <summary>
    /// Can be either plug or socket (e.g., row spacer connects both ways).
    /// </summary>
    Both
}
