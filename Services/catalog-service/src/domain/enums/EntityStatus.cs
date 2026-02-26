namespace CatalogService.Domain.Enums;

/// <summary>
/// Defines the lifecycle status of catalog entities.
/// </summary>
public enum EntityStatus
{
    /// <summary>
    /// Entity is available for use.
    /// </summary>
    Active,

    /// <summary>
    /// Entity is temporarily disabled.
    /// </summary>
    Inactive,

    /// <summary>
    /// Entity is being phased out.
    /// </summary>
    Deprecated
}
