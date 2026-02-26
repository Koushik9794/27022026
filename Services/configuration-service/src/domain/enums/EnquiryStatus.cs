namespace ConfigurationService.Domain.Enums;

/// <summary>
/// Lifecycle status of an enquiry configuration.
/// </summary>
public enum EnquiryStatus
{
    /// <summary>
    /// Initial state, configuration in progress.
    /// </summary>
    Draft,

    /// <summary>
    /// Active design work in progress.
    /// </summary>
    InProgress,

    /// <summary>
    /// Configuration submitted for review/approval.
    /// </summary>
    Submitted,

    /// <summary>
    /// Enquiry converted to project/order.
    /// </summary>
    Converted,

    /// <summary>
    /// Enquiry archived (no longer active).
    /// </summary>
    Archived
}
