using FluentValidation;
using AdminService.Application.Commands;

namespace AdminService.Application.Validators
{
    /// <summary>
    /// Validator for RegisterUserCommand
    /// </summary>
    public sealed class RegisterUserCommandValidator : AbstractValidator<RegisterUserCommand>
    {
        public RegisterUserCommandValidator()
        {
            RuleFor(x => x.Email)
                .NotEmpty().WithMessage("Email is required")
                .EmailAddress().WithMessage("Invalid email format");

            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("Display name is required")
                .MinimumLength(2).WithMessage("Display name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Display name cannot exceed 100 characters");

            RuleFor(x => x.Role)
                .NotEmpty().WithMessage("Role is required")
                .Must(BeValidRole).WithMessage("Invalid role. Valid roles: SUPER_ADMIN, ADMIN, DEALER, DESIGNER, VIEWER");
        }

        private bool BeValidRole(string role)
        {
            var validRoles = new[] { "SUPER_ADMIN", "ADMIN", "DEALER", "DESIGNER", "VIEWER" };
            return validRoles.Contains(role?.ToUpperInvariant());
        }
    }
}
