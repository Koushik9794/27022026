using AdminService.Application.Commands;
using FluentValidation;

namespace AdminService.Application.Validators.RolePermissions;

public sealed class AssignPermissionToRoleCommandValidator : AbstractValidator<AssignPermissionToRoleCommand>
{
    public AssignPermissionToRoleCommandValidator()
    {
        RuleFor(x => x.RoleId).NotEmpty();
        RuleFor(x => x.PermissionId).NotEmpty();
        RuleFor(x => x.CreatedBy).NotEmpty().MaximumLength(100);
    }
}
