using System.Text.Json;
using CatalogService.Application.commands;
using CatalogService.Application.commands.attributes;
using CatalogService.Application.commands.Mhe;
using CatalogService.Application.Errors;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.attributes;

public class CreateAttributeDefinitionCommandHandler(IAttributeDefinitionRepository repository)

{
    public async Task<Result<Guid>> Handle(CreateattributeCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (await repository.ExistsAsync(request.AttributeKey, null, request.Screen, cancellationToken))
            {
                return Result.Failure<Guid>(AttributeDefinitionErrors.AlreadyExists(request.AttributeKey));
            }
            var attribute = AttributeDefinition.Create(
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
                request.CreatedBy);
            await repository.CreateAsync(attribute, cancellationToken);
            return Result<Guid>.Success(attribute.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(AttributeDefinitionErrors.CreateFailed(ex.Message));
        }
    }
}

