using System;
using GssCommon.Common;

namespace ConfigurationService.application.errors;

public static class RackConfigurationErrors
{
    public static Error NotFound(Guid id)
        => Error.NotFound("RackConfiguration.NotFound", $"Rack configuration '{id}' not found.");

    public static Error CreateFailed(string reason)
        => Error.Failure("RackConfiguration.CreateFailed", reason);

    public static Error InvalidEnquiryId(string? id)
        => Error.Validation("RackConfiguration.InvalidEnquiryId", $"'{id}' is not a valid enquiry ID.");

    public static Error EnquiryNotFound(Guid id)
        => Error.NotFound("RackConfiguration.EnquiryNotFound", $"Enquiry '{id}' not found.");

    public static Error UpdateFailed(string reason)
        => Error.Failure("RackConfiguration.UpdateFailed", reason);
}
