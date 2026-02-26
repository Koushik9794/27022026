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
public class CountriesController : ControllerBase
{
    private readonly IMessageBus _mediator;

    public CountriesController(IMessageBus mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves all countries.
    /// </summary>
    /// <param name="includeInactive">[OPTIONAL] If true, includes inactive countries (default: false).</param>
    /// <returns>A list of countries.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<CountryDto>), 200)]
    public async Task<IActionResult> GetAll([FromQuery] bool includeInactive = false)
    {
        var result = await _mediator.InvokeAsync<IEnumerable<Domain.Aggregates.Country>>(new GetAllCountriesQuery(includeInactive));
        var dtos = result.Select(c => new CountryDto(
            c.Id, c.CountryCode, c.CountryName, c.CurrencyCode,
            c.IsActive, c.IsDelete, c.CreatedAt, c.CreatedBy, c.UpdatedBy, c.UpdatedAt
        ));
        return Ok(dtos);
    }

    /// <summary>
    /// Gets a country by its unique identifier.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique ID of the country.</param>
    /// <returns>The country details if found.</returns>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(CountryDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.InvokeAsync<Domain.Aggregates.Country?>(new GetCountryByIdQuery(id));
        if (result == null) return NotFound(CountryErrors.NotFound);

        var dto = new CountryDto(
            result.Id, result.CountryCode, result.CountryName, result.CurrencyCode,
            result.IsActive, result.IsDelete, result.CreatedAt, result.CreatedBy, result.UpdatedBy, result.UpdatedAt
        );
        return Ok(dto);
    }

    /// <summary>
    /// Gets a country by its 2-character ISO code.
    /// </summary>
    /// <param name="code">[REQUIRED] The 2-character ISO country code (e.g., 'IN', 'US').</param>
    /// <returns>The country details if found.</returns>
    [HttpGet("code/{code}")]
    [ProducesResponseType(typeof(CountryDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetByCode(string code)
    {
        var result = await _mediator.InvokeAsync<Domain.Aggregates.Country?>(new GetCountryByCodeQuery(code));
        if (result == null) return NotFound(CountryErrors.NotFound);

        var dto = new CountryDto(
            result.Id, result.CountryCode, result.CountryName, result.CurrencyCode,
            result.IsActive, result.IsDelete, result.CreatedAt, result.CreatedBy, result.UpdatedBy, result.UpdatedAt
        );
        return Ok(dto);
    }

    /// <summary>
    /// Creates a new country.
    /// </summary>
    /// <param name="request">[REQUIRED] The creation request details.</param>
    /// <returns>The ID of the newly created country.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateCountryRequest request)
    {
        var command = new CreateCountryCommand(
            request.CountryCode, request.CountryName, request.CurrencyCode, request.CreatedBy
        );
        var result = await _mediator.InvokeAsync<Result<Guid>>(command);

        if (result.IsFailure) return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// Updates an existing country.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique ID of the country to update.</param>
    /// <param name="request">[REQUIRED] The update request details.</param>
    /// <returns>True if successful.</returns>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCountryRequest request)
    {
        var command = new UpdateCountryCommand(
            id, request.CountryName, request.CurrencyCode, request.IsActive, request.UpdatedBy
        );
        var result = await _mediator.InvokeAsync<Result<bool>>(command);

        if (result.IsFailure)
        {
            if (result.Error == CountryErrors.NotFound) return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes a country (soft delete).
    /// </summary>
    /// <param name="id">[REQUIRED] The unique ID of the country to delete.</param>
    /// <param name="updatedBy">[OPTIONAL] User identifier for the update record.</param>
    [HttpDelete("{id}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] string? updatedBy)
    {
        var result = await _mediator.InvokeAsync<Result<bool>>(new DeleteCountryCommand(id, updatedBy));

        if (result.IsFailure)
        {
            if (result.Error == CountryErrors.NotFound) return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return NoContent();
    }
}
