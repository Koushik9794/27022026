using GssCommon.Common;

namespace CatalogService.Application.Errors;

public static class CountryErrors
{
    public static Error NotFound => Error.NotFound("Country.NotFound", "The specified country was not found.");
    public static Error DuplicateCode => Error.Conflict("Country.DuplicateCode", "A country with the same code already exists.");
}
