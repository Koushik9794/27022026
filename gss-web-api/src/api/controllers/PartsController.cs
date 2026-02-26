using GssWebApi.Dto;
using GssWebApi.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Net.Http;

namespace GssWebApi.src.api.controllers;

/// <summary>
/// BFF Controller for Catalog Service Parts API.
/// </summary>
[ApiController]
[Route("api/parts")]
[Produces("application/json")]
public class PartsController : ControllerBase
{
    private readonly ILogger<PartsController> _logger;
    private readonly ICatalogServiceClient _catalogService;

    public PartsController(ILogger<PartsController> logger, ICatalogServiceClient catalogService)
    {
        _logger = logger;
        _catalogService = catalogService;
    }

    /// <summary>
    /// Retrieves a list of parts based on optional filter criteria.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PartDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(
        [FromQuery] string? countryCode,
        [FromQuery] Guid? componentGroupId,
        [FromQuery] Guid? componentTypeId,
        [FromQuery] bool? isActive = true,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] int? page = 1,
        [FromQuery] int? pageSize = 50)
    {
        try
        {
            _logger.LogInformation("Getting all parts via BFF. Filters - Country: {CountryCode}, Group: {GroupId}, Type: {TypeId}, Active: {IsActive}", 
                countryCode, componentGroupId, componentTypeId, isActive);
            
            var response = await _catalogService.GetPartsAsync(countryCode, componentGroupId, componentTypeId, isActive, includeDeleted, page, pageSize);
            return await ProxyResponse(response, "GetAllParts");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting all parts via BFF");
            throw;
        }
    }

    /// <summary>
    /// Gets a specific part by its ID.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the part.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting part by ID {Id} via BFF", id);
            var response = await _catalogService.GetPartByIdAsync(id);
            return await ProxyResponse(response, $"GetPartById({id})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting part {Id} via BFF", id);
            throw;
        }
    }

    /// <summary>
    /// Gets a specific part by its Part Code and Country Code.
    /// </summary>
    /// <param name="code">[REQUIRED] The unique part code.</param>
    /// <param name="countryCode">[REQUIRED] The 2-character country code.</param>
    [HttpGet("code/{code}/country/{countryCode}")]
    [ProducesResponseType(typeof(PartDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code, string countryCode)
    {
        try
        {
            _logger.LogInformation("Getting part by code {Code} and country {CountryCode} via BFF", code, countryCode);
            var response = await _catalogService.GetPartByCodeAsync(code, countryCode);
            return await ProxyResponse(response, $"GetPartByCode({code}, {countryCode})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting part by code {Code} via BFF", code);
            throw;
        }
    }

    /// <summary>
    /// Creates a new part.
    /// </summary>
    /// <param name="request">[REQUIRED] The part creation request details.</param>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromForm] CreatePartRequest request)
    {
        try
        {
            _logger.LogInformation("Creating new part via BFF. PartCode: {PartCode}", request.PartCode);
            var response = await _catalogService.CreatePartAsync(request);
            return await ProxyResponse(response, "CreatePart");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating part {PartCode} via BFF", request.PartCode);
            throw;
        }
    }

    /// <summary>
    /// Updates an existing part.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the part.</param>
    /// <param name="request">[REQUIRED] The part update request details.</param>
    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdatePartRequest request)
    {
        try
        {
            _logger.LogInformation("Updating part {Id} via BFF", id);
            var response = await _catalogService.UpdatePartAsync(id, request);
            return await ProxyResponse(response, $"UpdatePart({id})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating part {Id} via BFF", id);
            throw;
        }
    }

    /// <summary>
    /// Soft deletes an existing part.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the part to delete.</param>
    /// <param name="updatedBy">[OPTIONAL] The user performing the deletion.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] string? updatedBy)
    {
        try
        {
            _logger.LogInformation("Deleting part {Id} via BFF by user {User}", id, updatedBy);
            var response = await _catalogService.DeletePartAsync(id, updatedBy);
            return await ProxyResponse(response, $"DeletePart({id})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting part {Id} via BFF", id);
            throw;
        }
    }

    #region Lookups

    /// <summary>
    /// Gets all active component groups for lookup.
    /// </summary>
    [HttpGet("lookup/groups")]
    [ProducesResponseType(typeof(IEnumerable<ComponentGroupDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetGroupsLookup()
    {
        try
        {
            _logger.LogInformation("Getting component groups lookup via BFF");
            var response = await _catalogService.GetPartGroupsLookupAsync();
            return await ProxyResponse(response, "GetGroupsLookup");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting groups lookup via BFF");
            throw;
        }
    }

    /// <summary>
    /// Gets component types filtered by group for lookup.
    /// </summary>
    [HttpGet("lookup/types/{groupId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ComponentTypeDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetTypesLookup(Guid groupId)
    {
        try
        {
            _logger.LogInformation("Getting component types lookup for group {GroupId} via BFF", groupId);
            var response = await _catalogService.GetPartTypesLookupAsync(groupId);
            return await ProxyResponse(response, $"GetTypesLookup({groupId})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting types lookup via BFF for group {Group}", groupId);
            throw;
        }
    }

    /// <summary>
    /// Gets component names filtered by type for lookup.
    /// </summary>
    [HttpGet("lookup/names/{typeId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<ComponentNameDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetNamesLookup(Guid typeId)
    {
        try
        {
            _logger.LogInformation("Getting component names lookup for type {TypeId} via BFF", typeId);
            var response = await _catalogService.GetPartNamesLookupAsync(typeId);
            return await ProxyResponse(response, $"GetNamesLookup({typeId})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting names lookup via BFF for type {Type}", typeId);
            throw;
        }
    }

    #endregion

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
