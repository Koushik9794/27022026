using CatalogService.Application.commands.Mhe;
using CatalogService.Infrastructure.Persistence;
using FluentValidation;

namespace CatalogService.Application.Validation.Mhe;

public class DeleteMheRequestValidator : AbstractValidator<DeleteMheCommand>
{
    private readonly IMheRepository _repository;

    public DeleteMheRequestValidator(IMheRepository repository)
    {
        _repository = repository;

        
    RuleFor(x => x.Id)
        .Cascade(CascadeMode.Stop)
        .NotEqual(Guid.Empty)
        .WithMessage("MHE Id must not be empty.")
        .MustAsync(BeAValidIdInDatabase)
        .WithMessage("Invalid MHE Id: does not exist in the database.");
}

    private async Task<bool> BeAValidIdInDatabase(
        Guid id,
        CancellationToken cancellationToken)
    {
        var mhe = await _repository.GetByIdAsync(id, cancellationToken);
        return mhe != null;
    }
}