using CatalogService.Application.Commands;
using CatalogService.Application.Queries;
using CatalogService.Application.Errors;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.Exchange;

public class ExchangeRateHandlers
{
    private readonly IExchangeRateRepository _exchangeRateRepository;
    private readonly ICurrencyRepository _currencyRepository;

    public ExchangeRateHandlers(
        IExchangeRateRepository exchangeRateRepository,
        ICurrencyRepository currencyRepository)
    {
        _exchangeRateRepository = exchangeRateRepository;
        _currencyRepository = currencyRepository;
    }

    public async Task<Result<Guid>> Handle(CreateExchangeRateCommand command)
    {
        // Validate currencies
        var baseCurrency = await _currencyRepository.GetByCodeAsync(command.BaseCurrency);
        if (baseCurrency == null)
            return Result.Failure<Guid>(CurrencyErrors.NotFound);

        var quoteCurrency = await _currencyRepository.GetByCodeAsync(command.QuoteCurrency);
        if (quoteCurrency == null)
            return Result.Failure<Guid>(CurrencyErrors.NotFound);

        // Overlap Check
        if (await _exchangeRateRepository.ExistsOverlappingAsync(
            command.BaseCurrency, command.QuoteCurrency, command.ValidFrom, command.ValidEnd))
        {
            return Result.Failure<Guid>(ExchangeRateErrors.OverlappingPeriod);
        }

        try
        {
            var exchangeRate = ExchangeRate.Create(
                command.BaseCurrency,
                command.QuoteCurrency,
                command.Rate,
                command.ValidFrom,
                command.ValidEnd,
                command.CreatedBy
            );

            await _exchangeRateRepository.CreateAsync(exchangeRate);
            return Result.Success(exchangeRate.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Guid>(Error.Validation("ExchangeRate.Validation", ex.Message));
        }
    }

    public async Task<Result<bool>> Handle(UpdateExchangeRateCommand command)
    {
        var exchangeRate = await _exchangeRateRepository.GetByIdAsync(command.Id);
        if (exchangeRate == null)
        {
            return Result.Failure<bool>(ExchangeRateErrors.NotFound);
        }

        // Overlap Check
        if (await _exchangeRateRepository.ExistsOverlappingAsync(
            exchangeRate.BaseCurrency, exchangeRate.QuoteCurrency, command.ValidFrom, command.ValidEnd, command.Id))
        {
            return Result.Failure<bool>(ExchangeRateErrors.OverlappingPeriod);
        }

        try
        {
            exchangeRate.Update(
                command.Rate,
                command.ValidFrom,
                command.ValidEnd,
                command.UpdatedBy
            );

            if (command.IsActive) exchangeRate.Activate();
            else exchangeRate.Deactivate();

            await _exchangeRateRepository.UpdateAsync(exchangeRate);
            return Result.Success(true);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<bool>(Error.Validation("ExchangeRate.Validation", ex.Message));
        }
    }

    public async Task<Result<bool>> Handle(DeleteExchangeRateCommand command)
    {
        var exchangeRate = await _exchangeRateRepository.GetByIdAsync(command.Id);
        if (exchangeRate == null)
        {
            return Result.Failure<bool>(ExchangeRateErrors.NotFound);
        }

        exchangeRate.Delete(command.DeletedBy);
        await _exchangeRateRepository.UpdateAsync(exchangeRate);
        return Result.Success(true);
    }

    public async Task<IEnumerable<ExchangeRate>> Handle(GetAllExchangeRatesQuery query)
    {
        return await _exchangeRateRepository.GetAllAsync(query.IncludeInactive);
    }

    public async Task<ExchangeRate?> Handle(GetExchangeRateByIdQuery query)
    {
        return await _exchangeRateRepository.GetByIdAsync(query.Id);
    }

    public async Task<ExchangeRate?> Handle(GetLatestExchangeRateQuery query)
    {
        return await _exchangeRateRepository.GetLatestRateAsync(query.BaseCurrency, query.QuoteCurrency);
    }
}
