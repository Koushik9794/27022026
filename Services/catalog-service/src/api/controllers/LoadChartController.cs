using CatalogService.application.commands.loadchart;
using CatalogService.application.dtos;
using CatalogService.Application.Commands;
using CatalogService.Application.Dtos;
using CatalogService.Application.Queries;
using GssCommon.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace CatalogService.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class LoadChartController : ControllerBase
{
    private readonly IMessageBus _mediator;

    public LoadChartController(IMessageBus mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Retrieves all load charts based on optional filtering criteria.
    /// </summary>
    /// <param name="productGroupId">[OPTIONAL] Filter by Product Group ID.</param>
    /// <param name="chartType">[OPTIONAL] Filter by Chart Type.</param>
    /// <param name="componentCode">[OPTIONAL] Filter by Component Code (Component Name).</param>
    /// <param name="includeDeleted">[OPTIONAL] Whether to include deleted records (default: false).</param>
    /// <returns>A list of load charts.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<LoadChartDto>), 200)]
    public async Task<IActionResult> GetAll(
        [FromQuery] Guid? productGroupId,
        [FromQuery] string? chartType,
        [FromQuery] string? componentCode,
        [FromQuery] Guid? componentTypeId,
        [FromQuery] bool includeDeleted = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 50)
    {
        var query = new GetAllLoadChartsQuery(productGroupId, chartType, componentCode, componentTypeId, includeDeleted, page, pageSize);
        var result = await _mediator.InvokeAsync<Result<IEnumerable<LoadChartDto>>>(query);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Retrieves all load charts for a specific chart type without pagination.
    /// </summary>
    /// <param name="chartType">[REQUIRED] The chart type to filter by.</param>
    /// <returns>A full list of load charts for the specified type.</returns>
    [HttpGet("all/{chartType}")]
    [ProducesResponseType(typeof(IEnumerable<LoadChartDto>), 200)]
    public async Task<IActionResult> GetByType(string chartType)
    {
        var result = await _mediator.InvokeAsync<Result<IEnumerable<LoadChartDto>>>(new GetLoadChartsByTypeQuery(chartType));
        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Gets a specific load chart by its unique identifier.
    /// </summary>
    /// <param name="id">[REQUIRED] The unique ID of the load chart.</param>
    /// <returns>Load chart details.</returns>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(LoadChartDto), 200)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> GetById(Guid id)
    {
        var result = await _mediator.InvokeAsync<Result<LoadChartDto>>(new GetLoadChartByIdQuery(id));
        return result.IsSuccess ? Ok(result.Value) : NotFound(result.Error);
    }

    /// <summary>
    /// Creates a new load chart entry.
    /// </summary>
    /// <param name="request">[REQUIRED] Load chart creation details.</param>
    /// <returns>The ID of the created load chart.</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), 201)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Create([FromBody] CreateLoadChartRequest request)
    {
        var command = new CreateLoadChartCommand(
            request.ProductGroupId,
            request.ChartType,
            request.ComponentCode,
            request.ComponentTypeId,
            request.Attributes,
            request.CreatedBy
        );

        var result = await _mediator.InvokeAsync<Result<Guid>>(command);
        if (result.IsFailure) return BadRequest(result.Error);

        return CreatedAtAction(nameof(GetById), new { id = result.Value }, result.Value);
    }

    /// <summary>
    /// Updates an existing load chart entry.
    /// </summary>
    /// <param name="id">[REQUIRED] The ID of the load chart to update.</param>
    /// <param name="request">[REQUIRED] Updated load chart details.</param>
    /// <returns>The updated load chart status.</returns>
    [HttpPut("{id:guid}")]
    [ProducesResponseType(typeof(bool), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateLoadChartRequest request)
    {
        var command = new UpdateLoadChartCommand(
            id,
            request.ProductGroupId,
            request.ChartType,
            request.ComponentCode,
            request.ComponentTypeId,
            request.Attributes,
            request.IsActive,
            request.UpdatedBy
        );

        var result = await _mediator.InvokeAsync<Result<bool>>(command);
        if (result.IsFailure)
        {
            if (result.Error.Code.Contains("NotFound")) return NotFound(result.Error);
            return BadRequest(result.Error);
        }

        return Ok(result.Value);
    }

    /// <summary>
    /// Deletes a specific load chart entry (soft delete).
    /// </summary>
    /// <param name="id">[REQUIRED] The ID of the load chart to delete.</param>
    /// <param name="deletedBy">[OPTIONAL] The ID of the user performing the deletion.</param>
    /// <returns>No content on success.</returns>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(204)]
    [ProducesResponseType(404)]
    public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid? deletedBy)
    {
        var result = await _mediator.InvokeAsync<Result<bool>>(new DeleteLoadChartCommand(id, deletedBy));
        if (result.IsFailure)
        {
            if (result.Error.Code.Contains("NotFound")) return NotFound(result.Error);
            return BadRequest(result.Error);
        }
        return NoContent();
    }

    /// <summary>
    /// Imports load charts from an Excel file.
    /// </summary>
    /// <param name="request">[REQUIRED] The Excel file and metadata.</param>
    /// <returns>The number of imported records.</returns>
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    [ProducesResponseType(typeof(int), 200)]
    [ProducesResponseType(400)]
    public async Task<IActionResult> Import([FromForm] ImportLoadChartExcelRequest request)
    {
        var command = new ImportLoadChartExcelCommand(request.File, request.ProductGroupId, request.ChartType, request.CreatedBy);
        var result = await _mediator.InvokeAsync<Result<int>>(command);

        return result.IsSuccess ? Ok(result.Value) : BadRequest(result.Error);
    }

    /// <summary>
    /// Load chart search based on attributes
    /// </summary>
    /// <param name="chart_type"></param>
    /// <param name="levels"></param>
    /// <param name="beamSpan"></param>
    ///  <param name="IsStiffenerenable"></param>
    ///   <param name="levelConfigs"></param>
    /// <returns></returns>

    [HttpPost("candidates")]
    [ProducesResponseType(typeof(IReadOnlyList<LoadChartCandidateDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Candidates([FromBody] LoadChartSearchCommand command, CancellationToken ct)
    {
        // If [ApiController] is enabled, IValidatableObject will be evaluated
        // and invalid ModelState will automatically return 400

        var result = await _mediator.InvokeAsync<IReadOnlyList<LoadChartCandidateDto>>(command, ct);
        return Ok(result);
    }

}
