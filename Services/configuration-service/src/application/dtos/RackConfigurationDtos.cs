using System;
using System.Text.Json;

namespace ConfigurationService.Application.Dtos;

public record CreateRackConfigurationRequest(
    string Name,
    JsonDocument ConfigurationLayout,
    string ProductCode,
    string Scope, // ENQUIRY, PERSONAL, GLOBAL
    string? EnquiryId,
    string? CreatedBy = null
);

public record UpdateRackConfigurationRequest(
    string Name,
    JsonDocument ConfigurationLayout,
    string ProductCode,
    string Scope,
    string? EnquiryId,
    string? UpdatedBy = null
);

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
