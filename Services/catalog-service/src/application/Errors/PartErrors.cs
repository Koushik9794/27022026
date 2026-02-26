using GssCommon.Common;

namespace CatalogService.Application.Errors;

public static class PartErrors
{
    public static readonly Error NotFound = Error.NotFound("Part.NotFound", "Part not found.");
    public static readonly Error DuplicateCode = Error.Conflict("Part.DuplicateCode", "Part with Code and Country already exists.");
}
