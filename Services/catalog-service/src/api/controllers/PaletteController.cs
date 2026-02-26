using CatalogService.Application.queries.palette;
using CatalogService.Application.dtos;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Wolverine;


namespace CatalogService.Api.Controllers;

/// <summary>
/// Palette initialization data endpoints
/// Serves static palette configuration for DesignDeck UI component menu
/// </summary>
/// <remarks>
/// This is a temporary endpoint that serves static JSON files.
/// In the future, this data should be accessed via proper Catalog APIs (SKU, Pallet, MHE endpoints).
/// </remarks>
[ApiController]
[Route("api/v1/palette")]
public class PaletteController : ControllerBase
{
    private readonly IMessageBus _bus;
    private readonly IWebHostEnvironment _env;
    private readonly ILogger<PaletteController> _logger;

    public PaletteController(IWebHostEnvironment env, ILogger<PaletteController> logger, IMessageBus bus)
    {
        _env = env;
        _logger = logger;
        _bus = bus;
    }

    /// <summary>
    /// Get the palette configuration.
    /// </summary>
    /// <param name="ct">[OPTIONAL] Cancellation token.</param>
    /// <remarks>
    /// Returns the palette configuration JSON containing all available components
    /// for the DesignDeck UI component menu.
    /// </remarks>
    /// <response code="200">Returns the palette configuration JSON</response>
    /// <response code="404">Palette file not found</response>
    /// <response code="500">Internal server error</response>
    [HttpGet]
    [ProducesResponseType(typeof(CivilDesignerResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetPalette(CancellationToken ct)
    {
        try
        {
            var result = await _bus.InvokeAsync<CivilDesignerResponseDto>(new GetCivilDesignerDataQuery());
            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reading palette configuration");
            return StatusCode(500, new 
            { 
                error = "Failed to load palette configuration",
                message = ex.Message 
            });
        }
    }
}

