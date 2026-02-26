using Azure.Core;
using GssCommon.Common;
using Spectre.Console;

namespace ConfigurationService.Application.Errors;

public static class EnquiryErrors
{
    public static Error NoExists(string? Name)
        => Error.Conflict("Enquiry.NoExists", $"Enquiry Number '{Name}' already exists.");

    public static Error ExtExists(string? Name)
    => Error.Conflict("Enquiry.external", $"Enquiry with external ID '{Name}' already exists.");

    public static Error NotFound(Guid id)
        => Error.NotFound("Enquiry.NotFound", $"Enquiry '{id}' not found.");

    public static Error CreateFailed(string reason)
        => Error.Failure("Enquiry.CreateFailed", $"Failed to create Enquiry. {reason}");
    public static Error UpdateFailed(string reason)
    => Error.Failure("Enquiry.UpdateFailed", $"Failed to update Enquiry. {reason}");

    public static Error InvalidKey()
        => Error.Validation("Enquiry.Invalidname", "Enquiry Code is required.");
    public static Error InvalidStatus(string Status)
    => Error.Validation("Enquiry.InvalidStatus", $"Invalid status : {Status}");

    public static Error ConfigNotFounds(Guid id)
     => Error.NotFound("Config.NotFound", $"Enquiry '{id}' not found.");

    public static Error VersionNotFounds(int VersionNumber)
 => Error.NotFound("Config.Version", $"Version {VersionNumber} not found.");
}
