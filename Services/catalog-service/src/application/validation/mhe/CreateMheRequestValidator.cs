using System.Text.Json;
using CatalogService.Application.commands.Mhe;
using CatalogService.Domain.Enums;
using CatalogService.Infrastructure.Persistence;
using CatalogService.Application.Validation.Common;
using FluentValidation;

namespace CatalogService.Application.Validation.Mhe;

public class CreateMheRequestValidator : AbstractValidator<CreateMheCommand>
{
    IAttributesJsonValidator _attrsValidator;
    public CreateMheRequestValidator(IAttributesJsonValidator attrsValidator)
    {
        _attrsValidator = attrsValidator;
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("Code is required.")
            .MaximumLength(50).WithMessage("MheCode cannot exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Name is required.")
            .MaximumLength(200).WithMessage("MheName cannot exceed 200 characters.");

        RuleFor(x => x.Manufacturer).MaximumLength(120);
        RuleFor(x => x.Brand).MaximumLength(120);
        RuleFor(x => x.Model).MaximumLength(120);
        RuleFor(x => x.MheType).MaximumLength(80);
        RuleFor(x => x.MheCategory).MaximumLength(80);

        RuleFor(x => x.GlbFile)
            .Must(file => file == null || file.Length <= 50 * 1024 * 1024) // 50MB limit
            .WithMessage("GLB file size cannot exceed 50MB.");

        When(x => x.GlbFile is not null, () =>
        {
            RuleFor(x => x.GlbFile!.FileName)
                .MaximumLength(500)
                .Must(path => path is null || path.IndexOfAny(Path.GetInvalidPathChars()) < 0)
                .WithMessage("GlbFilePath is not a valid path.");
        });



        RuleFor(x => x.Attributes)
             .CustomAsync(async (json, ctx, ct) =>
             {
                 var failures = await _attrsValidator.ValidateAsync(AttributeScreen.MHE.ToString(), json, ct);
                 foreach ((string path, string message) in failures)
                     ctx.AddFailure(path, message);
             });
    }


}








