using GssWebApi.Dto;
using GssWebApi.src.Services;
using Microsoft.AspNetCore.Mvc;

namespace GssWebApi.Api.Controllers;

/// <summary>
/// BFF controller for Storage Configuration and Design operations.
/// Proxies requests to the Configuration Service.
/// </summary>
[ApiController]
[Route("api/v1/storage-configurations")]
[Produces("application/json")]
public class StorageConfigurationsController : ControllerBase
{
    private readonly IConfigurationService _configClient;
    private readonly ILogger<StorageConfigurationsController> _logger;

    public StorageConfigurationsController(IConfigurationService configClient, ILogger<StorageConfigurationsController> logger)
    {
        _configClient = configClient;
        _logger = logger;
    }

    /// <summary>
    /// Autosave endpoint - updates design data for an existing storage configuration.
    /// Called frequently from UI as designer makes changes.
    /// </summary>
    /// <param name="id">[REQUIRED] Configuration ID.</param>
    /// <param name="request">[REQUIRED] Design data to save.</param>
    [HttpPut("{id:guid}/design")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> SaveDesign(Guid id, [FromBody] SaveDesignRequest request)
    {
        var response = await _configClient.SaveDesignAsync(id, request);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Create a new storage configuration for a version.
    /// </summary>
    /// <param name="request">[REQUIRED] Configuration details.</param>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateStorageConfigurationRequest request)
    {
        var response = await _configClient.CreateStorageConfigurationAsync(request);
        return await ProxyResponse(response);
    }

    private async Task<IActionResult> ProxyResponse(HttpResponseMessage response)
    {
        var content = await response.Content.ReadAsStringAsync();
        return new ContentResult
        {
            Content = content,
            ContentType = "application/json",
            StatusCode = (int)response.StatusCode
        };
    }
}
