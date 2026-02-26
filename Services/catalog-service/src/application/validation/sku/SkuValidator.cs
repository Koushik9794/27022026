using CatalogService.Application.Validation;
using CatalogService.Application.Commands.Pallets;
using CatalogService.Application.Commands.Sku;
using CatalogService.Application.Queries.Sku;
using CatalogService.Domain.Enums;
using CatalogService.Application.Validation.Common;
using FluentValidation;

namespace CatalogService.Application.Validation.sku;

public class CreateSkuValidator : AbstractValidator<CreateSkuCommand>
{
    private readonly IAttributesJsonValidator _attrsValidator;

    public CreateSkuValidator(IAttributesJsonValidator attrsValidator)
    {
        _attrsValidator = attrsValidator;

        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("invalid code or null code not valid").WithErrorCode("SKU.InvalidCode")
            .MaximumLength(50).WithMessage("Code cannot exceed 50 characters.").WithErrorCode("SKU.InvalidCode");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("invalid name or null name not valid").WithErrorCode("SKU.InvalidName")
            .MaximumLength(120).WithMessage("Name cannot exceed 120 characters.").WithErrorCode("SKU.InvalidName");

        When(x => x.GlbFile != null, () =>
        {
            RuleFor(x => x.GlbFile!.FileName)
                .MaximumLength(500).WithMessage("GlbFile file name cannot exceed 500 characters.").WithErrorCode("SKU.InvalidFile")
                .Must(path => path.IndexOfAny(Path.GetInvalidPathChars()) < 0)
                .WithMessage("GlbFilePath is not a valid path.").WithErrorCode("SKU.InvalidFile");
        });

        RuleFor(x => x.AttributeSchema)
             .CustomAsync(async (json, ctx, ct) =>
             {
                 var failures = await attrsValidator.ValidateAsync(AttributeScreen.SKU.ToString(), json, ct);
                 foreach (var (path, message) in failures)
                     ctx.AddFailure(new FluentValidation.Results.ValidationFailure(path, message) { ErrorCode = "SKU.InvalidAttributeSchema" });
             });
    }
}


public class GetSkuByIdQueryValidator : AbstractValidator<GetSkuByIdQuery>
{
    public GetSkuByIdQueryValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("invalid id or null id not valid").WithErrorCode("SKU.InvalidId");
    }
}


public class UpdateSkuValidator : AbstractValidator<UpdateSkuCommand>
{
    private readonly IAttributesJsonValidator _attrsValidator;

    public UpdateSkuValidator(IAttributesJsonValidator attrsValidator)
    {
        _attrsValidator = attrsValidator;

        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("invalid sku id or null sku id not valid").WithErrorCode("SKU.InvalidId");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("invalid sku name or null sku name not valid").WithErrorCode("SKU.InvalidName")
            .MaximumLength(120).WithMessage("sku name cannot exceed 120 characters.").WithErrorCode("SKU.InvalidName");

        When(x => x.GlbFile != null, () =>
        {
            RuleFor(x => x.GlbFile!.FileName)
                .MaximumLength(500).WithMessage("GlbFile file name cannot exceed 500 characters.").WithErrorCode("SKU.InvalidFile")
                .Must(path => path.IndexOfAny(Path.GetInvalidPathChars()) < 0)
                .WithMessage("GlbFilePath is not a valid path.").WithErrorCode("SKU.InvalidFile");
        });

        RuleFor(x => x.AttributeSchema)
             .CustomAsync(async (json, ctx, ct) =>
             {
                 var failures = await attrsValidator.ValidateAsync(AttributeScreen.SKU.ToString(), json, ct);
                 foreach (var (path, message) in failures)
                     ctx.AddFailure(new FluentValidation.Results.ValidationFailure(path, message) { ErrorCode = "SKU.InvalidAttributeSchema" });
             });
    }
}

public class DeleteSkuCommandValidator : AbstractValidator<DeleteSkuCommand>
{
    public DeleteSkuCommandValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("invalid sku id or null  id not valid").WithErrorCode("SKU.InvalidId");
    }
}
