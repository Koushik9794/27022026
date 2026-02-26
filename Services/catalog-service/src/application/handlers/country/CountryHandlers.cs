using CatalogService.Application.Commands;
using CatalogService.Application.Queries;
using CatalogService.Application.Errors;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.Country;

public class CountryHandlers
{
    private readonly ICountryRepository _countryRepository;
    private readonly ICurrencyRepository _currencyRepository;

    public CountryHandlers(
        ICountryRepository countryRepository,
        ICurrencyRepository currencyRepository)
    {
        _countryRepository = countryRepository;
        _currencyRepository = currencyRepository;
    }

    public async Task<Result<Guid>> Handle(CreateCountryCommand command)
    {
        if (await _countryRepository.ExistsByCodeAsync(command.CountryCode))
        {
            return Result.Failure<Guid>(CountryErrors.DuplicateCode);
        }

        // Validate currency existence
        var currency = await _currencyRepository.GetByCodeAsync(command.CurrencyCode);
        if (currency == null)
        {
            return Result.Failure<Guid>(CurrencyErrors.NotFound);
        }

        try
        {
            var country = Domain.Aggregates.Country.Create(
                command.CountryCode,
                command.CountryName,
                command.CurrencyCode,
                command.CreatedBy
            );

            await _countryRepository.CreateAsync(country);
            return Result.Success(country.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Guid>(Error.Validation("Country.Validation", ex.Message));
        }
    }

    public async Task<Result<bool>> Handle(UpdateCountryCommand command)
    {
        var country = await _countryRepository.GetByIdAsync(command.Id);
        if (country == null)
        {
            return Result.Failure<bool>(CountryErrors.NotFound);
        }

        // Validate currency existence
        var currency = await _currencyRepository.GetByCodeAsync(command.CurrencyCode);
        if (currency == null)
        {
            return Result.Failure<bool>(CurrencyErrors.NotFound);
        }

        try
        {
            country.Update(
                command.CountryName,
                command.CurrencyCode,
                command.UpdatedBy
            );

            if (command.IsActive) country.Activate();
            else country.Deactivate();

            await _countryRepository.UpdateAsync(country);
            return Result.Success(true);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<bool>(Error.Validation("Country.Validation", ex.Message));
        }
    }

    public async Task<Result<bool>> Handle(DeleteCountryCommand command)
    {
        var country = await _countryRepository.GetByIdAsync(command.Id);
        if (country == null)
        {
            return Result.Failure<bool>(CountryErrors.NotFound);
        }

        country.Delete(command.DeletedBy);
        await _countryRepository.UpdateAsync(country);
        return Result.Success(true);
    }

    public async Task<IEnumerable<Domain.Aggregates.Country>> Handle(GetAllCountriesQuery query)
    {
        return await _countryRepository.GetAllAsync(query.IncludeInactive);
    }

    public async Task<Domain.Aggregates.Country?> Handle(GetCountryByIdQuery query)
    {
        return await _countryRepository.GetByIdAsync(query.Id);
    }

    public async Task<Domain.Aggregates.Country?> Handle(GetCountryByCodeQuery query)
    {
        return await _countryRepository.GetByCodeAsync(query.Code);
    }
}
