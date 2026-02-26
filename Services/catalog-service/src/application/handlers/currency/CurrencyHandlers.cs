using CatalogService.Application.Commands;
using CatalogService.Application.Queries;
using CatalogService.Application.Errors;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.Currency;

public class CurrencyHandlers
{
    private readonly ICurrencyRepository _currencyRepository;

    public CurrencyHandlers(ICurrencyRepository currencyRepository)
    {
        _currencyRepository = currencyRepository;
    }

    public async Task<Result<Guid>> Handle(CreateCurrencyCommand command)
    {
        if (await _currencyRepository.ExistsByCodeAsync(command.CurrencyCode))
        {
            return Result.Failure<Guid>(CurrencyErrors.DuplicateCode);
        }

        try
        {
            var currency = Domain.Aggregates.Currency.Create(
                command.CurrencyCode,
                command.CurrencyName,
                command.CurrencyValue,
                command.DecimalUnit,
                command.CreatedBy
            );

            await _currencyRepository.CreateAsync(currency);
            return Result.Success(currency.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Guid>(Error.Validation("Currency.Validation", ex.Message));
        }
    }

    public async Task<Result<bool>> Handle(UpdateCurrencyCommand command)
    {
        var currency = await _currencyRepository.GetByIdAsync(command.Id);
        if (currency == null)
        {
            return Result.Failure<bool>(CurrencyErrors.NotFound);
        }

        try
        {
            currency.Update(
                command.CurrencyName,
                command.CurrencyValue,
                command.DecimalUnit,
                command.UpdatedBy
            );

            if (command.IsActive) currency.Activate();
            else currency.Deactivate();

            await _currencyRepository.UpdateAsync(currency);
            return Result.Success(true);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<bool>(Error.Validation("Currency.Validation", ex.Message));
        }
    }

    public async Task<Result<bool>> Handle(DeleteCurrencyCommand command)
    {
        var currency = await _currencyRepository.GetByIdAsync(command.Id);
        if (currency == null)
        {
            return Result.Failure<bool>(CurrencyErrors.NotFound);
        }

        currency.Delete(command.DeletedBy);
        await _currencyRepository.UpdateAsync(currency);
        return Result.Success(true);
    }

    public async Task<IEnumerable<Domain.Aggregates.Currency>> Handle(GetAllCurrenciesQuery query)
    {
        return await _currencyRepository.GetAllAsync(query.IncludeInactive);
    }

    public async Task<Domain.Aggregates.Currency?> Handle(GetCurrencyByIdQuery query)
    {
        return await _currencyRepository.GetByIdAsync(query.Id);
    }

    public async Task<Domain.Aggregates.Currency?> Handle(GetCurrencyByCodeQuery query)
    {
        return await _currencyRepository.GetByCodeAsync(query.Code);
    }
}
