using Microsoft.AspNetCore.Mvc;
using Wolverine;
using AdminService.Application.Commands;
using AdminService.Application.Queries;
using AdminService.Application.Dtos;
using AdminService.Api.Dto;
using AdminService.Domain.Aggregates;

namespace AdminService.Api
{
    /// <summary>
    /// Role management API endpoints
    /// </summary>
    [ApiController]
    [Route("api/v1/roles")]
    [Produces("application/json")]
    public sealed class RolesController : ControllerBase
    {
        private readonly IMessageBus _messageBus;

        public RolesController(IMessageBus messageBus)
        {
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        /// <summary>
        /// Create a new role
        /// </summary>
        /// <param name="command">Role creation details</param>
        /// <returns>Created role ID</returns>
        [HttpPost]
        [ProducesResponseType(typeof(CreateRoleResult), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleCommand command)
        {
            var result = await _messageBus.InvokeAsync<CreateRoleResult>(command);
            return CreatedAtAction(nameof(GetRoleById), new { id = result.RoleId }, result);
        }

        /// <summary>
        /// Get role by ID
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <returns>Role details</returns>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetRoleById(Guid id)
        {
            var role = await _messageBus.InvokeAsync<Role?>(new GetRoleByIdQuery(id));
            if (role is null) return NotFound();

            var response = new RoleResponse(
                role.Id,
                role.RoleName,
                role.Description,
                role.IsActive,
                role.IsDeleted,
                role.CreatedBy,
                role.CreatedAt,
                role.ModifiedBy,
                role.ModifiedAt,
                role.PermissionCount
            );
            return Ok(response);
        }

        /// <summary>
        /// Get all roles
        /// </summary>
        /// <returns>List of roles</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<RoleResponse>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _messageBus.InvokeAsync<IEnumerable<Role>>(new GetAllRolesQuery());
            
            var response = roles.Select(r => new RoleResponse(
                r.Id,
                r.RoleName,
                r.Description,
                r.IsActive,
                r.IsDeleted,
                r.CreatedBy,
                r.CreatedAt,
                r.ModifiedBy,
                r.ModifiedAt,
                r.PermissionCount
            )).ToList();

            return Ok(response);
        }

        /// <summary>
        /// Update a role (name/description)
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <param name="request">Update details</param>
        /// <returns>No content</returns>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<IActionResult> UpdateRole(Guid id, [FromBody] UpdateRoleRequest request)
        {
            await _messageBus.InvokeAsync(new UpdateRoleCommand(
                id,
                request.RoleName,
                request.Description,
                request.ModifiedBy
            ));
            return NoContent();
        }

        public record UpdateRoleRequest(string RoleName, string? Description, string? ModifiedBy);

        /// <summary>
        /// Soft delete a role (marks is_deleted = true)
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <param name="modifiedBy">Optional user performing the deletion (audit)</param>
        /// <returns>No content</returns>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteRole(Guid id, [FromQuery] string? modifiedBy)
        {
            await _messageBus.InvokeAsync(new DeleteRoleCommand(id, modifiedBy));
            return NoContent();
        }

        /// <summary>
        /// Activate a role
        /// </summary>
        /// <param name="id">Role ID</param>
        /// <returns>No content</returns>
        [HttpPost("{id:guid}/activate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivateRole(Guid id, [FromQuery] bool activate = true, [FromQuery] string? modifiedBy = null)
        {
            await _messageBus.InvokeAsync(new ActivateRoleCommand(id, activate, modifiedBy));
            return NoContent();
        }
    }
}
