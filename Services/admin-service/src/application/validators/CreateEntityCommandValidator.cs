using FluentValidation;
using AdminService.Application.Commands;

namespace AdminService.Application.Validators
{
    public sealed class CreateEntityCommandValidator : AbstractValidator<CreateEntityCommand>
    {
        public CreateEntityCommandValidator()
        {
            RuleFor(x => x.EntityName).NotEmpty().MaximumLength(200);
            RuleFor(x => x.CreatedBy).NotEmpty().MaximumLength(200);
            RuleFor(x => x.Description).MaximumLength(1000).When(x => x.Description != null);

            RuleFor(x => x.SourceTable).NotEmpty().MaximumLength(200);
            RuleFor(x => x.PkColumn).NotEmpty().MaximumLength(200);
            RuleFor(x => x.LabelColumn).NotEmpty().MaximumLength(200);
        }
    }
}
