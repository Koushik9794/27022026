using GssWebApi.Dto;
using GssWebApi.Services;
using Microsoft.AspNetCore.Mvc;
namespace GssWebApi.src.api.controllers;

[ApiController]
[Route("api/admin/mhe-type")]
[Produces("application/json")]
public class AdminMheTypesController : ControllerBase
{
    private readonly ILogger<AdminMheTypesController> _logger;
    private readonly ICatalogServiceClient _catalogService;
    public AdminMheTypesController(ILogger<AdminMheTypesController> logger, ICatalogServiceClient catalogService)
    {
        _logger = logger;
        _catalogService = catalogService;
    }
    /// <summary>
    /// Gets all MHEs combined with Attribute Definitions for the browser.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<MheDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll()
    {
        var response = await _catalogService.GetMheTypesAsync();
        return await ProxyResponse(response);
    }
    /// <summary>
    /// Gets a specific MHE by ID combined with Attribute Definitions.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the MHE.</param>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(MheDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var response = await _catalogService.GetMheTypeByIdAsync(id);
        return await ProxyResponse(response);
    }
    /// <summary>
    /// Creates a new MHE
    /// </summary>
    /// <param name="request">[REQUIRED] The MHE creation request details.</param>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Create([FromForm] CreateMheRequest request)
    {
        var response = await _catalogService.CreateMheTypeAsync(request);
        return await ProxyResponse(response);
    }
    /// <summary>
    /// Updates an existing MHE
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the MHE.</param>
    /// <param name="request">[REQUIRED] The MHE update request details.</param>
    [HttpPut("{id}")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdateMheRequest request)
    {
        var response = await _catalogService.UpdateMheTypeAsync(id, request);
        return await ProxyResponse(response);
    }

    /// <summary>
    /// Delete an existing MHE
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the MHE to delete.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var response = await _catalogService.DeleteMheTypeAsync(id);
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
