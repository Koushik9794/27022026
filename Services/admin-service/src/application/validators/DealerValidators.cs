using FluentValidation;
using AdminService.Application.Commands;
using AdminService.Infrastructure.Persistence;

namespace AdminService.Application.Validators
{
    public class CreateDealerCommandValidator : AbstractValidator<CreateDealerCommand>
    {
        public CreateDealerCommandValidator(IDealerRepository repository)
        {
            RuleFor(x => x.Code)
                .NotEmpty().WithMessage("Dealer code is required")
                .MaximumLength(50).WithMessage("Dealer code cannot exceed 50 characters")
                .MustAsync(async (code, cancellation) =>
                {
                    var existing = await repository.GetByCodeAsync(code);
                    return existing == null;
                }).WithMessage("Dealer code already exists");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Dealer name is required")
                .MaximumLength(200).WithMessage("Dealer name cannot exceed 200 characters");

            RuleFor(x => x.ContactEmail)
                .EmailAddress().WithMessage("Invalid email format")
                .When(x => !string.IsNullOrEmpty(x.ContactEmail));
                
            RuleFor(x => x.CountryCode)
                .Length(2).WithMessage("Country code must be exactly 2 characters")
                .When(x => !string.IsNullOrEmpty(x.CountryCode));
        }
    }

    public class UpdateDealerCommandValidator : AbstractValidator<UpdateDealerCommand>
    {
        public UpdateDealerCommandValidator()
        {
            RuleFor(x => x.Id)
                .NotEmpty().WithMessage("Dealer ID is required");

            RuleFor(x => x.Name)
                .NotEmpty().WithMessage("Dealer name is required")
                .MaximumLength(200).WithMessage("Dealer name cannot exceed 200 characters");

            RuleFor(x => x.ContactEmail)
                .EmailAddress().WithMessage("Invalid email format")
                .When(x => !string.IsNullOrEmpty(x.ContactEmail));

            RuleFor(x => x.CountryCode)
                .Length(2).WithMessage("Country code must be exactly 2 characters")
                .When(x => !string.IsNullOrEmpty(x.CountryCode));
        }
    }
}
