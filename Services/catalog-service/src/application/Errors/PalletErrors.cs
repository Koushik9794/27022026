using GssCommon.Common;

namespace CatalogService.Application.Errors;


public static class PalletErrors
{
    public static Error CodeExists(string Name)
        => Error.Conflict("Pallet.CodeExists", $"Code '{Name}' already exists.");

    public static Error InvalidId()
        => Error.Validation("Pallet.InvalidId", "Pallet id is required in the URL. Please provide a valid id.");

    public static Error NotFound(Guid id)
        => Error.NotFound("Pallet.NotFound", $"Pallet '{id}' not found.");

    public static Error CreateFailed(string reason)
        => Error.Failure("Pallet.CreateFailed", $"Failed to create Pallet. {reason}");

    public static Error UpdateFailed(string reason)
        => Error.Failure("Pallet.UpdateFailed", $"Failed to update Pallet. {reason}");

    public static Error InvalidKey()
        => Error.Validation("Pallet.Invalidname", "Pallet Code is required.");

     public static Error MissingId()
        => Error.Validation("Pallet.InvalidId", "Pallet id is required in the URL. Please provide a valid id.");
}
