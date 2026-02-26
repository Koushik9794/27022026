using CatalogService.Application.Dtos;
using CatalogService.Application.Queries.Taxonomy;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.Taxonomy;

public class GetAllProductGroupsHandler
{
    private readonly IProductGroupRepository _repository;

    public GetAllProductGroupsHandler(IProductGroupRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ProductGroupDto>>> Handle(GetAllProductGroupsQuery query)
    {
        var groups = await _repository.GetAllAsync(query.IncludeInactive);
        return Result.Success(groups.Select(MapToDto).ToList());
    }

    private static ProductGroupDto MapToDto(ProductGroup group)
    {
        return new ProductGroupDto(
            group.Id,
            group.Code,
            group.Name,
            group.Description,
            group.ParentGroupId,
            group.ParentGroupCode,
            group.IsVariant,
            group.IsActive,
            group.CreatedAt,
            group.UpdatedAt
        );
    }
}

public class GetProductGroupByIdHandler
{
    private readonly IProductGroupRepository _repository;

    public GetProductGroupByIdHandler(IProductGroupRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ProductGroupDto?>> Handle(GetProductGroupByIdQuery query)
    {
        var group = await _repository.GetByIdAsync(query.Id);
        if (group == null) return Result.Success<ProductGroupDto?>(null);

        return new ProductGroupDto(
            group.Id,
            group.Code,
            group.Name,
            group.Description,
            group.ParentGroupId,
            group.ParentGroupCode,
            group.IsVariant,
            group.IsActive,
            group.CreatedAt,
            group.UpdatedAt
        );
    }
}

public class GetProductGroupByCodeHandler
{
    private readonly IProductGroupRepository _repository;

    public GetProductGroupByCodeHandler(IProductGroupRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<ProductGroupDto?>> Handle(GetProductGroupByCodeQuery query)
    {
        var group = await _repository.GetByCodeAsync(query.Code);
        if (group == null) return Result.Success<ProductGroupDto?>(null);

        return new ProductGroupDto(
            group.Id,
            group.Code,
            group.Name,
            group.Description,
            group.ParentGroupId,
            group.ParentGroupCode,
            group.IsVariant,
            group.IsActive,
            group.CreatedAt,
            group.UpdatedAt
        );
    }
}

public class GetProductGroupVariantsHandler
{
    private readonly IProductGroupRepository _repository;

    public GetProductGroupVariantsHandler(IProductGroupRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<ProductGroupDto>>> Handle(GetProductGroupVariantsQuery query)
    {
        var variants = await _repository.GetVariantsAsync(query.ParentGroupId);
        return Result.Success(variants.Select(g => new ProductGroupDto(
            g.Id,
            g.Code,
            g.Name,
            g.Description,
            g.ParentGroupId,
            g.ParentGroupCode,
            g.IsVariant,
            g.IsActive,
            g.CreatedAt,
            g.UpdatedAt
        )).ToList());
    }
}
