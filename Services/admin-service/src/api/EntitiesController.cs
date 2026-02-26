using System.Net.Mime;
using AdminService.Application.Commands;
using AdminService.Application.Queries;
using AdminService.Domain.Entities;
using AdminService.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace AdminService.Api;

[ApiController]
[Route("api/v1/entities")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class EntitiesController : ControllerBase
{
    private readonly IMessageBus _bus;
    public EntitiesController(IMessageBus bus) => _bus = bus;

    /// <summary>List all active entities (from app_entities).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<AppEntity>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var list = await _bus.InvokeAsync<IEnumerable<AppEntity>>(new GetAllEntitiesQuery(), ct);
        return Ok(list);
    }

    /// <summary>Create a new entity (idempotent by name).</summary>
    /// <remarks>
    /// The body must include: entityName, createdBy, sourceTable, pkColumn, labelColumn.
    /// Returns 201 if created, 409 if it already exists, 400 for validation errors.
    /// </remarks>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreateEntityCommand cmd, CancellationToken ct)
    {
        if (cmd is null) return BadRequest("Invalid payload.");

        try
        {
            // Returns the created entity name
            var name = await _bus.InvokeAsync<string>(cmd, ct);

            // If you add a GetByName endpoint later, switch CreatedAtAction to that route.
            return CreatedAtAction(nameof(GetAll), new { name }, name);
        }
        catch (ArgumentException ex)
        {
            // From handler guard clauses (missing/blank fields)
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            // From handler when entity already exists
            return Conflict(ex.Message);
        }
    }
}
