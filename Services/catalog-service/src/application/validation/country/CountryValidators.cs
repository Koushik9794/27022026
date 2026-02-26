using CatalogService.Application.Commands;
using FluentValidation;

namespace CatalogService.Application.Validation.Country;

public class CreateCountryValidator : AbstractValidator<CreateCountryCommand>
{
    public CreateCountryValidator()
    {
        RuleFor(x => x.CountryCode)
            .NotEmpty().WithMessage("Country code is required.")
            .Length(2).WithMessage("Country code must be exactly 2 characters.");

        RuleFor(x => x.CountryName)
            .NotEmpty().WithMessage("CountryName is required.")
            .MaximumLength(100).WithMessage("CountryName cannot exceed 100 characters.");
            
        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Currency code is required.")
            .Length(3).WithMessage("Currency code must be exactly 3 characters.");
    }
}

public class UpdateCountryValidator : AbstractValidator<UpdateCountryCommand>
{
    public UpdateCountryValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        
        RuleFor(x => x.CountryName)
            .NotEmpty().WithMessage("CountryName is required.")
            .MaximumLength(100).WithMessage("CountryName cannot exceed 100 characters.");
            
        RuleFor(x => x.CurrencyCode)
            .NotEmpty().WithMessage("Currency code is required.")
            .Length(3).WithMessage("Currency code must be exactly 3 characters.");
    }
}
