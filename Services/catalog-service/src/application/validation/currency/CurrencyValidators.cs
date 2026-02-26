using CatalogService.Application.Commands;
using FluentValidation;

namespace CatalogService.Application.Validation.Currency;

public class CreateCurrencyValidator : AbstractValidator<CreateCurrencyCommand>
{
    public CreateCurrencyValidator()
    {
        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Currency code is required.")
            .Length(3).WithMessage("Currency code must be exactly 3 characters.");

        RuleFor(x => x.CurrencyName)
            .NotEmpty().WithMessage("CurrencyName is required.")
            .MaximumLength(100).WithMessage("CurrencyName cannot exceed 100 characters.");
            
        RuleFor(x => x.DecimalUnit)
            .InclusiveBetween((short)0, (short)4).WithMessage("Decimal unit must be between 0 and 4.");
    }
}

public class UpdateCurrencyValidator : AbstractValidator<UpdateCurrencyCommand>
{
    public UpdateCurrencyValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        
        RuleFor(x => x.CurrencyName)
            .NotEmpty().WithMessage("CurrencyName is required.")
            .MaximumLength(100).WithMessage("CurrencyName cannot exceed 100 characters.");

        RuleFor(x => x.DecimalUnit)
            .InclusiveBetween((short)0, (short)4).WithMessage("Decimal unit must be between 0 and 4.");
    }
}
