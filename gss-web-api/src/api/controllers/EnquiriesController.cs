using GssWebApi.Dto;
using GssWebApi.src.Services;
using Microsoft.AspNetCore.Mvc;

namespace GssWebApi.Api.Controllers;

/// <summary>
/// BFF controller for managing Enquiries and Configurations.
/// Proxies requests to the Configuration Service.
/// </summary>
[ApiController]
[Route("api/v1/enquiries")]
[Produces("application/json")]
public class EnquiriesController : ControllerBase
{
    private readonly IConfigurationService _configClient;
    private readonly ILogger<EnquiriesController> _logger;

    public EnquiriesController(IConfigurationService configClient, ILogger<EnquiriesController> logger)
    {
        _configClient = configClient;
        _logger = logger;
    }

    // ============ Enquiry Endpoints ============

    /// <summary>
    /// Get all enquiries.
    /// </summary>
    /// <param name="includeDeleted">[OPTIONAL] Whether to include deleted enquiries (default: false).</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<EnquiryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeDeleted = false)
    {
        var response = await _configClient.GetAllEnquiriesAsync(includeDeleted);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Get enquiry by ID.
    /// </summary>
    /// <param name="id">[REQUIRED] Enquiry unique identifier.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(EnquiryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _configClient.GetEnquiryByIdAsync(id);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Get enquiry by its external reference ID.
    /// </summary>
    /// <param name="externalId">[REQUIRED] External system identifier.</param>
    [HttpGet("external/{externalId}")]
    [ProducesResponseType(typeof(EnquiryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByExternalId(string externalId)
    {
        var response = await _configClient.GetEnquiryByExternalIdAsync(externalId);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Create a new enquiry.
    /// </summary>
    /// <param name="request">[REQUIRED] Enquiry details.</param>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromBody] CreateEnquiryRequest request)
    {
        var response = await _configClient.CreateEnquiryAsync(request);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Update an existing enquiry.
    /// </summary>
    /// <param name="id">[REQUIRED] Enquiry ID to update.</param>
    /// <param name="request">[REQUIRED] Updated enquiry details.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateEnquiryRequest request)
    {
        var response = await _configClient.UpdateEnquiryAsync(id, request);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Delete an enquiry.
    /// </summary>
    /// <param name="id">[REQUIRED] Enquiry ID to delete.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _configClient.DeleteEnquiryAsync(id);
        return await ProxyResponse(response);
    }

    // ============ Configuration Endpoints ============

    /// <summary>
    /// Get all configurations for a specific enquiry.
    /// </summary>
    /// <param name="enquiryId">[REQUIRED] Parent enquiry ID.</param>
    /// <param name="includeInactive">[OPTIONAL] Include inactive configurations.</param>
    [HttpGet("{enquiryId:guid}/configurations")]
    [ProducesResponseType(typeof(IEnumerable<ConfigurationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConfigurations(Guid enquiryId, [FromQuery] bool includeInactive = false)
    {
        var response = await _configClient.GetConfigurationsByEnquiryIdAsync(enquiryId, includeInactive);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Get list of configurations for an enquiry (summary view).
    /// </summary>
    /// <param name="enquiryId">[REQUIRED] Enquiry ID.</param>
    /// <param name="includeInactive">[OPTIONAL] Include inactive.</param>
    [HttpGet("{enquiryId:guid}/configurationList")]
    [ProducesResponseType(typeof(IEnumerable<ConfigurationDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetConfigurationList(Guid enquiryId, [FromQuery] bool includeInactive = false)
    {
        var response = await _configClient.GetConfigurationListAsync(enquiryId, includeInactive);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Get a specific configuration by ID.
    /// </summary>
    /// <param name="enquiryId">[REQUIRED] Parent enquiry ID.</param>
     /// <param name="configId">[REQUIRED] Configuration ID.</param>
    [HttpGet("{enquiryId:guid}/configurations/{configId:guid}")]
    [ProducesResponseType(typeof(ConfigurationDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetConfiguration(Guid enquiryId, Guid configId)
    {
        var response = await _configClient.GetConfigurationByIdAsync(enquiryId, configId);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Create a new configuration for an enquiry.
    /// </summary>
    /// <param name="enquiryId">[REQUIRED] Parent enquiry ID.</param>
    /// <param name="request">[REQUIRED] Configuration details.</param>
    [HttpPost("{enquiryId:guid}/configurations")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateConfiguration(Guid enquiryId, [FromBody] EnquiryCreateConfigurationRequest request)
    {
        var response = await _configClient.CreateConfigurationAsync(enquiryId, request);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Delete a configuration.
    /// </summary>
    /// <param name="enquiryId">[REQUIRED] Parent enquiry ID.</param>
    /// <param name="configId">[REQUIRED] Configuration ID to delete.</param>
    [HttpDelete("{enquiryId:guid}/configurations/{configId:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteConfiguration(Guid enquiryId, Guid configId)
    {
        var response = await _configClient.DeleteConfigurationAsync(enquiryId, configId);
        return await ProxyResponse(response);
    }

    // ============ Version Endpoints ============

    /// <summary>
    /// Create a new version for a configuration.
    /// </summary>
    /// <param name="enquiryId">[REQUIRED] Parent enquiry ID.</param>
    /// <param name="configId">[REQUIRED] Configuration ID.</param>
    /// <param name="request">[REQUIRED] Version details.</param>
    [HttpPost("{enquiryId:guid}/configurations/{configId:guid}/versions")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateVersion(Guid enquiryId, Guid configId, [FromBody] CreateVersionRequest request)
    {
        var response = await _configClient.CreateVersionAsync(enquiryId, configId, request);
        return await ProxyResponse(response);
    }

    [HttpPut("{enquiryId:guid}/configurations/{configId:guid}/version-lock/{versionNumber:int}")]
    public async Task<IActionResult> LockVersion(Guid enquiryId, Guid configId, int versionNumber, [FromBody] CreateVersionRequest request)
    {
        var response = await _configClient.LockVersionAsync(enquiryId, configId, versionNumber, request);
        return await ProxyResponse(response);
    }

    [HttpPut("{enquiryId:guid}/configurations/{configId:guid}/un-lock-version/{versionNumber:int}")]
    public async Task<IActionResult> UnlockVersion(Guid enquiryId, Guid configId, int versionNumber, [FromBody] CreateVersionRequest request)
    {
        var response = await _configClient.UnlockVersionAsync(enquiryId, configId, versionNumber, request);
        return await ProxyResponse(response);
    }

    // ============ Civil Layout & Rack Layout Endpoints ============

    [HttpGet("{enquiryId:guid}/configurations/{configId:guid}/civil-layout")]
    public async Task<IActionResult> GetCivilLayoutList(Guid enquiryId, Guid configId)
    {
        var response = await _configClient.GetCivilLayoutListAsync(enquiryId, configId);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Get civil layout by ID.
    /// </summary>
    /// <param name="id">[REQUIRED] Civil layout ID.</param>
    [HttpGet("configurations/civil-layout/{id:guid}")]
    [ProducesResponseType(typeof(CivilLayoutDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCivilLayoutById(Guid id)
    {
        var response = await _configClient.GetCivilLayoutByIdAsync(id);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Update an existing civil layout.
    /// </summary>
    /// <param name="id">[REQUIRED] Civil layout ID to update.</param>
    /// <param name="request">[REQUIRED] Updated layout data (multipart/form-data).</param>
    [HttpPut("configurations/civil-layout/{id:guid}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCivilLayout(Guid id, [FromForm] UpdateCivilLayoutRequest request)
    {
        var response = await _configClient.UpdateCivilLayoutAsync(id, request);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Create a new civil layout for a configuration.
    /// </summary>
    /// <param name="enquiryId">[REQUIRED] Parent enquiry ID.</param>
    /// <param name="configId">[REQUIRED] Configuration ID.</param>
    /// <param name="request">[REQUIRED] Layout data (multipart/form-data).</param>
    [HttpPost("{enquiryId:guid}/configurations/{configId:guid}/civil-layout")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCivilLayout(Guid enquiryId, Guid configId, [FromForm] SaveCivilLayoutRequest request)
    {
        var response = await _configClient.CreateCivilLayoutAsync(enquiryId, configId, request);
        return await ProxyResponse(response);
    }

    [HttpGet("{enquiryId:guid}/configurations/{configId:guid}/versions/{versionNumber:int}/rack-layout")]
    public async Task<IActionResult> GetRackLayout(Guid enquiryId, Guid configId, int versionNumber)
    {
        var response = await _configClient.GetRackLayoutAsync(enquiryId, configId, versionNumber);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Update an existing rack layout.
    /// </summary>
    /// <param name="id">[REQUIRED] Rack layout ID.</param>
    /// <param name="request">[REQUIRED] Updated layout data (multipart/form-data).</param>
    [HttpPut("configurations/rack-layout/{id:guid}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateRackLayout(Guid id, [FromForm] SaveRackLayoutRequest request)
    {
        var response = await _configClient.UpdateRackLayoutAsync(id, request);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Create a new rack layout.
    /// </summary>
    /// <param name="configId">[REQUIRED] Configuration ID.</param>
    /// <param name="civilVersion">[REQUIRED] Civil version index.</param>
    /// <param name="configVersion">[REQUIRED] Configuration version index.</param>
    /// <param name="request">[REQUIRED] Layout data (multipart/form-data).</param>
    [HttpPost("configurations/{configId:guid}/civil-versions/{civilVersion:int}/config-versions/{configVersion:int}/rack-layout")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateRackLayout(Guid configId, int civilVersion, int configVersion, [FromForm] SaveRackLayoutRequest request)
    {

        var response = await _configClient.CreateRackLayoutAsync(configId, civilVersion, configVersion, request);
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
