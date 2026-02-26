namespace CatalogService.Domain.Enums;

/// <summary>
/// Defines the type of rack bay.
/// </summary>
public enum BayType
{
    /// <summary>
    /// Starter bay with 2 independent frames.
    /// </summary>
    Starter,

    /// <summary>
    /// Add-on bay that shares one frame with the previous bay.
    /// </summary>
    AddOn
}
