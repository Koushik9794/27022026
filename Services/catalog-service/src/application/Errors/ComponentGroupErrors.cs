using GssCommon.Common;

namespace CatalogService.Application.Errors;

public static class ComponentGroupErrors
{
    public static readonly Error NotFound = Error.NotFound("ComponentGroup.NotFound", "Component Group not found.");
    public static readonly Error DuplicateCode = Error.Conflict("ComponentGroup.DuplicateCode", "Component Group code already exists.");
}
