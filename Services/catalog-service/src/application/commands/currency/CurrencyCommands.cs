namespace CatalogService.Application.Commands;

public record CreateCurrencyCommand(
    string CurrencyCode,
    string CurrencyName,
    string? CurrencyValue,
    short DecimalUnit,
    string? CreatedBy
);

public record UpdateCurrencyCommand(
    Guid Id,
    string CurrencyName,
    string? CurrencyValue,
    short DecimalUnit,
    bool IsActive,
    string? UpdatedBy
);

public record DeleteCurrencyCommand(Guid Id, string? DeletedBy);
