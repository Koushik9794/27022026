using GssCommon.Common;

namespace GssCommon.Common;

public static class SkuErrors
{
    public static Error CodeExists(string code)
        => Error.Conflict("SKU.CodeExists", $"Code '{code}' already exists.");

    public static Error NotFound(Guid id)
        => Error.NotFound("SKU.NotFound", $"SKU '{id}' not found.");

    public static Error CreateFailed(string reason)
        => Error.Failure("SKU.CreateFailed", $"Failed to create SKU. {reason}");

    public static Error UpdateFailed(string reason)
        => Error.Failure("SKU.UpdateFailed", $"Failed to update SKU. {reason}");

    public static Error InvalidId()
        => Error.Validation("SKU.InvalidId", "invalid id or null id not valid");

    public static Error MissingId()
        => Error.Validation("SKU.InvalidId", "SKU id is required in the URL. Please provide a valid id.");

    public static Error InvalidCode()
        => Error.Validation("SKU.InvalidCode", "invalid code or null code not valid");

    public static Error InvalidName()
        => Error.Validation("SKU.InvalidName", "invalid name or null name not valid");

    public static Error InvalidAttributeSchema()
        => Error.Validation("SKU.InvalidAttributeSchema", "invalid attributeschema or null attributeschema not valid");
}
