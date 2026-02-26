using GssWebApi.Dto;
using GssWebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Net.Http;

namespace GssWebApi.Api.Controllers;

/// <summary>
/// Admin controller for managing Component Masters.
/// Proxies requests to the Catalog Service.
/// </summary>
[ApiController]
[Route("api/admin/component-masters")]
[Produces("application/json")]
public class AdminComponentMastersController : ControllerBase
{
    private readonly ICatalogServiceClient _catalogClient;
    private readonly ILogger<AdminComponentMastersController> _logger;

    public AdminComponentMastersController(ICatalogServiceClient catalogClient, ILogger<AdminComponentMastersController> logger)
    {
        _catalogClient = catalogClient;
        _logger = logger;
    }

    /// <summary>
    /// Retrieves a list of component masters based on optional filter criteria.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ComponentMasterDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Get(
        [FromQuery] Guid? componentGroupId,
        [FromQuery] Guid? componentTypeId,
        [FromQuery] string? status,
        [FromQuery] bool includeDeleted = false)
    {
        try
        {
            _logger.LogInformation("Getting all component masters via BFF. Filters - Group: {GroupId}, Type: {TypeId}, Status: {Status}", 
                componentGroupId, componentTypeId, status);
            var response = await _catalogClient.GetComponentMastersAsync(componentGroupId, componentTypeId, status, includeDeleted);
            return await ProxyResponse(response, "GetComponentMasters");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting component masters via BFF");
            throw;
        }
    }

    /// <summary>
    /// Gets a specific component master by its ID.
    /// </summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ComponentMasterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting component master by ID {Id} via BFF", id);
            var response = await _catalogClient.GetComponentMasterByIdAsync(id);
            return await ProxyResponse(response, $"GetComponentMasterById({id})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting component master {Id} via BFF", id);
            throw;
        }
    }

    /// <summary>
    /// Gets a specific component master by its Component Master Code and Country Code.
    /// </summary>
    [HttpGet("code/{code}/country/{countryCode}")]
    [ProducesResponseType(typeof(ComponentMasterDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, string countryCode)
    {
        try
        {
            _logger.LogInformation("Getting component master by code {Code} and country {CountryCode} via BFF", code, countryCode);
            var response = await _catalogClient.GetComponentMasterByCodeAsync(code, countryCode);
            return await ProxyResponse(response, $"GetComponentMasterByCode({code}, {countryCode})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting component master by code {Code} via BFF", code);
            throw;
        }
    }

    /// <summary>
    /// Creates a new component master.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromForm] CreateComponentMasterRequest request)
    {
        try
        {
            _logger.LogInformation("Creating new component master via BFF. Code: {Code}", request.ComponentMasterCode);
            var response = await _catalogClient.CreateComponentMasterAsync(request);
            return await ProxyResponse(response, "CreateComponentMaster");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating component master {Code} via BFF", request.ComponentMasterCode);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing component master.
    /// </summary>
    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(bool), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateComponentMasterRequest request)
    {
        try
        {
            _logger.LogInformation("Updating component master {Id} via BFF", id);
            var response = await _catalogClient.UpdateComponentMasterAsync(id, request);
            return await ProxyResponse(response, $"UpdateComponentMaster({id})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating component master {Id} via BFF", id);
            throw;
        }
    }

    /// <summary>
    /// Soft deletes a component master.
    /// </summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] string? updatedBy)
    {
        try
        {
            _logger.LogInformation("Deleting component master {Id} via BFF by user {User}", id, updatedBy);
            var response = await _catalogClient.DeleteComponentMasterAsync(id, updatedBy);
            return await ProxyResponse(response, $"DeleteComponentMaster({id})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting component master {Id} via BFF", id);
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
