using GssCommon.Common;
using Microsoft.AspNetCore.Http;

namespace CatalogService.Application.Commands;

public record ImportLoadChartExcelCommand(
    IFormFile File,
    Guid ProductGroupId,
    string ChartType,
    Guid? CreatedBy
);
