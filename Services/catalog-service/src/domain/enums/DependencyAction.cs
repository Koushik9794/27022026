namespace CatalogService.Domain.Enums;

/// <summary>
/// Defines the action type for contextual dependencies.
/// </summary>
public enum DependencyAction
{
    /// <summary>
    /// Add a new component when condition is met.
    /// </summary>
    Add,

    /// <summary>
    /// Upgrade an existing component to a higher-spec version.
    /// </summary>
    Upgrade,

    /// <summary>
    /// Require a specific component to be present.
    /// </summary>
    Require
}
