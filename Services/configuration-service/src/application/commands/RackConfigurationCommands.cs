using System;
using System.Text.Json;

namespace ConfigurationService.Application.Commands;

public record CreateRackConfigurationCommand(
    string Name,
    JsonDocument ConfigurationLayout,
    string ProductCode,
    string Scope,
    string? EnquiryId,
    string? CreatedBy,
    bool IsAdmin
);

public record UpdateRackConfigurationCommand(
    Guid Id,
    string Name,
    JsonDocument ConfigurationLayout,
    string ProductCode,
    string Scope,
    string? EnquiryId,
    string? UpdatedBy,
    bool IsAdmin
);

public record DeleteRackConfigurationCommand(
    Guid Id,
    string? UpdatedBy
);
