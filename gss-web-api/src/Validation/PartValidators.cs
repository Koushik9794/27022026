using GssWebApi.Dto;
using FluentValidation;

namespace GssWebApi.Validation;

public class CreatePartRequestValidator : AbstractValidator<CreatePartRequest>
{
    public CreatePartRequestValidator()
    {
        RuleFor(x => x.PartCode)
            .NotEmpty().WithMessage("Part Code is required.")
            .MaximumLength(100).WithMessage("Part Code cannot exceed 100 characters.");

        RuleFor(x => x.CountryCode)
            .NotEmpty().WithMessage("Country Code is required.")
            .Length(2).WithMessage("Country Code must be exactly 2 characters.");

        RuleFor(x => x.ComponentGroupId)
            .NotEmpty().WithMessage("Component Group is required.");

        RuleFor(x => x.ComponentTypeId)
            .NotEmpty().WithMessage("Component Type is required.");

        RuleFor(x => x.UnitBasicPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit Basic Price must be zero or greater.");

        RuleFor(x => x.Cbm)
            .GreaterThanOrEqualTo(0).When(x => x.Cbm.HasValue)
            .WithMessage("CBM must be zero or greater.");
    }
}

public class UpdatePartRequestValidator : AbstractValidator<UpdatePartRequest>
{
    public UpdatePartRequestValidator()
    {
        RuleFor(x => x.ComponentGroupId)
            .NotEmpty().WithMessage("Component Group is required.");

        RuleFor(x => x.ComponentTypeId)
            .NotEmpty().WithMessage("Component Type is required.");

        RuleFor(x => x.UnitBasicPrice)
            .GreaterThanOrEqualTo(0).WithMessage("Unit Basic Price must be zero or greater.");

        RuleFor(x => x.Status)
            .NotEmpty().WithMessage("Status is required.");

        RuleFor(x => x.Cbm)
            .GreaterThanOrEqualTo(0).When(x => x.Cbm.HasValue)
            .WithMessage("CBM must be zero or greater.");
    }
}
