using GssCommon.Common;

namespace CatalogService.Application.Errors;

public static class CurrencyErrors
{
    public static Error NotFound => Error.NotFound("Currency.NotFound", "The specified currency was not found.");
    public static Error DuplicateCode => Error.Conflict("Currency.DuplicateCode", "A currency with the same code already exists.");
}
