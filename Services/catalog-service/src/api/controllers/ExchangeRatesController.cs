using CatalogService.Application.Commands;
using CatalogService.Application.Dtos;
using CatalogService.Application.Queries;
using CatalogService.Application.Errors;
using GssCommon.Common;
using Wolverine;
using Microsoft.AspNetCore.Mvc;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class ExchangeRatesController : ControllerBase
{
    private readonly IMessageBus _mediator;

    public ExchangeRatesController(IMessageBus mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves all exchange rates.
    /// </summary>
    /// <param name="includeInactive">[OPTIONAL] If true, includes inactive rates (default: false).</param>
    /// <returns>A list of exchange rates.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<ExchangeRateDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.InvokeAsync<IEnumerable<Domain.Aggregates.ExchangeRate>>(new GetAllExchangeRatesQuery(includeInactive));
        var dtos = result.Select(e => new ExchangeRateDto(
            e.Id, e.BaseCurrency, e.QuoteCurrency, e.Rate, e.ValidFrom, e.ValidEnd,
            e.IsActive, e.IsDelete, e.CreatedAt, e.CreatedBy, e.UpdatedBy, e.UpdatedAt
        ));
        return Ok(dtos);
    }

    /// <summary>
    /// Gets an exchange rate by its unique identifier.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique ID of the exchange rate.</param>
    /// <returns>The exchange rate details if found.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ExchangeRateDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.InvokeAsync<Domain.Aggregates.ExchangeRate?>(new GetExchangeRateByIdQuery(id));
        if (result == null) return NotFound(ExchangeRateErrors.NotFound);

        var dto = new ExchangeRateDto(
            result.Id, result.BaseCurrency, result.QuoteCurrency, result.Rate, result.ValidFrom, result.ValidEnd,
            result.IsActive, result.IsDelete, result.CreatedAt, result.CreatedBy, result.UpdatedBy, result.UpdatedAt
        );
        return Ok(dto);
    }

    /// <summary>
    /// Gets the latest exchange rate for a currency pair.
    /// </summary>
    /// <param name="baseCurrency">[REQUIRED] The 3-character base currency code.</param>
    /// <param name="quoteCurrency">[REQUIRED] The 3-character target currency code.</param>
    /// <returns>The latest active exchange rate if found.</returns>
    [HttpGet("latest")]
    [ProducesResponseType(typeof(ExchangeRateDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetLatest([FromQuery] string baseCurrency, [FromQuery] string quoteCurrency)
    {
        var result = await _mediator.InvokeAsync<Domain.Aggregates.ExchangeRate?>(new GetLatestExchangeRateQuery(baseCurrency, quoteCurrency));
        if (result == null) return NotFound(ExchangeRateErrors.NotFound);

        var dto = new ExchangeRateDto(
            result.Id, result.BaseCurrency, result.QuoteCurrency, result.Rate, result.ValidFrom, result.ValidEnd,
            result.IsActive, result.IsDelete, result.CreatedAt, result.CreatedBy, result.UpdatedBy, result.UpdatedAt
        );
        return Ok(dto);
    }

    /// <summary>
    /// Creates a new exchange rate.
    /// </summary>
    /// <remarks>
    /// Validates that there are no overlapping exchange rates for the same currency pair and validity period.
    /// </remarks>
    /// <param name="request">[REQUIRED] The creation request details.</param>
    /// <returns>The ID of the newly created exchange rate.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateExchangeRateRequest request)
    {
        var command = new CreateExchangeRateCommand(
            request.BaseCurrency, request.QuoteCurrency, request.Rate, request.ValidFrom, request.ValidEnd, request.CreatedBy
        );
        var result = await _mediator.InvokeAsync<Result<Guid>>(command);

        if (result.IsFailure) return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// Updates an existing exchange rate.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique ID of the exchange rate to update.</param>
    /// <param name="request">[REQUIRED] The update request details.</param>
    /// <returns>True if successful.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateExchangeRateRequest request)
    {
        var command = new UpdateExchangeRateCommand(
            id, request.Rate, request.ValidFrom, request.ValidEnd, request.IsActive, request.UpdatedBy
        );
        var result = await _mediator.InvokeAsync<Result<bool>>(command);

        if (result.IsFailure)
        {
            if (result.Error == ExchangeRateErrors.NotFound) return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes an exchange rate (soft delete).
    /// </summary>
    /// <param name="id">[REQUIRED] The unique ID of the exchange rate to delete.</param>
    /// <param name="updatedBy">[OPTIONAL] User identifier for the update record.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] string? updatedBy)
    {
        var result = await _mediator.InvokeAsync<Result<bool>>(new DeleteExchangeRateCommand(id, updatedBy));

        if (result.IsFailure)
        {
            if (result.Error == ExchangeRateErrors.NotFound) return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return NoContent();
    }
}
