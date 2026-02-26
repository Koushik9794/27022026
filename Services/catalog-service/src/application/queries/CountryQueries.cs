namespace CatalogService.Application.Queries;

public record GetAllCountriesQuery(bool IncludeInactive = false);
public record GetCountryByIdQuery(Guid Id);
public record GetCountryByCodeQuery(string Code);
