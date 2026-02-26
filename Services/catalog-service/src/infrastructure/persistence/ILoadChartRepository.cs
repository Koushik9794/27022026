using CatalogService.application.commands.loadchart;
using CatalogService.application.dtos;
using CatalogService.Domain.Aggregates;

namespace CatalogService.Infrastructure.Persistence;

public interface ILoadChartRepository
{
    Task<LoadChart?> GetByIdAsync(Guid id);
    Task<IEnumerable<LoadChart>> GetAllAsync(
        Guid? productGroupId = null,
        string? chartType = null,
        string? componentCode = null,
        Guid? componentTypeId = null,
        bool includeDeleted = false,
        int page = 1,
        int pageSize = 50);
    Task<IEnumerable<LoadChart>> GetByChartTypeAsync(string chartType);
    Task<Guid> CreateAsync(LoadChart loadChart);
    Task<bool> UpdateAsync(LoadChart loadChart);
    Task<bool> DeleteAsync(Guid id);
    Task<IEnumerable<LoadChartCandidateDto>> GetLoadchartbysearch(string request);
}
