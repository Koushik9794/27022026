using CatalogService.Application.Commands;
using FluentValidation;

namespace CatalogService.Application.Validation.LoadChart;

public class CreateLoadChartCommandValidator : AbstractValidator<CreateLoadChartCommand>
{
    public CreateLoadChartCommandValidator()
    {
        RuleFor(x => x.ProductGroupId)
            .NotEmpty().WithMessage("Product Group is required.");

        RuleFor(x => x.ChartType)
            .NotEmpty().WithMessage("Chart Type is required.")
            .MaximumLength(50).WithMessage("Chart Type cannot exceed 50 characters.");

        RuleFor(x => x.ComponentCode)
            .NotEmpty().WithMessage("Component Code is required.")
            .MaximumLength(100).WithMessage("Component Code cannot exceed 100 characters.");

        RuleFor(x => x.ComponentTypeId)
            .NotEmpty().WithMessage("Component Type is required.");
    }
}

public class UpdateLoadChartCommandValidator : AbstractValidator<UpdateLoadChartCommand>
{
    public UpdateLoadChartCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.ProductGroupId)
            .NotEmpty().WithMessage("Product Group is required.");

        RuleFor(x => x.ChartType)
            .NotEmpty().WithMessage("Chart Type is required.")
            .MaximumLength(50).WithMessage("Chart Type cannot exceed 50 characters.");

        RuleFor(x => x.ComponentCode)
            .NotEmpty().WithMessage("Component Code is required.")
            .MaximumLength(100).WithMessage("Component Code cannot exceed 100 characters.");

        RuleFor(x => x.ComponentTypeId)
            .NotEmpty().WithMessage("Component Type is required.");
    }
}
