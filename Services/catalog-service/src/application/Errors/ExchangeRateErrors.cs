using GssCommon.Common;

namespace CatalogService.Application.Errors;

public static class ExchangeRateErrors
{
    public static Error NotFound => Error.NotFound("ExchangeRate.NotFound", "The specified exchange rate was not found.");
    public static Error OverlappingPeriod => Error.Conflict("ExchangeRate.OverlappingPeriod", "An exchange rate for this pair and period already exists.");
}
