using GssCommon.Common;

namespace CatalogService.Application.Errors;

public static class ComponentMasterErrors
{
    public static readonly Error NotFound = Error.NotFound("ComponentMaster.NotFound", "Component Master not found.");
    public static readonly Error DuplicateCode = Error.Conflict("ComponentMaster.DuplicateCode", "Component Master with Code and Country already exists.");
}
