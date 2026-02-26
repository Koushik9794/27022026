using GssCommon.Common;

namespace CatalogService.Application.Errors;

public static class AttributeDefinitionErrors
{
    public static Error AlreadyExists(string key)
        => Error.Conflict("AttributeDefinition.AlreadyExists", $"Attribute with key '{key}' already exists.");

    public static Error NotFound(Guid id)
        => Error.NotFound("AttributeDefinition.NotFound", $"Attribute definition '{id}' not found.");

    public static Error CreateFailed(string reason)
        => Error.Failure("AttributeDefinition.CreateFailed", $"Failed to create attribute definition. {reason}");

    public static Error UpdateFailed(string reason)
        => Error.Failure("AttributeDefinition.UpdateFailed", $"Failed to update attribute definition. {reason}");

    public static Error DeleteFailed(string reason)
        => Error.Failure("AttributeDefinition.DeleteFailed", $"Failed to delete attribute definition. {reason}");
}
