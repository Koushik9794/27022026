// AdminService.Api/PermissionsController.cs
using System.Net.Mime;
using AdminService.Application.Commands;
using AdminService.Application.Queries;
using AdminService.Domain.Aggregates;
using AdminService.Infrastructure.Persistence;
using AdminService.Api.Dto;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace AdminService.Api;

[ApiController]
[Route("api/v1/permissions")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class PermissionsController : ControllerBase
{
    private readonly IMessageBus _bus;
    public PermissionsController(IMessageBus bus) => _bus = bus;

    /// <summary>Create a new permission.</summary>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Create([FromBody] CreatePermissionCommand cmd, CancellationToken ct)
    {
        if (cmd is null) return BadRequest("Invalid payload.");

        try
        {
            var id = await _bus.InvokeAsync<Guid>(cmd, ct);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (InvalidOperationException ex)
        {
            // thrown by handler when duplicate exists
            return Conflict(ex.Message);
        }
    }

    /// <summary>Get permission by Id.</summary>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(PermissionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById([FromRoute] Guid id, CancellationToken ct)
    {
        var permission = await _bus.InvokeAsync<Permission?>(new GetPermissionByIdQuery(id), ct);
        if (permission is null) return NotFound();

        var response = new PermissionResponse(
            permission.Id,
            permission.PermissionName.Value,
            permission.Description,
            permission.ModuleName.Value,
            permission.EntityName?.Value,
            permission.IsActive,
            permission.IsDeleted,
            permission.CreatedBy,
            permission.CreatedAt,
            permission.ModifiedBy,
            permission.ModifiedAt);

        return Ok(response);
    }

    /// <summary>List permissions (optionally filter by module/entity).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<PermissionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll([FromQuery] string? module, [FromQuery] string? entityName, CancellationToken ct)
    {
        var list = await _bus.InvokeAsync<IEnumerable<Permission>>(new GetAllPermissionsQuery(module, entityName), ct);
        
        var response = list.Select(p => new PermissionResponse(
            p.Id,
            p.PermissionName.Value,
            p.Description,
            p.ModuleName.Value,
            p.EntityName?.Value,
            p.IsActive,
            p.IsDeleted,
            p.CreatedBy,
            p.CreatedAt,
            p.ModifiedBy,
            p.ModifiedAt));

        return Ok(response);
    }

    /// <summary>Update a permission.</summary>
    [HttpPut("{id:guid}")]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Update([FromRoute] Guid id, [FromBody] UpdatePermissionRequest request, CancellationToken ct)
    {
        if (request is null) return BadRequest("Invalid payload.");

        var cmd = new UpdatePermissionCommand(
            id,
            request.Description,
            request.ModuleName,
            request.EntityName,
            request.ModifiedBy ?? "System");

        var ok = await _bus.InvokeAsync<bool>(cmd, ct);
        return ok ? NoContent() : NotFound();
    }

    public record UpdatePermissionRequest(
        string? Description,
        string ModuleName,
        string? EntityName,
        string? ModifiedBy);

    /// <summary>Soft delete a permission.</summary>
    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Delete([FromRoute] Guid id, [FromQuery] string? modifiedBy, CancellationToken ct)
    {
        var user = string.IsNullOrWhiteSpace(modifiedBy) ? "System" : modifiedBy;
        var ok = await _bus.InvokeAsync<bool>(new DeletePermissionCommand(id, user), ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>Activate/Deactivate a permission.</summary>
    [HttpPost("{id:guid}/activate")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Activate([FromRoute] Guid id, [FromQuery] bool activate, [FromQuery] string? modifiedBy, CancellationToken ct)
    {
        var user = string.IsNullOrWhiteSpace(modifiedBy) ? "System" : modifiedBy;
        var ok = await _bus.InvokeAsync<bool>(new ActivatePermissionCommand(id, activate, user), ct);
        return ok ? NoContent() : NotFound();
    }
}
