using GssCommon.Common;

namespace CatalogService.Application.Errors;


public static class AttributeError
{
    public static Error CodeExists(string Name)
        => Error.Conflict("Attribute.CodeExists", $"Code '{Name}' already exists.");

    public static Error NotFound(Guid id)
        => Error.NotFound("Attribute.NotFound", $"MHE '{id}' not found.");

    public static Error CreateFailed(string reason)
        => Error.Failure("Attribute.CreateFailed", $"Failed to create MHE. {reason}");
    public static Error UpdateFailed(string reason)
    => Error.Failure("Attribute.UpdateFailed", $"Failed to update MHE. {reason}");

    public static Error InvalidKey()
        => Error.Validation("Attribute.Invalidname", "Attribute key is required.");


}
