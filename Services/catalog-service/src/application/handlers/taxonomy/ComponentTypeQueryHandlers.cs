using System.Text.Json;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using CatalogService.Application.Dtos;
using CatalogService.Application.Queries.Taxonomy;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.Taxonomy;

public class GetAllComponentTypesHandler
{
    private readonly IComponentTypeRepository _repository;

    public GetAllComponentTypesHandler(IComponentTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ComponentTypeDto>>> Handle(GetAllComponentTypesQuery query)
    {
        var types = await _repository.GetAllAsync(query.ComponentGroupCode, query.ComponentGroupId, query.IncludeInactive);
        return Result.Success(types.Select(MapToDto).ToList());
    }

    private static ComponentTypeDto MapToDto(ComponentType type)
    {
        return new ComponentTypeDto(
            type.Id,
            type.Code,
            type.Name,
            type.Description,
            type.ComponentGroupId,
            type.ComponentGroupCode,
            type.ComponentGroupName,
            type.ParentTypeId,
            type.ParentTypeCode,
            type.AttributeSchema != null ? JsonSerializer.Deserialize<object>(type.AttributeSchema.RootElement.GetRawText()) : null,
            type.IsActive,
            type.CreatedAt,
            type.UpdatedAt
        );
    }
}

public class GetComponentTypeByIdHandler
{
    private readonly IComponentTypeRepository _repository;

    public GetComponentTypeByIdHandler(IComponentTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ComponentTypeDto?>> Handle(GetComponentTypeByIdQuery query)
    {
        var type = await _repository.GetByIdAsync(query.Id);
        if (type == null) return Result.Success<ComponentTypeDto?>(null);

        return Result.Success<ComponentTypeDto?>(new ComponentTypeDto(
            type.Id,
            type.Code,
            type.Name,
            type.Description,
            type.ComponentGroupId,
            type.ComponentGroupCode,
            type.ComponentGroupName,
            type.ParentTypeId,
            type.ParentTypeCode,
            type.AttributeSchema != null ? JsonSerializer.Deserialize<object>(type.AttributeSchema.RootElement.GetRawText()) : null,
            type.IsActive,
            type.CreatedAt,
            type.UpdatedAt
        ));
    }
}

public class GetComponentTypeByCodeHandler
{
    private readonly IComponentTypeRepository _repository;

    public GetComponentTypeByCodeHandler(IComponentTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ComponentTypeDto?>> Handle(GetComponentTypeByCodeQuery query)
    {
        var type = await _repository.GetByCodeAsync(query.Code);
        if (type == null) return Result.Success<ComponentTypeDto?>(null);

        return Result.Success<ComponentTypeDto?>(new ComponentTypeDto(
            type.Id,
            type.Code,
            type.Name,
            type.Description,
            type.ComponentGroupId,
            type.ComponentGroupCode,
            type.ComponentGroupName,
            type.ParentTypeId,
            type.ParentTypeCode,
            type.AttributeSchema != null ? JsonSerializer.Deserialize<object>(type.AttributeSchema.RootElement.GetRawText()) : null,
            type.IsActive,
            type.CreatedAt,
            type.UpdatedAt
        ));
    }
}
