namespace CatalogService.Application.Queries;

public record GetAllCurrenciesQuery(bool IncludeInactive = false);
public record GetCurrencyByIdQuery(Guid Id);
public record GetCurrencyByCodeQuery(string Code);
