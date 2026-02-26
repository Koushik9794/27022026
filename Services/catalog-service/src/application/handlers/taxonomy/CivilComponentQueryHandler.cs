using CatalogService.Application.dtos;
using CatalogService.Application.Dtos;
using CatalogService.Application.Queries.Taxonomy;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.Taxonomy;


public class CivilComponentQueryHandler(ICivilComponentRepository repository)
{

    public async Task<Result<List<CivilComponentDto>>> Handle(GetAllCivileComponentQuery request, CancellationToken cancellationToken)
    {
        var entities = await repository.GetAllAsync(request.IncludeInactive, cancellationToken);
        var dtos = entities.Select(entity => new CivilComponentDto(
            entity.Id,
            entity.Code,
            entity.Name,
            entity.Label,
            entity.Icon,
            entity.Tooltip,
            entity.Category,
            entity.DefaultElement,
            entity.IsActive,
            entity.IsDeleted,
            entity.CreatedAt,
            entity.CreatedBy,
            entity.UpdatedAt,
            entity.UpdatedBy
        )).ToList();
        return Result<List<CivilComponentDto>>.Success(dtos);

    }
}
public class CivilComponentByIdQueryHandler(ICivilComponentRepository repository)
{
    public async Task<Result<CivilComponentDto>> Handle(GetCivileComponentByIdQuery request, CancellationToken cancellationToken)
    {
        var entity = await repository.GetByIdAsync(request.Id, cancellationToken);
        if (entity == null) return Result.Failure<CivilComponentDto>(Error.NotFound("CivilComponent.NotFound", "Civil component not found."));


        return new CivilComponentDto(
             entity.Id,
            entity.Code,
            entity.Name,
            entity.Label,
            entity.Icon,
            entity.Tooltip,
            entity.Category,
            entity.DefaultElement,
            entity.IsActive,
            entity.IsDeleted,
            entity.CreatedAt,
            entity.CreatedBy,
            entity.UpdatedAt,
            entity.UpdatedBy
);

    }
}
