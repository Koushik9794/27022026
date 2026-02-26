using AdminService.Application.Commands;
using FluentValidation;

namespace AdminService.Application.Validators;

public sealed class UpdatePermissionCommandValidator : AbstractValidator<UpdatePermissionCommand>
{
    private static readonly string[] ValidModules = 
    {
        "Product Group", "Sub Products", "Weight", "Price", "Review", "BOM", 
        "Outputs", "Standard Type", "Design", "Rules", "Audit", "BOM Type", "Generate Outputs"
    };

    private static readonly string[] ValidEntities = 
    {
        "Access & Pricing", "Standards & Reviews", "General"
    };

    public UpdatePermissionCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty();
        
        RuleFor(x => x.ModuleName)
            .NotEmpty()
            .Must(m => ValidModules.Contains(m))
            .WithMessage($"Module Name must be one of: {string.Join(", ", ValidModules)}");

        RuleFor(x => x.ModifiedBy).NotEmpty().MaximumLength(200);
        RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description != null);
        
        RuleFor(x => x.EntityName)
            .Must(e => e == null || ValidEntities.Contains(e))
            .WithMessage($"Entity Name must be one of: {string.Join(", ", ValidEntities)}");
    }
}
