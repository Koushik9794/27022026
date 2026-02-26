using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using GssWebApi.Dto;
using GssWebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;

namespace GssWebApi.Api.Controllers;

/// <summary>
/// Admin controller for managing Load Charts.
/// Proxies requests to the Catalog Service.
/// </summary>
[ApiController]
[Route("api/admin/load-charts")]
[Produces("application/json")]
public class AdminLoadChartsController : ControllerBase
{
    private readonly ICatalogServiceClient _catalogClient;
    private readonly ILogger<AdminLoadChartsController> _logger;

    public AdminLoadChartsController(ICatalogServiceClient catalogClient, ILogger<AdminLoadChartsController> logger)
    {
        _catalogClient = catalogClient;
        _logger = logger;
    }

    /// <summary>
    /// Get all load charts with optional filters.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LoadChartDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetLoadCharts(
        [FromQuery] Guid? productGroupId,
        [FromQuery] string? chartType,
        [FromQuery] string? componentCode,
        [FromQuery] Guid? componentTypeId,
        [FromQuery] bool includeDeleted = false)
    {
        try
        {
            _logger.LogInformation("Getting all load charts via BFF. Filters - ProductGroup: {ProductGroupId}, Type: {ChartType}, Code: {Code}", 
                productGroupId, chartType, componentCode);
            var response = await _catalogClient.GetLoadChartsAsync(productGroupId, chartType, componentCode, componentTypeId, includeDeleted);
            return await ProxyResponse(response, "GetLoadCharts");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting load charts via BFF");
            throw;
        }
    }

    /// <summary>
    /// Get a load chart by its unique ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LoadChartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting load chart by ID {Id} via BFF", id);
            var response = await _catalogClient.GetLoadChartByIdAsync(id);
            return await ProxyResponse(response, $"GetLoadChartById({id})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting load chart {Id} via BFF", id);
            throw;
        }
    }

    /// <summary>
    /// Create a new load chart.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateLoadChartRequest request)
    {
        try
        {
            _logger.LogInformation("Creating new load chart via BFF. ComponentCode: {Code}", request.ComponentCode);
            var response = await _catalogClient.CreateLoadChartAsync(request);
            return await ProxyResponse(response, "CreateLoadChart");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating load chart {Code} via BFF", request.ComponentCode);
            throw;
        }
    }

    /// <summary>
    /// Update an existing load chart.
    /// </summary>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLoadChartRequest request)
    {
        try
        {
            _logger.LogInformation("Updating load chart {Id} via BFF", id);
            var response = await _catalogClient.UpdateLoadChartAsync(id, request);
            return await ProxyResponse(response, $"UpdateLoadChart({id})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating load chart {Id} via BFF", id);
            throw;
        }
    }

    /// <summary>
    /// Delete a load chart (soft delete).
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] string? deletedBy)
    {
        try
        {
            _logger.LogInformation("Deleting load chart {Id} via BFF by user {User}", id, deletedBy);
            var response = await _catalogClient.DeleteLoadChartAsync(id, deletedBy);
            return await ProxyResponse(response, $"DeleteLoadChart({id})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting load chart {Id} via BFF", id);
            throw;
        }
    }

    private async Task<IActionResult> ProxyResponse(HttpResponseMessage response, string operation)
    {
        var content = await response.Content.ReadAsStringAsync();

        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Downstream service call {Operation} failed with status: {StatusCode}. Response: {Content}", 
                operation, response.StatusCode, content);
        }

        return new ContentResult
        {
            Content = content,
            ContentType = "application/json",
            StatusCode = (int)response.StatusCode
        };
    }
}
