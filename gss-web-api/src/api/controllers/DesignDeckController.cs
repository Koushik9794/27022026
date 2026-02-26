using System.Text.Json;
using GssWebApi.Services;
using GssWebApi.Dto;
using Microsoft.AspNetCore.Mvc;
namespace GssWebApi.Api.Controllers;
/// <summary>
/// DesignDeck configuration and palette endpoints
/// </summary>
[ApiController]
[Route("api/v1/design-deck")]
[Produces("application/json")]
public class DesignDeckController : ControllerBase
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IConfiguration _configuration;
    private readonly ILogger<DesignDeckController> _logger;
    private readonly ICatalogServiceClient _catalogService;
    public DesignDeckController(
        IHttpClientFactory httpClientFactory,
        IConfiguration configuration,
        ILogger<DesignDeckController> logger,
        ICatalogServiceClient catalogService)
    {
        _httpClientFactory = httpClientFactory;
        _configuration = configuration;
        _logger = logger;
        _catalogService = catalogService;
    }
    /// <summary>
    /// Get the DesignDeck component palette
    /// </summary>
    /// <remarks>
    /// Returns the palette configuration containing all available components
    /// that dealers can drag and drop in the DesignDeck UI.
    /// 
    /// **User Journey:** 03_Configure_Warehouse (DesignDeck)
    /// 
    /// **What is the Palette?**
    /// The palette is a menu/component library for the DesignDeck interface.
    /// It contains all the building blocks dealers can use to design their warehouse:
    /// 
    /// - **Warehouse Types**: Flat, Roof, Inclined
    /// - **Civil Components**: Categorization for CivilComponents
    /// 
    /// **Frontend Usage:**
    /// 1. Load palette on DesignDeck initialization
    /// 2. Display components in a sidebar/menu
    /// 3. Allow dealers to drag and drop components onto the canvas
    /// 4. Use component attributes (DEPTH, WIDTH, HEIGHT) for rendering
    /// 
    /// **Sample Response Structure:**
    /// ```json
    /// {
    ///   "paletteVersion": "v1",
    ///   "groups": [
    ///     {
    ///       "id": "warehouseTypes",
    ///       "items": [
    ///         {
    ///           "id": "2ba29789-40b2-4a6c-bcbf-1170f1529ea3",
    ///           "Code": "FLAT",
    ///           "name": "Flat Warehouse",
    ///           "dxf": {
    ///             "fileName": "flat_layout.dxf",
    ///             "filePath": "/warehouse-templates/flat/flat_layout.dxf"
    ///              }
    ///              "json": {
    ///             "fileName": "flat_layout.json",
    ///             "filePath": "/warehouse-templates/flat/flat_layout.json"
    ///              }
    ///         }
    ///       ]
    ///     }
    ///   ]
    /// }
    /// ```
    /// </remarks>
    /// <response code="200">Returns the palette configuration JSON</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Catalog service unavailable</response>



    [HttpGet("palette")]
    [ProducesResponseType(typeof(PaletteResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetPalette()
    {
        var roles = new List<string>
    {
        "ADMIN",
        "DESIGNER",
        "CIVIL_DESIGNER"
    };


        // Your service returns HttpResponseMessage
        var apiResponse = await _catalogService.GetPalettesAsync();


        // Deserialize content
        var json = await apiResponse.Content.ReadAsStringAsync();
        var palettes = JsonSerializer.Deserialize<JsonElement>(json);


        var response = new PaletteResponse
        {
            PaletteVersion = "v1",
            Roles = roles,
            catalogservice=palettes
        };


        return ProxyResponse(response);
    }



    /// <summary>
    /// Get the DesignDeck initialization configuration
    /// </summary>
    /// <remarks>
    /// Returns the initialization configuration for the DesignDeck.
    /// This includes default settings and initial state for the design canvas.
    /// 
    /// **User Journey:** 03_Configure_Warehouse (DesignDeck initialization)
    /// </remarks>
    /// <response code="200">Returns the initialization configuration JSON</response>
    /// <response code="500">Internal server error</response>
    /// <response code="503">Catalog service unavailable</response>
    /// <param name="ct">[OPTIONAL] Cancellation token.</param>
    [HttpGet("init")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    [ProducesResponseType(StatusCodes.Status503ServiceUnavailable)]
    public async Task<IActionResult> GetInitConfig([FromQuery] CancellationToken ct)
    {
        try
        {
            var catalogServiceUrl = _configuration["ServiceEndpoints:CatalogService"]
                ?? "http://localhost:5002";
            var client = _httpClientFactory.CreateClient();
            var response = await client.GetAsync(
                $"{catalogServiceUrl}/api/v1/design-deck/init",
                ct);
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Failed to fetch init config from catalog service. Status: {StatusCode}",
                    response.StatusCode);
                return StatusCode(
                    (int)response.StatusCode,
                    new { error = "Failed to load initialization configuration" });
            }
            var json = await response.Content.ReadAsStringAsync(ct);
            return Content(json, "application/json");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Catalog service is unavailable");
            return StatusCode(503, new
            {
                error = "Catalog service unavailable",
                message = "Unable to connect to catalog service"
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching init configuration");
            return StatusCode(500, new
            {
                error = "Failed to load initialization configuration",
                message = ex.Message
            });
        }
    }



    private IActionResult ProxyResponse(object result)
    {
        var json = JsonSerializer.Serialize(result, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        });

        return new ContentResult
        {
            Content = json,
            ContentType = "application/json",
            StatusCode = 200
        };
    }


}
