using GssCommon.Common;

namespace CatalogService.Application.Errors;

public static class ComponentTypeErrors
{
    public static readonly Error NotFound = Error.NotFound("ComponentType.NotFound", "Component type not found.");
    public static readonly Error DuplicateCode = Error.Conflict("ComponentType.DuplicateCode", "Component type with this code already exists.");
    public static readonly Error CategoryNotFound = Error.NotFound("ComponentType.CategoryNotFound", "Component category not found.");
    public static readonly Error ParentNotFound = Error.NotFound("ComponentType.ParentNotFound", "Parent component type not found.");
}
