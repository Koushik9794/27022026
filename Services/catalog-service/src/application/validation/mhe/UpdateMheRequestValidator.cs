using System.Text.Json;
using CatalogService.Application.Commands.Mhe;
using CatalogService.Domain.Enums;
using CatalogService.Infrastructure.Persistence;
using CatalogService.Application.Validation.Common;
using FluentValidation;

namespace CatalogService.Application.Validation.Mhe;

public class UpdateMheRequestValidator : AbstractValidator<UpdateMheCommand>
{
    private readonly IAttributesJsonValidator _attrsValidator;

    public UpdateMheRequestValidator(IAttributesJsonValidator attrsValidator)
    {
        _attrsValidator = attrsValidator;

        RuleFor(x => x.Id).NotEmpty();
        
        RuleFor(x => x.Code)
            .NotEmpty().WithMessage("MHE Code is required.")
            .MaximumLength(50).WithMessage("MHE Code cannot exceed 50 characters.");

        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("MHE Name is required.")
            .MaximumLength(200).WithMessage("MHE Name cannot exceed 200 characters.");

        RuleFor(x => x.Manufacturer).MaximumLength(120);
        RuleFor(x => x.Brand).MaximumLength(120);
        RuleFor(x => x.Model).MaximumLength(120);
        RuleFor(x => x.MheType).MaximumLength(80);
        RuleFor(x => x.MheCategory).MaximumLength(80);

        RuleFor(x => x.GlbFile)
            .Must(file => file == null || file.Length <= 50 * 1024 * 1024) // 50MB limit
            .WithMessage("GLB file size cannot exceed 50MB.");

        RuleFor(x => x.Attributes)
             .CustomAsync(async (json, ctx, ct) =>
             {
                 var failures = await _attrsValidator.ValidateAsync(AttributeScreen.MHE.ToString(), json, ct);
                 foreach ((string path, string message) in failures)
                     ctx.AddFailure(path, message);
             });
    }
}
