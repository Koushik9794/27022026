using GssWebApi.Dto;
using GssWebApi.src.Services;
using Microsoft.AspNetCore.Mvc;

namespace GssWebApi.Api.Controllers;

/// <summary>
/// BFF controller for Rack Configuration operations.
/// Proxies requests to the Configuration Service.
/// </summary>
[ApiController]
[Route("api/v1/rack-configurations")]
[Produces("application/json")]
public class RackConfigurationsController : ControllerBase
{
    private readonly IConfigurationService _configClient;
    private readonly ILogger<RackConfigurationsController> _logger;

    public RackConfigurationsController(IConfigurationService configClient, ILogger<RackConfigurationsController> logger)
    {
        _configClient = configClient;
        _logger = logger;
    }

    /// <summary>
    /// Get all rack configurations.
    /// </summary>
    /// <param name="includeInactive">[OPTIONAL] Whether to include inactive configurations (default: false).</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<RackConfigurationResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var response = await _configClient.GetAllRackConfigurationsAsync(includeInactive);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Get rack configuration by ID.
    /// </summary>
    /// <param name="id">[REQUIRED] Configuration unique identifier.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(RackConfigurationResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _configClient.GetRackConfigurationByIdAsync(id);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Create a new rack configuration.
    /// </summary>
    /// <param name="request">[REQUIRED] Configuration details.</param>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateRackConfigurationRequest request)
    {
        var response = await _configClient.CreateRackConfigurationAsync(request);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Update an existing rack configuration.
    /// </summary>
    /// <param name="id">[REQUIRED] Configuration ID to update.</param>
    /// <param name="request">[REQUIRED] Updated configuration details.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRackConfigurationRequest request)
    {
        var response = await _configClient.UpdateRackConfigurationAsync(id, request);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Delete a rack configuration.
    /// </summary>
    /// <param name="id">[REQUIRED] Configuration ID to delete.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _configClient.DeleteRackConfigurationAsync(id);
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
