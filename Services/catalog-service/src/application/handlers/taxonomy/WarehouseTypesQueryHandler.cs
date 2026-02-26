using CatalogService.Application.dtos;
using CatalogService.Application.Errors;
using CatalogService.Application.Dtos;
using CatalogService.Application.Queries.Taxonomy;
using CatalogService.Domain.Entities;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.Taxonomy;

public class WarehouseTypesQueryHandler(IWarehouseTypeRepository repository)
{
    public async Task<Result<List<WarehouseTypeDto>>> Handle(GetAllWarehouseTypesQuery request, CancellationToken cancellationToken)
    {
        var entities = await repository.GetAllAsync(request.IncludeInactive, cancellationToken);

        return entities.Select(entity => new WarehouseTypeDto(
            entity.Id,
            entity.Name,
            entity.Label,
            entity.Icon,
            entity.Tooltip,
            entity.templatePath_Civil,
            entity.templatePath_Json,
            entity.Attributes,
            entity.IsActive,
            entity.IsDeleted,
            entity.CreatedAt,
            entity.CreatedBy,
            entity.UpdatedAt,
            entity.UpdatedBy
        )).ToList();
       
    }
}
public class WarehouseTypeByIdQueryHandler(IWarehouseTypeRepository repository)
{
    public async Task<Result<WarehouseTypeDto>> Handle(GetWarehouseTypesByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null) return Result.Failure<WarehouseTypeDto>(Error.NotFound("WarehouseType.NotFound", "Warehouse type not found."));


        return new WarehouseTypeDto(
           entity.Id,
            entity.Name,
            entity.Label,
            entity.Icon,
            entity.Tooltip,
            entity.templatePath_Civil,
            entity.templatePath_Json,
            entity.Attributes,
            entity.IsActive,
            entity.IsDeleted,
            entity.CreatedAt,
            entity.CreatedBy,
            entity.UpdatedAt,
            entity.UpdatedBy
);

    }
}
