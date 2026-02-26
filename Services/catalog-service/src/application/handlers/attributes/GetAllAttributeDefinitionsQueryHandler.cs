using CatalogService.Application.queries;
using CatalogService.Infrastructure.Persistence;
using CatalogService.Application.dtos;
using CatalogService.Application.queries.Attributes;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.attributes;

public class GetAllAttributeDefinitionsQueryHandler(IAttributeDefinitionRepository repository)
{
    public async Task<Result<List<AttributeDefinitionDto>>> Handle(GetAllAttributesQuery request, CancellationToken cancellationToken)
    {
        var attributes = await repository.GetAllAsync(request.IsActive, cancellationToken);
        return attributes.Select(attr => new AttributeDefinitionDto(
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
        )).ToList();
    }
}

