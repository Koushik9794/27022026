using CatalogService.Application.commands.Mhe;
using CatalogService.Application.Errors;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.Mhe;

public class DeleteMheCommandHandler
{
    private readonly IMheRepository _repository;

    public DeleteMheCommandHandler(IMheRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(
        DeleteMheCommand request,
        CancellationToken cancellationToken)
    {
        //  1. Validate empty Guid
        if (request.Id == Guid.Empty)
        {
            return Result.Failure<bool>(
                MheErrors.InvalidId()
            );
        }

        //  2. Check existence
        var mhe = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (mhe == null)
        {
            return Result.Failure<bool>(
                MheErrors.NotFound(request.Id)
            );
        }

        // 3. Repository handles delete + persistence
        return await _repository.DeleteAsync(
            request.Id,
            request.DeletedBy,
            cancellationToken
        );
    }
}