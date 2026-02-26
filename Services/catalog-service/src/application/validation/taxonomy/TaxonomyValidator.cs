using System.Text.Json;
using CatalogService.Application.Commands.Taxonomy;
using CatalogService.Domain.Enums;
using CatalogService.Application.Validation.Common;
using FluentValidation;

namespace CatalogService.Application.Validation.Taxonomy;

// Component Type Validators
public class CreateComponentTypeValidator : AbstractValidator<CreateComponentTypeCommand>
{
    public CreateComponentTypeValidator(IAttributesJsonValidator attrsValidator)
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code cannot exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(120).WithMessage("Name cannot exceed 120 characters.");

        RuleFor(x => x.ComponentGroupCode)
            .NotEmpty().WithMessage("Component Group Code is required.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.AttributeSchema)
            .CustomAsync(async (json, ctx, ct) =>
            {
                if (json != null)
                {
                    var failures = await attrsValidator.ValidateAsync(AttributeScreen.CT.ToString(), json, ct);
                    foreach (var (path, message) in failures)
                        ctx.AddFailure(path, message);
                }
            });
    }
}

public class UpdateComponentTypeValidator : AbstractValidator<UpdateComponentTypeCommand>
{
    public UpdateComponentTypeValidator(IAttributesJsonValidator attrsValidator)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(120).WithMessage("Name cannot exceed 120 characters.");

        RuleFor(x => x.ComponentGroupCode)
            .NotEmpty().WithMessage("Component Group Code is required.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");

        RuleFor(x => x.AttributeSchema)
            .CustomAsync(async (json, ctx, ct) =>
            {
                if (json != null)
                {
                    var failures = await attrsValidator.ValidateAsync(AttributeScreen.CT.ToString(), json, ct);
                    foreach (var (path, message) in failures)
                        ctx.AddFailure(path, message);
                }
            });
    }
}

// Product Group Validators
public class CreateProductGroupValidator : AbstractValidator<CreateProductGroupCommand>
{
    public CreateProductGroupValidator()
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code cannot exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(120).WithMessage("Name cannot exceed 120 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}

public class UpdateProductGroupValidator : AbstractValidator<UpdateProductGroupCommand>
{
    public UpdateProductGroupValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(120).WithMessage("Name cannot exceed 120 characters.");

        RuleFor(x => x.Description)
            .MaximumLength(500).WithMessage("Description cannot exceed 500 characters.");
    }
}

// Warehouse Type Validators
public class CreateWarehouseTypeValidator : AbstractValidator<CreateWarehouseTypeCommand>
{
    public CreateWarehouseTypeValidator(IAttributesJsonValidator attrsValidator)
    {
        RuleFor(x => x.name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(120).WithMessage("Name cannot exceed 120 characters.");

        RuleFor(x => x.label)
            .NotEmpty().WithMessage("Label is required.")
            .MaximumLength(120).WithMessage("Label cannot exceed 120 characters.");

        RuleFor(x => x.attributes)
            .CustomAsync(async (json, ctx, ct) =>
            {
                if (json != null)
                {
                    var failures = await attrsValidator.ValidateAsync(AttributeScreen.WT.ToString(), json, ct);
                    foreach (var (path, message) in failures)
                        ctx.AddFailure(path, message);
                }
            });
    }
}

public class UpdateWarehouseTypeValidator : AbstractValidator<UpdateWarehouseTypeCommand>
{
    public UpdateWarehouseTypeValidator(IAttributesJsonValidator attrsValidator)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(120).WithMessage("Name cannot exceed 120 characters.");

        RuleFor(x => x.label)
            .NotEmpty().WithMessage("Label is required.")
            .MaximumLength(120).WithMessage("Label cannot exceed 120 characters.");

        RuleFor(x => x.attributes)
            .CustomAsync(async (json, ctx, ct) =>
            {
                if (json != null)
                {
                    var failures = await attrsValidator.ValidateAsync(AttributeScreen.WT.ToString(), json, ct);
                    foreach (var (path, message) in failures)
                        ctx.AddFailure(path, message);
                }
            });
    }
}

// Civil Component Validators
public class CreateCivilComponentValidator : AbstractValidator<CreateCivilComponentCommand>
{
    public CreateCivilComponentValidator(IAttributesJsonValidator attrsValidator)
    {
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("Code cannot exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(120).WithMessage("Name cannot exceed 120 characters.");

        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Label is required.")
            .MaximumLength(120).WithMessage("Label cannot exceed 120 characters.");

        RuleFor(x => x.DefaultElement)
            .CustomAsync(async (json, ctx, ct) =>
            {
                if (json != null)
                {
                    var failures = await attrsValidator.ValidateAsync(AttributeScreen.CC.ToString(), json, ct);
                    foreach (var (path, message) in failures)
                        ctx.AddFailure(path, message);
                }
            });
    }
}

public class UpdateCivilComponentValidator : AbstractValidator<UpdateCivilComponentCommand>
{
    public UpdateCivilComponentValidator(IAttributesJsonValidator attrsValidator)
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(120).WithMessage("Name cannot exceed 120 characters.");

        RuleFor(x => x.Label)
            .NotEmpty().WithMessage("Label is required.")
            .MaximumLength(120).WithMessage("Label cannot exceed 120 characters.");

        RuleFor(x => x.DefaultElement)
            .CustomAsync(async (json, ctx, ct) =>
            {
                if (json != null)
                {
                    var failures = await attrsValidator.ValidateAsync(AttributeScreen.CC.ToString(), json, ct);
                    foreach (var (path, message) in failures)
                        ctx.AddFailure(path, message);
                }
            });
    }
}
