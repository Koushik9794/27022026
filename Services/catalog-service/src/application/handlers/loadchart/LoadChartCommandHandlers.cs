using System.Text.Json;
using CatalogService.Application.Commands;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.LoadCharts;

public class LoadChartCommandHandlers
{
    private readonly ILoadChartRepository _repository;
    private readonly IProductGroupRepository _productGroupRepository;
    private readonly IComponentNameRepository _componentNameRepository;

    public LoadChartCommandHandlers(
        ILoadChartRepository repository,
        IProductGroupRepository productGroupRepository,
        IComponentNameRepository componentNameRepository)
    {
        _repository = repository;
        _productGroupRepository = productGroupRepository;
        _componentNameRepository = componentNameRepository;
    }

    public async Task<Result<Guid>> Handle(CreateLoadChartCommand command)
    {
        // Validate Foreign Keys
        var productGroup = await _productGroupRepository.GetByIdAsync(command.ProductGroupId);
        if (productGroup == null) return Result.Failure<Guid>(Error.NotFound("ProductGroup.NotFound", $"Product Group {command.ProductGroupId} not found."));

        var component = await _componentNameRepository.GetByCodeAsync(command.ComponentCode);
        if (component == null) return Result.Failure<Guid>(Error.NotFound("ComponentName.NotFound", $"Component Name {command.ComponentCode} not found."));

        // Parse JSON strings
        var attributes = DeserializeJson(command.Attributes);

        try
        {
            var loadChart = CatalogService.Domain.Aggregates.LoadChart.Create(
                command.ProductGroupId,
                command.ChartType,
                command.ComponentCode,
                command.ComponentTypeId,
                attributes,
                command.CreatedBy
            );

            await _repository.CreateAsync(loadChart);
            return Result.Success(loadChart.Id);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<Guid>(Error.Validation("LoadChart.Validation", ex.Message));
        }
    }

    public async Task<Result<bool>> Handle(UpdateLoadChartCommand command)
    {
        var loadChart = await _repository.GetByIdAsync(command.Id);
        if (loadChart == null) return Result.Failure<bool>(Error.NotFound("LoadChart.NotFound", $"Load Chart {command.Id} not found."));

        // Validate Foreign Keys
        var productGroup = await _productGroupRepository.GetByIdAsync(command.ProductGroupId);
        if (productGroup == null) return Result.Failure<bool>(Error.NotFound("ProductGroup.NotFound", $"Product Group {command.ProductGroupId} not found."));

        var component = await _componentNameRepository.GetByCodeAsync(command.ComponentCode);
        if (component == null) return Result.Failure<bool>(Error.NotFound("ComponentName.NotFound", $"Component Name {command.ComponentCode} not found."));

        // Parse JSON strings
        var attributes = DeserializeJson(command.Attributes);

        try
        {
            loadChart.Update(
                command.ProductGroupId,
                command.ChartType,
                command.ComponentCode,
                command.ComponentTypeId,
                attributes,
                command.IsActive,
                command.UpdatedBy
            );

            await _repository.UpdateAsync(loadChart);
            return Result.Success(true);
        }
        catch (ArgumentException ex)
        {
            return Result.Failure<bool>(Error.Validation("LoadChart.Validation", ex.Message));
        }
    }

    public async Task<Result<bool>> Handle(DeleteLoadChartCommand command)
    {
        var loadChart = await _repository.GetByIdAsync(command.Id);
        if (loadChart == null) return Result.Failure<bool>(Error.NotFound("LoadChart.NotFound", $"Load Chart {command.Id} not found."));

        loadChart.Delete(command.DeletedBy);
        await _repository.UpdateAsync(loadChart); // Soft delete
        return Result.Success(true);
    }

    private Dictionary<string, JsonElement> DeserializeJson(string? json)
    {
        if (string.IsNullOrWhiteSpace(json)) return [];
        try
        {
            return JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(json) ?? [];
        }
        catch { return []; }
    }
}
