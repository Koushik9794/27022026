using CatalogService.Application.dtos;
using CatalogService.Application.Errors;
using CatalogService.Application.queries;
using CatalogService.Application.queries.Attributes;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.attributes;

public class GetAttributeDefinitionByIdQueryHandler(IAttributeDefinitionRepository repository)
{
    public async Task<Result<AttributeDefinitionDto?>> Handle(GetAttributesByIdQuery request, CancellationToken cancellationToken)
    {
        var attr = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (attr == null) return Result.Failure<AttributeDefinitionDto?>(AttributeDefinitionErrors.NotFound(request.Id));
        return new AttributeDefinitionDto(
            attr.Id,
            attr.AttributeKey,
            attr.DisplayName,
            attr.Unit,
            attr.DataType,
            attr.MinValue,
            attr.MaxValue,
            attr.DefaultValue,
            attr.IsRequired,
            attr.AllowedValues,
            attr.Description,
            attr.Screen,
            attr.IsActive,
            attr.IsDeleted,
            attr.CreatedAt,
            attr.CreatedBy,
            attr.UpdatedAt,
            attr.UpdatedBy
        );
    }
}

