using CatalogService.Application.Commands;
using FluentValidation;

namespace CatalogService.Application.Validation.ComponentMaster;

public class CreateComponentMasterCommandValidator : AbstractValidator<CreateComponentMasterCommand>
{
    public CreateComponentMasterCommandValidator()
    {
        RuleFor(x => x.ComponentMasterCode)
            .NotEmpty().WithMessage("Component Master Code is required.")
            .MaximumLength(100).WithMessage("Component Master Code cannot exceed 100 characters.");

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

        RuleFor(x => x.ShortDescription)
            .MaximumLength(255).WithMessage("Short Description cannot exceed 255 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");
    }
}

public class UpdateComponentMasterCommandValidator : AbstractValidator<UpdateComponentMasterCommand>
{
    public UpdateComponentMasterCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

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

        RuleFor(x => x.ShortDescription)
            .MaximumLength(255).WithMessage("Short Description cannot exceed 255 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(1000).WithMessage("Description cannot exceed 1000 characters.");
    }
}
