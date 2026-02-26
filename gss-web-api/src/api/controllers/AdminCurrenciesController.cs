using GssWebApi.Dto;
using GssWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GssWebApi.Api.Controllers;

/// <summary>
/// Admin controller for managing Currencies.
/// Proxies requests to the Catalog Service.
/// </summary>
[ApiController]
[Route("api/admin/currencies")]
[Produces("application/json")]
// [Authorize(Roles = "Admin")] // Uncomment when auth is configured
public class AdminCurrenciesController : ControllerBase
{
    private readonly ICatalogServiceClient _catalogClient;
    private readonly ILogger<AdminCurrenciesController> _logger;

    public AdminCurrenciesController(ICatalogServiceClient catalogClient, ILogger<AdminCurrenciesController> logger)
    {
        _catalogClient = catalogClient;
        _logger = logger;
    }

    /// <summary>
    /// Get all currencies.
    /// </summary>
    /// <param name="includeInactive">[OPTIONAL] Whether to include inactive currencies (default: false).</param>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CurrencyDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetCurrencies([FromQuery] bool includeInactive = false)
    {
        try
        {
            _logger.LogInformation("Getting all currencies via BFF. IncludeInactive: {IncludeInactive}", includeInactive);
            var response = await _catalogClient.GetCurrenciesAsync(includeInactive);
            return await ProxyResponse(response, "GetCurrencies");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting currencies via BFF");
            throw;
        }
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrencyById(Guid id)
    {
        try
        {
            _logger.LogInformation("Getting currency by ID {Id} via BFF", id);
            var response = await _catalogClient.GetCurrencyByIdAsync(id);
            return await ProxyResponse(response, $"GetCurrencyById({id})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting currency {Id} via BFF", id);
            throw;
        }
    }

    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(CurrencyDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetCurrencyByCode(string code)
    {
        try
        {
            _logger.LogInformation("Getting currency by code {Code} via BFF", code);
            var response = await _catalogClient.GetCurrencyByCodeAsync(code);
            return await ProxyResponse(response, $"GetCurrencyByCode({code})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while getting currency by code {Code} via BFF", code);
            throw;
        }
    }

    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateCurrency([FromBody] CreateCurrencyRequest request)
    {
        try
        {
            _logger.LogInformation("Creating new currency via BFF");
            var response = await _catalogClient.CreateCurrencyAsync(request);
            return await ProxyResponse(response, "CreateCurrency");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while creating currency via BFF");
            throw;
        }
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateCurrency(Guid id, [FromBody] UpdateCurrencyRequest request)
    {
        try
        {
            _logger.LogInformation("Updating currency {Id} via BFF", id);
            var response = await _catalogClient.UpdateCurrencyAsync(id, request);
            return await ProxyResponse(response, $"UpdateCurrency({id})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while updating currency {Id} via BFF", id);
            throw;
        }
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteCurrency(Guid id, [FromQuery] string? updatedBy = null)
    {
        try
        {
            _logger.LogInformation("Deleting currency {Id} via BFF by user {User}", id, updatedBy);
            var response = await _catalogClient.DeleteCurrencyAsync(id, updatedBy);
            return await ProxyResponse(response, $"DeleteCurrency({id})");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while deleting currency {Id} via BFF", id);
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
