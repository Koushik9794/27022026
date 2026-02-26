using System.Text.Json;
using CatalogService.Application.Commands.Pallets;
using CatalogService.Domain.Enums;
using CatalogService.Infrastructure.Persistence;
using CatalogService.Application.Validation.Common;
using FluentValidation;

namespace CatalogService.Application.Validation.Pallets;


public class CreatePalletsValidator : AbstractValidator<CreatePalletCommand>
{
    IAttributesJsonValidator _attrsValidator;

    public CreatePalletsValidator(IAttributesJsonValidator attrsValidator)
    {
        _attrsValidator = attrsValidator;

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Pallet Code is required.")
            .MaximumLength(50).WithMessage("Pallet Code cannot exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Pallet Name is required.")
            .MaximumLength(200).WithMessage("Pallet Name cannot exceed 200 characters.");

        When(x => x.GlbFile != null, () =>
        {
            RuleFor(x => x.GlbFile!.FileName)
                .MaximumLength(500).WithMessage("GlbFile file name cannot exceed 500 characters.")
                .Must(path => path.IndexOfAny(Path.GetInvalidPathChars()) < 0)
                .WithMessage("GlbFilePath is not a valid path.");
        });

        RuleFor(x => x.AttributeSchema)
             .CustomAsync(async (json, ctx, ct) =>
             {
                 var failures = await attrsValidator.ValidateAsync(AttributeScreen.PALLET.ToString(), json, ct);
                 foreach (var (path, message) in failures)
                     ctx.AddFailure(path, message);
             });
    }
}


public class UpdatePalletsValidator : AbstractValidator<UpdatePalletCommand>
{
    private readonly IAttributesJsonValidator _attrsValidator;

    public UpdatePalletsValidator(IAttributesJsonValidator attrsValidator)
    {
        _attrsValidator = attrsValidator;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("invalid pallet id or null id not valid").WithErrorCode("Pallet.InvalidId");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Pallet Name is required.")
            .MaximumLength(120).WithMessage("Pallet Name cannot exceed 120 characters.");

        When(x => x.GlbFile != null, () =>
        {
            RuleFor(x => x.GlbFile!.FileName)
                .MaximumLength(500).WithMessage("GlbFile file name cannot exceed 500 characters.")
                .Must(path => path.IndexOfAny(Path.GetInvalidPathChars()) < 0)
                .WithMessage("GlbFilePath is not a valid path.");
        });

        RuleFor(x => x.AttributeSchema)
             .CustomAsync(async (json, ctx, ct) =>
             {
                 var failures = await attrsValidator.ValidateAsync(AttributeScreen.PALLET.ToString(), json, ct);
                 foreach (var (path, message) in failures)
                     ctx.AddFailure(path, message);
             });
    }
}

public class DeletePalletCommandValidator : AbstractValidator<DeletePalletCommand>
{
    public DeletePalletCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("invalid pallet id or null id not valid").WithErrorCode("Pallet.InvalidId");
    }
}

