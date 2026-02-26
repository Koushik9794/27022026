using GssWebApi.Dto;
using GssWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GssWebApi.Api.Controllers;

/// <summary>
/// Admin controller for managing Countries.
/// Proxies requests to the Catalog Service.
/// </summary>
[ApiController]
[Route("api/admin/countries")]
[Produces("application/json")]
// [Authorize(Roles = "Admin")] // Uncomment when auth is configured
public class AdminCountriesController : ControllerBase
{
    private readonly ICatalogServiceClient _catalogClient;
    private readonly ILogger<AdminCountriesController> _logger;

    public AdminCountriesController(ICatalogServiceClient catalogClient, ILogger<AdminCountriesController> logger)
    {
        _catalogClient = catalogClient;
        _logger = logger;
    }

    /// <summary>
    /// Get all countries.
    /// </summary>
    /// <param name="includeInactive">[OPTIONAL] Whether to include inactive countries (default: false).</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CountryDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCountries([FromQuery] bool includeInactive = false)
    {
        try
        {
            _logger.LogInformation("Getting all countries via BFF. IncludeInactive: {IncludeInactive}", includeInactive);
            var response = await _catalogClient.GetCountriesAsync(includeInactive);
            return await ProxyResponse(response, "GetCountries");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting countries via BFF");
            throw;
        }
    }

    /// <summary>
    /// Get a country by ID.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the country.</param>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CountryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCountryById(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting country by ID {Id} via BFF", id);
            var response = await _catalogClient.GetCountryByIdAsync(id);
            return await ProxyResponse(response, $"GetCountryById({id})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting country {Id} via BFF", id);
            throw;
        }
    }

    /// <summary>
    /// Get a country by its 2-character ISO code.
    /// </summary>
    /// <param name="code">[REQUIRED] The 2-character ISO country code (e.g., 'IN', 'US').</param>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(CountryDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCountryByCode(string code)
    {
        try
        {
            _logger.LogInformation("Getting country by code {Code} via BFF", code);
            var response = await _catalogClient.GetCountryByCodeAsync(code);
            return await ProxyResponse(response, $"GetCountryByCode({code})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting country by code {Code} via BFF", code);
            throw;
        }
    }

    /// <summary>
    /// Create a new country.
    /// </summary>
    /// <param name="request">[REQUIRED] The country creation request details.</param>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCountry([FromBody] CreateCountryRequest request)
    {
        try
        {
            _logger.LogInformation("Creating new country via BFF. Name: {Name}", request.CountryName);
            var response = await _catalogClient.CreateCountryAsync(request);
            return await ProxyResponse(response, "CreateCountry");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating country via BFF");
            throw;
        }
    }

    /// <summary>
    /// Update an existing country.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the country.</param>
    /// <param name="request">[REQUIRED] The country update request details.</param>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCountry(Guid id, [FromBody] UpdateCountryRequest request)
    {
        try
        {
            _logger.LogInformation("Updating country {Id} via BFF", id);
            var response = await _catalogClient.UpdateCountryAsync(id, request);
            return await ProxyResponse(response, $"UpdateCountry({id})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating country {Id} via BFF", id);
            throw;
        }
    }

    /// <summary>
    /// Delete a country.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique identifier of the country to delete.</param>
    /// <param name="updatedBy">[OPTIONAL] User identifier for the update record.</param>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCountry(Guid id, [FromQuery] string? updatedBy = null)
    {
        try
        {
            _logger.LogInformation("Deleting country {Id} via BFF by user {User}", id, updatedBy);
            var response = await _catalogClient.DeleteCountryAsync(id, updatedBy);
            return await ProxyResponse(response, $"DeleteCountry({id})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting country {Id} via BFF", id);
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
