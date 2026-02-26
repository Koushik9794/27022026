using FluentValidation;
using AdminService.Application.Commands;

namespace AdminService.Application.Validators
{
    /// <summary>
    /// Validator for UpdateUserCommand
    /// </summary>
    public sealed class UpdateUserCommandValidator : AbstractValidator<UpdateUserCommand>
    {
        public UpdateUserCommandValidator()
        {
            RuleFor(x => x.UserId)
                .NotEmpty().WithMessage("User ID is required");

            RuleFor(x => x.DisplayName)
                .NotEmpty().WithMessage("Display name is required")
                .MinimumLength(2).WithMessage("Display name must be at least 2 characters")
                .MaximumLength(100).WithMessage("Display name cannot exceed 100 characters");
        }
    }
}
