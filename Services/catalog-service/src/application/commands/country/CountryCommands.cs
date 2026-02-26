namespace CatalogService.Application.Commands;

public record CreateCountryCommand(
    string CountryCode,
    string CountryName,
    string CurrencyCode,
    string? CreatedBy
);

public record UpdateCountryCommand(
    Guid Id,
    string CountryName,
    string CurrencyCode,
    bool IsActive,
    string? UpdatedBy
);

public record DeleteCountryCommand(Guid Id, string? DeletedBy);
