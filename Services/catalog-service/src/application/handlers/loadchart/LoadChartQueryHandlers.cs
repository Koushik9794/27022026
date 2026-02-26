using CatalogService.Application.Dtos;
using CatalogService.Application.Queries;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.LoadCharts;

public class LoadChartQueryHandlers
{
    private readonly ILoadChartRepository _repository;

    public LoadChartQueryHandlers(ILoadChartRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<LoadChartDto>>> Handle(GetAllLoadChartsQuery query)
    {
        var entities = await _repository.GetAllAsync(
            query.ProductGroupId,
            query.ChartType,
            query.ComponentCode,
            query.ComponentTypeId,
            query.IncludeDeleted,
            query.Page,
            query.PageSize
        );

        return Result.Success(entities.Select(MapToDto));
    }

    public async Task<Result<IEnumerable<LoadChartDto>>> Handle(GetLoadChartsByTypeQuery query)
    {
        var entities = await _repository.GetByChartTypeAsync(query.ChartType);
        return Result.Success(entities.Select(MapToDto));
    }

    public async Task<Result<LoadChartDto>> Handle(GetLoadChartByIdQuery query)
    {
        var entity = await _repository.GetByIdAsync(query.Id);
        if (entity == null) return Result.Failure<LoadChartDto>(Error.NotFound("LoadChart.NotFound", $"Load Chart {query.Id} not found."));

        return Result.Success(MapToDto(entity));
    }

    private static LoadChartDto MapToDto(CatalogService.Domain.Aggregates.LoadChart entity)
    {
        return new LoadChartDto(
            entity.Id,
            entity.ProductGroupId,
            entity.ProductGroupName,
            entity.ChartType,
            entity.ComponentCode,
            entity.ComponentTypeId,
            entity.ComponentName,
            entity.Attributes,
            entity.IsActive,
            entity.IsDelete,
            entity.CreatedAt,
            entity.UpdatedAt,
            entity.CreatedBy,
            entity.UpdatedBy
        );
    }
}
