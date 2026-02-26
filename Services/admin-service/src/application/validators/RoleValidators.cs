using FluentValidation;
using AdminService.Application.Commands;

namespace AdminService.Application.Validators
{
    public class CreateRoleValidator : AbstractValidator<CreateRoleCommand>
    {
        public CreateRoleValidator()
        {
            RuleFor(x => x.RoleName).NotEmpty().MinimumLength(3).MaximumLength(50);
            RuleFor(x => x.Description).MaximumLength(500);
        }
    }

    public class UpdateRoleValidator : AbstractValidator<UpdateRoleCommand>
    {
        public UpdateRoleValidator()
        {
            RuleFor(x => x.RoleId).NotEmpty();
            RuleFor(x => x.RoleName).NotEmpty().MinimumLength(3).MaximumLength(50);
            RuleFor(x => x.Description).MaximumLength(500);
        }
    }
}
