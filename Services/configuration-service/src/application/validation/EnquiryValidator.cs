namespace ConfigurationService.Application.Validation;

using ConfigurationService.Application.Commands;
using FluentValidation;
public class CreateEnquiryValidator : AbstractValidator<CreateEnquiryCommand>
{
    public CreateEnquiryValidator()
    {
        RuleFor(x => x.EnquiryNo)
        .NotEmpty().WithMessage("Enquiry Number is required.")
        .MaximumLength(200).WithMessage("Enquiry Number cannot exceed 200 characters.");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Enquiry Name is required.")
            .MaximumLength(255).WithMessage("Name cannot exceed 255 characters.");
        RuleFor(x => x.CustomerMail)
            .EmailAddress().WithMessage("Customer Email is not valid")
            .Unless(x => string.IsNullOrWhiteSpace(x.CustomerMail));
        RuleFor(x => x.CustomerContact)
.Must(v => !v.HasValue || v.Value.ToString().Length == 10)
.WithMessage("Customer contact must be a 10‑digit number.");

    }
}
public class UpdateEnquiryValidator : AbstractValidator<UpdateEnquiryCommand>
{
    public UpdateEnquiryValidator()
    {
        RuleFor(x => x.EnquiryNo)
        .NotEmpty().WithMessage("Enquiry Number is required.")
        .MaximumLength(200).WithMessage("Enquiry Number cannot exceed 200 characters.");
        RuleFor(x => x.Name)
            .NotEmpty().WithMessage("Enquiry Name is required.")
            .MaximumLength(255).WithMessage("Name cannot exceed 255 characters.");
        RuleFor(x => x.CustomerMail)
            .EmailAddress().WithMessage("Customer Email is not valid")
            .Unless(x => string.IsNullOrWhiteSpace(x.CustomerMail));
        RuleFor(x => x.CustomerContact)
    .Must(v => !v.HasValue || v.Value.ToString().Length == 10)
    .WithMessage("Customer contact must be a 10‑digit number.");


    }
}
