namespace CatalogService.Application.Commands;

public record CreateExchangeRateCommand(
    string BaseCurrency,
    string QuoteCurrency,
    decimal Rate,
    DateTime ValidFrom,
    DateTime? ValidEnd,
    string? CreatedBy
);

public record UpdateExchangeRateCommand(
    Guid Id,
    decimal Rate,
    DateTime ValidFrom,
    DateTime? ValidEnd,
    bool IsActive,
    string? UpdatedBy
);

public record DeleteExchangeRateCommand(Guid Id, string? DeletedBy);
