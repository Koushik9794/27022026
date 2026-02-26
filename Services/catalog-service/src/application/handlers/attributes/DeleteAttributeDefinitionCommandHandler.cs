using CatalogService.Application.commands;
using CatalogService.Application.commands.attributes;
using CatalogService.Application.Errors;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.attributes;

public class DeleteAttributeDefinitionCommandHandler(IAttributeDefinitionRepository repository)
{
    public async Task<Result<bool>> Handle(DeleteattributeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await repository.GetByIdAsync(request.Id, cancellationToken);
            if (existing == null)
            {
                return Result.Failure<bool>(AttributeDefinitionErrors.NotFound(request.Id));
            }
            existing.Delete(request.DeletedBy);
            var success = await repository.DeleteAsync(request.Id, request.DeletedBy, cancellationToken);
            return success
                ? Result<bool>.Success(true)
                : Result.Failure<bool>(AttributeDefinitionErrors.DeleteFailed("Database delete failed."));
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(AttributeDefinitionErrors.DeleteFailed(ex.Message));
        }
    }
}

