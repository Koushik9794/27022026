using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace CatalogService.Application.Queries;

public record GetAllLoadChartsQuery(
    Guid? ProductGroupId = null,
    string? ChartType = null,
    string? ComponentCode = null,
    Guid? ComponentTypeId = null,
    bool IncludeDeleted = false,
    int Page = 1,
    int PageSize = 50
);

public record GetLoadChartsByTypeQuery(string ChartType);

public record GetLoadChartByIdQuery(Guid Id);


