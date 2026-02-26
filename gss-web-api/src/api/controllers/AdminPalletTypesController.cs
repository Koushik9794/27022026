using GssWebApi.Dto;
using GssWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GssWebApi.Api.Controllers;

/// <summary>
/// Admin controller for managing Pallet Types.
/// Proxies requests to the Catalog Service.
/// </summary>
[ApiController]
[Route("api/admin/pallet-types")]
[Produces("application/json")]
// [Authorize(Roles = "Admin")] // Uncomment when auth is configured
public class AdminPalletTypesController : ControllerBase
{
    private readonly ICatalogServiceClient _catalogClient;
    private readonly ILogger<AdminPalletTypesController> _logger;

    public AdminPalletTypesController(ICatalogServiceClient catalogClient, ILogger<AdminPalletTypesController> logger)
    {
        _catalogClient = catalogClient;
        _logger = logger;
    }

    /// <summary>
    /// Get all pallet types.
    /// </summary>
    /// <param name="includeInactive">[OPTIONAL] Whether to include inactive types (default: false).</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PalletDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var response = await _catalogClient.GetPalletTypesAsync(includeInactive);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Get a pallet type by ID.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the pallet type.</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(PalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _catalogClient.GetPalletTypeByIdAsync(id);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Get a pallet type by code.
    /// </summary>
    /// <param name="code">[REQUIRED] The unique code of the pallet type.</param>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(PalletDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetByCode(string code)
    {
        var response = await _catalogClient.GetPalletTypeByCodeAsync(code);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Create a new pallet type.
    /// </summary>
    /// <param name="request">[REQUIRED] The pallet type creation request details.</param>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromForm] CreatePalletRequest request)
    {
        var response = await _catalogClient.CreatePalletTypeAsync(request);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Update an existing pallet type.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the pallet type.</param>
    /// <param name="request">[REQUIRED] The pallet type update request details.</param>
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdatePalletRequest request)
    {
        var response = await _catalogClient.UpdatePalletTypeAsync(id, request);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Delete a pallet type.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the pallet type to delete.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _catalogClient.DeletePalletTypeAsync(id);
        return await ProxyResponse(response);
    }

    [HttpDelete]
    [HttpPut]
    [ApiExplorerSettings(IgnoreApi = true)]
    public IActionResult MissingId()
    {
        return BadRequest(new { message = "it should not be empty id" });
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
