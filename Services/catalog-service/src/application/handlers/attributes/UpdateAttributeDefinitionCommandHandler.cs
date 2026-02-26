using CatalogService.Application.commands;
using CatalogService.Application.Errors;
using CatalogService.Infrastructure.Persistence;
using CatalogService.Application.commands.attributes;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.attributes;

public class UpdateAttributeDefinitionCommandHandler(IAttributeDefinitionRepository repository)
{
    public async Task<Result<Guid>> Handle(UpdateattributeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            var existing = await repository.GetByIdAsync(request.Id, cancellationToken);
            if (existing == null)
            {
                return Result.Failure<Guid>(AttributeDefinitionErrors.NotFound(request.Id));
            }
            if (await repository.ExistsAsync(request.AttributeKey, request.Id, request.Screen, cancellationToken))
            {
                return Result.Failure<Guid>(AttributeDefinitionErrors.AlreadyExists(request.AttributeKey));
            }
            existing.Update(
                request.AttributeKey,
                request.DisplayName,
                request.DataType,
                request.Unit,
                request.MinValue,
                request.MaxValue,
                request.DefaultValue,
                request.IsRequired,
                request.AllowedValues,
                request.Description,
                request.Screen,
                request.IsActive,
                request.UpdatedBy);
            var success = await repository.UpdateAsync(existing, cancellationToken);
            return success;
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(AttributeDefinitionErrors.UpdateFailed(ex.Message));
        }
    }
}

