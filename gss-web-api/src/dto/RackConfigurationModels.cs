using System.Text.Json;

namespace GssWebApi.Dto;

public record RackConfigurationResponse(
    Guid Id,
    string Name,
    string Scope,
    Guid? EnquiryId,
    bool IsApprovedByAdmin,
    bool IsActive,
    DateTime CreatedOn,
    JsonDocument? ConfigurationLayout = null,
    string? CreatedBy = null
);

/// <summary>
/// Request DTO for creating a new rack configuration.
/// </summary>
/// <param name="Name">[REQUIRED] Name for the configuration.</param>
/// <param name="ConfigurationLayout">[REQUIRED] JSON layout data.</param>
/// <param name="ProductCode">[REQUIRED] Associated product code.</param>
/// <param name="Scope">[REQUIRED] Accessibility scope ('ENQUIRY', 'PERSONAL', 'GLOBAL').</param>
/// <param name="EnquiryId">[OPTIONAL] Associated enquiry identifier.</param>
/// <param name="CreatedBy">[OPTIONAL] User identifier.</param>
public record CreateRackConfigurationRequest(
    string Name,
    JsonDocument ConfigurationLayout,
    string ProductCode,
    string Scope, // ENQUIRY, PERSONAL, GLOBAL
    string? EnquiryId,
    string? CreatedBy = null
);

/// <summary>
/// Request DTO for updating an existing rack configuration.
/// </summary>
/// <param name="Name">[REQUIRED] Name for the configuration.</param>
/// <param name="ConfigurationLayout">[REQUIRED] JSON layout data.</param>
/// <param name="ProductCode">[REQUIRED] Associated product code.</param>
/// <param name="Scope">[REQUIRED] Accessibility scope.</param>
/// <param name="EnquiryId">[OPTIONAL] Associated enquiry identifier.</param>
/// <param name="UpdatedBy">[OPTIONAL] User identifier.</param>
public record UpdateRackConfigurationRequest(
    string Name,
    JsonDocument ConfigurationLayout,
    string ProductCode,
    string Scope,
    string? EnquiryId,
    string? UpdatedBy = null
);
