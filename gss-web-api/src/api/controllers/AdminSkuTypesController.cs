using GssWebApi.Dto;
using GssWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GssWebApi.Api.Controllers;

/// <summary>
/// Admin controller for managing SKU Types.
/// Proxies requests to the Catalog Service.
/// </summary>
[ApiController]
[Route("api/admin/sku-types")]
[Produces("application/json")]
// [Authorize(Roles = "Admin")] // Uncomment when auth is configured
public class AdminSkuTypesController : ControllerBase
{
    private readonly ICatalogServiceClient _catalogClient;
    private readonly ILogger<AdminSkuTypesController> _logger;

    public AdminSkuTypesController(ICatalogServiceClient catalogClient, ILogger<AdminSkuTypesController> logger)
    {
        _catalogClient = catalogClient;
        _logger = logger;
    }

    /// <summary>
    /// Get all SKU types.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<SkuDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var response = await _catalogClient.GetSkuTypesAsync();
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Get a SKU type by ID.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the SKU type.</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SkuDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _catalogClient.GetSkuTypeByIdAsync(id);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Create a new SKU type.
    /// </summary>
    /// <param name="request">[REQUIRED] The SKU type creation request details.</param>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromForm] CreateSkuRequest request)
    {
        var response = await _catalogClient.CreateSkuTypeAsync(request);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Update an existing SKU type.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the SKU type.</param>
    /// <param name="request">[REQUIRED] The SKU type update request details.</param>
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateSkuRequest request)
    {
        var response = await _catalogClient.UpdateSkuTypeAsync(id, request);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Delete a SKU type.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the SKU type to delete.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _catalogClient.DeleteSkuTypeAsync(id);
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
