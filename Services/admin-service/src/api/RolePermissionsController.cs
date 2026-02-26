using System.Net.Mime;
using AdminService.Application.Commands;
using AdminService.Application.Queries;
using AdminService.Domain.Aggregates;
using AdminService.Api.Dto;
using Microsoft.AspNetCore.Mvc;
using Wolverine;

namespace AdminService.Api;

[ApiController]
[Route("api/v1/role-permissions")]
[Produces(MediaTypeNames.Application.Json)]
public sealed class RolePermissionsController : ControllerBase
{
    private readonly IMessageBus _bus;
    public RolePermissionsController(IMessageBus bus) => _bus = bus;

    /// <summary>Assign a permission to a role.</summary>
    [HttpPost]
    [Consumes(MediaTypeNames.Application.Json)]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    public async Task<IActionResult> Assign([FromBody] AssignPermissionToRoleCommand cmd, CancellationToken ct)
    {
        var id = await _bus.InvokeAsync<Guid>(cmd, ct);
        return CreatedAtAction(nameof(GetPermissionsByRole), new { roleId = cmd.RoleId }, id);
    }

    /// <summary>Remove a permission from a role.</summary>
    [HttpDelete]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Remove([FromQuery] Guid roleId, [FromQuery] Guid permissionId, CancellationToken ct)
    {
        var ok = await _bus.InvokeAsync<bool>(new RemovePermissionFromRoleCommand(roleId, permissionId), ct);
        return ok ? NoContent() : NotFound();
    }

    /// <summary>List permissions assigned to a role.</summary>
    [HttpGet("roles/{roleId:guid}/permissions")]
    [ProducesResponseType(typeof(IEnumerable<PermissionResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPermissionsByRole([FromRoute] Guid roleId, CancellationToken ct)
    {
        var list = await _bus.InvokeAsync<IEnumerable<Permission>>(new GetPermissionsByRoleIdQuery(roleId), ct);
        
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

    /// <summary>List roles that have a specific permission.</summary>
    [HttpGet("permissions/{permissionId:guid}/roles")]
    [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetRolesByPermission([FromRoute] Guid permissionId, CancellationToken ct)
    {
        var list = await _bus.InvokeAsync<IEnumerable<Role>>(new GetRolesByPermissionIdQuery(permissionId), ct);
        
        var response = list.Select(r => new RoleResponse(
            r.Id,
            r.RoleName,
            r.Description,
            r.IsActive,
            r.IsDeleted,
            r.CreatedBy,
            r.CreatedAt,
            r.ModifiedBy,
            r.ModifiedAt
        )).ToList();

        return Ok(response);
    }
}
