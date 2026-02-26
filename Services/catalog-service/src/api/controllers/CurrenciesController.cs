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
public class CurrenciesController : ControllerBase
{
    private readonly IMessageBus _mediator;

    public CurrenciesController(IMessageBus mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves all currencies.
    /// </summary>
    /// <param name="includeInactive">[OPTIONAL] If true, includes inactive currencies (default: false).</param>
    /// <returns>A list of currencies.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CurrencyDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.InvokeAsync<IEnumerable<Domain.Aggregates.Currency>>(new GetAllCurrenciesQuery(includeInactive));
        var dtos = result.Select(c => new CurrencyDto(
            c.Id, c.CurrencyCode, c.CurrencyName, c.CurrencyValue, c.DecimalUnit,
            c.IsActive, c.IsDelete, c.CreatedAt, c.CreatedBy, c.UpdatedBy, c.UpdatedAt
        ));
        return Ok(dtos);
    }

    /// <summary>
    /// Gets a currency by its unique identifier.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique ID of the currency.</param>
    /// <returns>The currency details if found.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CurrencyDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.InvokeAsync<Domain.Aggregates.Currency?>(new GetCurrencyByIdQuery(id));
        if (result == null) return NotFound(CurrencyErrors.NotFound);

        var dto = new CurrencyDto(
            result.Id, result.CurrencyCode, result.CurrencyName, result.CurrencyValue, result.DecimalUnit,
            result.IsActive, result.IsDelete, result.CreatedAt, result.CreatedBy, result.UpdatedBy, result.UpdatedAt
        );
        return Ok(dto);
    }

    /// <summary>
    /// Gets a currency by its ISO code.
    /// </summary>
    /// <param name="code">[REQUIRED] The 3-character ISO currency code.</param>
    /// <returns>The currency details if found.</returns>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(CurrencyDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByCode(string code)
    {
        var result = await _mediator.InvokeAsync<Domain.Aggregates.Currency?>(new GetCurrencyByCodeQuery(code));
        if (result == null) return NotFound(CurrencyErrors.NotFound);

        var dto = new CurrencyDto(
            result.Id, result.CurrencyCode, result.CurrencyName, result.CurrencyValue, result.DecimalUnit,
            result.IsActive, result.IsDelete, result.CreatedAt, result.CreatedBy, result.UpdatedBy, result.UpdatedAt
        );
        return Ok(dto);
    }

    /// <summary>
    /// Creates a new currency.
    /// </summary>
    /// <param name="request">[REQUIRED] The creation request details.</param>
    /// <returns>The ID of the newly created currency.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateCurrencyRequest request)
    {
        var command = new CreateCurrencyCommand(
            request.CurrencyCode, request.CurrencyName, request.CurrencyValue, request.DecimalUnit, request.CreatedBy
        );
        var result = await _mediator.InvokeAsync<Result<Guid>>(command);

        if (result.IsFailure) return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// Updates an existing currency.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique ID of the currency to update.</param>
    /// <param name="request">[REQUIRED] The update request details.</param>
    /// <returns>True if successful.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCurrencyRequest request)
    {
        var command = new UpdateCurrencyCommand(
            id, request.CurrencyName, request.CurrencyValue, request.DecimalUnit, request.IsActive, request.UpdatedBy
        );
        var result = await _mediator.InvokeAsync<Result<bool>>(command);

        if (result.IsFailure)
        {
            if (result.Error == CurrencyErrors.NotFound) return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes a currency (soft delete).
    /// </summary>
    /// <param name="id">[REQUIRED] The unique ID of the currency to delete.</param>
    /// <param name="updatedBy">[OPTIONAL] User identifier for the update record.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] string? updatedBy)
    {
        var result = await _mediator.InvokeAsync<Result<bool>>(new DeleteCurrencyCommand(id, updatedBy));

        if (result.IsFailure)
        {
            if (result.Error == CurrencyErrors.NotFound) return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return NoContent();
    }
}
