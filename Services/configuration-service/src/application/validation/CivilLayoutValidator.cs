using ConfigurationService.Application.Commands;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace ConfigurationService.Application.Validation;

public class SaveCivilLayoutValidator : AbstractValidator<SaveCivilLayoutCommand>
{
    public SaveCivilLayoutValidator()
    {
        RuleFor(x => x.ConfigurationId)
            .NotEmpty().WithMessage("Configuration Id is required.");


        RuleFor(x => x.SourceFile)
            .NotNull().WithMessage("Source File (dxf) is required.")
            .Must(file => file.FileName.EndsWith(".dxf", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Source File must be a .dxf file.")
            .When(x => x.SourceFile != null);

        RuleFor(x => x.CivilJson)
            .NotNull().WithMessage("Civil Json is required.")
            .Must(file => file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Civil Json must be a .json file.")
            .When(x => x.CivilJson != null);
    }
}

public class UpdateCivilLayoutValidator : AbstractValidator<UpdateCivilLayoutCommand>
{
    public UpdateCivilLayoutValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.WarehouseType)
            .NotEmpty().WithMessage("Warehouse Type is required.");

        RuleFor(x => x.SourceFile)
            .Must(file => file.FileName.EndsWith(".dxf", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Source File must be a .dxf file.")
            .When(x => x.SourceFile != null);

        RuleFor(x => x.CivilJson)
            .Must(file => file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Civil Json must be a .json file.")
            .When(x => x.CivilJson != null);
    }
}

public class SaveRackLayoutValidator : AbstractValidator<SaveRackLayoutCommand>
{
    public SaveRackLayoutValidator()
    {
        RuleFor(x => x.ConfigurationId)
            .NotEmpty().WithMessage("Configuration Id is required.");

        RuleFor(x => x.Civilversion)
            .GreaterThanOrEqualTo(0).WithMessage("Civil version must be greater than or equal to 0.");

        RuleFor(x => x.Configversion)
            .GreaterThanOrEqualTo(0).WithMessage("Config version must be greater than or equal to 0.");

        RuleFor(x => x.RackJson)
            .NotNull().WithMessage("Rack Json is required.")
            .Must(file => file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Rack Json must be a .json file.")
            .When(x => x.RackJson != null);
    }
}

public class UpdateRackLayoutValidator : AbstractValidator<UpdateRackLayoutCommand>
{
    public UpdateRackLayoutValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.RackJson)
            .NotNull().WithMessage("Rack Json is required.")
            .Must(file => file.FileName.EndsWith(".json", StringComparison.OrdinalIgnoreCase))
            .WithMessage("Rack Json must be a .json file.")
            .When(x => x.RackJson != null);
    }
}
