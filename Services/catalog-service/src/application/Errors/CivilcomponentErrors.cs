using GssCommon.Common;

namespace CatalogService.Application.Errors;


public static class CivilcomponentErrors
{
    public static Error CodeExists(string Name)
        => Error.Conflict("MHE.CodeExists", $"Code '{Name}' already exists.");

    public static Error NotFound(Guid id)
        => Error.NotFound("MHE.NotFound", $"MHE '{id}' not found.");

    public static Error CreateFailed(string reason)
        => Error.Failure("MHE.CreateFailed", $"Failed to create MHE. {reason}");
    public static Error UpdateFailed(string reason)
    => Error.Failure("MHE.UpdateFailed", $"Failed to update MHE. {reason}");

    public static Error InvalidKey()
        => Error.Validation("MHE.Invalidname", "MHE Code is required.");


}
