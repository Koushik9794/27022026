using GssWebApi.Dto;
using GssWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GssWebApi.Api.Controllers;

/// <summary>
/// Admin controller for managing Exchange Rates.
/// Proxies requests to the Catalog Service.
/// </summary>
[ApiController]
[Route("api/admin/exchange-rates")]
[Produces("application/json")]
// [Authorize(Roles = "Admin")] // Uncomment when auth is configured
public class AdminExchangeRatesController : ControllerBase
{
    private readonly ICatalogServiceClient _catalogClient;
    private readonly ILogger<AdminExchangeRatesController> _logger;

    public AdminExchangeRatesController(ICatalogServiceClient catalogClient, ILogger<AdminExchangeRatesController> logger)
    {
        _catalogClient = catalogClient;
        _logger = logger;
    }

    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ExchangeRateDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetExchangeRates([FromQuery] bool includeInactive = false)
    {
        try
        {
            _logger.LogInformation("Getting all exchange rates via BFF. IncludeInactive: {IncludeInactive}", includeInactive);
            var response = await _catalogClient.GetExchangeRatesAsync(includeInactive);
            return await ProxyResponse(response, "GetExchangeRates");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting exchange rates via BFF");
            throw;
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(ExchangeRateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExchangeRateById(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting exchange rate by ID {Id} via BFF", id);
            var response = await _catalogClient.GetExchangeRateByIdAsync(id);
            return await ProxyResponse(response, $"GetExchangeRateById({id})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting exchange rate {Id} via BFF", id);
            throw;
        }
    }

    [HttpGet("latest")]
    [ProducesResponseType(typeof(ExchangeRateDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetLatestExchangeRate([FromQuery] string baseCurrency, [FromQuery] string quoteCurrency)
    {
        try
        {
            _logger.LogInformation("Getting latest exchange rate {Base}/{Quote} via BFF", baseCurrency, quoteCurrency);
            var response = await _catalogClient.GetLatestExchangeRateAsync(baseCurrency, quoteCurrency);
            return await ProxyResponse(response, $"GetLatestExchangeRate({baseCurrency}/{quoteCurrency})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting latest exchange rate via BFF");
            throw;
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateExchangeRate([FromBody] CreateExchangeRateRequest request)
    {
        try
        {
            _logger.LogInformation("Creating new exchange rate via BFF");
            var response = await _catalogClient.CreateExchangeRateAsync(request);
            return await ProxyResponse(response, "CreateExchangeRate");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating exchange rate via BFF");
            throw;
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateExchangeRate(Guid id, [FromBody] UpdateExchangeRateRequest request)
    {
        try
        {
            _logger.LogInformation("Updating exchange rate {Id} via BFF", id);
            var response = await _catalogClient.UpdateExchangeRateAsync(id, request);
            return await ProxyResponse(response, $"UpdateExchangeRate({id})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating exchange rate {Id} via BFF", id);
            throw;
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteExchangeRate(Guid id, [FromQuery] string? updatedBy = null)
    {
        try
        {
            _logger.LogInformation("Deleting exchange rate {Id} via BFF by user {User}", id, updatedBy);
            var response = await _catalogClient.DeleteExchangeRateAsync(id, updatedBy);
            return await ProxyResponse(response, $"DeleteExchangeRate({id})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting exchange rate {Id} via BFF", id);
            throw;
        }
    }

    private async Task<IActionResult> ProxyResponse(HttpResponseMessage response, string operation)
    {
        var content = await response.Content.ReadAsStringAsync();
        if (!response.IsSuccessStatusCode)
            _logger.LogWarning("Downstream call {Operation} failed with status {StatusCode}. Body: {Content}", operation, response.StatusCode, content);
        return new ContentResult { Content = content, ContentType = "application/json", StatusCode = (int)response.StatusCode };
    }
}
