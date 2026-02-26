namespace CatalogService.Application.Queries;

public record GetAllExchangeRatesQuery(bool IncludeInactive = false);
public record GetExchangeRateByIdQuery(Guid Id);
public record GetLatestExchangeRateQuery(string BaseCurrency, string QuoteCurrency);
