using CatalogService.Application.Commands.Sku;
using GssCommon.Common;
using CatalogService.Infrastructure.Persistence;

namespace CatalogService.Application.Handlers.Sku;

public class DeleteSkuCommandHandler
{
    private readonly ISkuRepository _repository;

    public DeleteSkuCommandHandler(ISkuRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(DeleteSkuCommand request, CancellationToken cancellationToken)
    {
        // 1. Validate empty Guid
        if (request.Id == Guid.Empty)
        {
            return Result.Failure<bool>(SkuErrors.InvalidId());
        }

        // 2. Check existence
        var sku = await _repository.GetByIdAsync(request.Id);
        if (sku == null)
        {
            return Result.Failure<bool>(SkuErrors.NotFound(request.Id));
        }

        // 3. Perform delete
        var deleted = await _repository.DeleteAsync(
            request.Id,
            request.DeletedBy
        );

        if (!deleted)
        {
            return Result.Failure<bool>(
                Error.Failure(
                    "SKU.DeleteFailed",
                    "Failed to delete SKU."
                )
            );
        }

        return Result.Success(true);
    }
}
