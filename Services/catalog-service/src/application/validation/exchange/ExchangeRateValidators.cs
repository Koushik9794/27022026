using CatalogService.Application.Commands;
using FluentValidation;

namespace CatalogService.Application.Validation.Exchange;

public class CreateExchangeRateValidator : AbstractValidator<CreateExchangeRateCommand>
{
    public CreateExchangeRateValidator()
    {
        RuleFor(x => x.BaseCurrency)
            .NotEmpty().WithMessage("Base currency is required.")
            .Length(3).WithMessage("Base currency must be 3 characters.");

        RuleFor(x => x.QuoteCurrency)
            .NotEmpty().WithMessage("Quote currency is required.")
            .Length(3).WithMessage("Quote currency must be 3 characters.");

        RuleFor(x => x.BaseCurrency)
            .NotEqual(x => x.QuoteCurrency).WithMessage("Base and Quote currencies cannot be the same.");

        RuleFor(x => x.Rate)
            .GreaterThan(0).WithMessage("Exchange rate must be greater than zero.");

        RuleFor(x => x.ValidFrom)
            .NotEmpty().WithMessage("ValidFrom is required.");

        When(x => x.ValidEnd.HasValue, () =>
        {
            RuleFor(x => x.ValidEnd)
                .GreaterThanOrEqualTo(x => x.ValidFrom).WithMessage("ValidEnd cannot be before ValidFrom.");
        });
    }
}

public class UpdateExchangeRateValidator : AbstractValidator<UpdateExchangeRateCommand>
{
    public UpdateExchangeRateValidator()
    {
        RuleFor(x => x.Id).NotEmpty();

        RuleFor(x => x.Rate)
            .GreaterThan(0).WithMessage("Exchange rate must be greater than zero.");

        RuleFor(x => x.ValidFrom)
            .NotEmpty().WithMessage("ValidFrom is required.");

        When(x => x.ValidEnd.HasValue, () =>
        {
            RuleFor(x => x.ValidEnd)
                .GreaterThanOrEqualTo(x => x.ValidFrom).WithMessage("ValidEnd cannot be before ValidFrom.");
        });
    }
}
