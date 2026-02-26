using GssCommon.Common;

namespace CatalogService.Application.Errors;

public static class ComponentNameErrors
{
    public static readonly Error NotFound = Error.NotFound("ComponentName.NotFound", "Component Name not found.");
    public static readonly Error DuplicateCode = Error.Conflict("ComponentName.DuplicateCode", "Component Name code already exists.");
}
