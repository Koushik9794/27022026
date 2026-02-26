using GssWebApi.Dto;
using GssWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GssWebApi.Api.Controllers
{
    [ApiController]
    [Route("api/v1/roles")]
    [AllowAnonymous]
    [Produces("application/json")]
    public class RolesController : ControllerBase
    {
        private readonly IAdminServiceClient _adminService;
        private readonly ILogger<RolesController> _logger;

        public RolesController(IAdminServiceClient adminService, ILogger<RolesController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        /// <summary>
        /// Get all roles.
        /// </summary>
        /// <param name="ct">[OPTIONAL] Cancellation token.</param>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<RoleResponse>>> GetAll(CancellationToken ct)
        {
            var roles = await _adminService.GetRolesAsync(ct);
            return Ok(roles);
        }

        /// <summary>
        /// Get role by ID.
        /// </summary>
        /// <param name="id">[REQUIRED] Role unique identifier.</param>
        /// <param name="ct">[OPTIONAL] Cancellation token.</param>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(RoleResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<RoleResponse>> GetById(Guid id, CancellationToken ct)
        {
            var role = await _adminService.GetRoleByIdAsync(id, ct);
            return role == null ? NotFound() : Ok(role);
        }

        /// <summary>
        /// Create a new role.
        /// </summary>
        /// <param name="request">[REQUIRED] Role details.</param>
        /// <param name="ct">[OPTIONAL] Cancellation token.</param>
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreateRoleRequest request, CancellationToken ct)
        {
            var id = await _adminService.CreateRoleAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        /// <summary>
        /// Update an existing role.
        /// </summary>
        /// <param name="id">[REQUIRED] Role ID to update.</param>
        /// <param name="request">[REQUIRED] Updated role details.</param>
        /// <param name="ct">[OPTIONAL] Cancellation token.</param>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateRoleRequest request, CancellationToken ct)
        {
            var result = await _adminService.UpdateRoleAsync(id, request, ct);
            return result ? NoContent() : NotFound();
        }

        /// <summary>
        /// Delete a role.
        /// </summary>
        /// <param name="id">[REQUIRED] Role ID to delete.</param>
        /// <param name="modifiedBy">[OPTIONAL] User performing the deletion.</param>
        /// <param name="ct">[OPTIONAL] Cancellation token.</param>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] string? modifiedBy, CancellationToken ct)
        {
            var result = await _adminService.DeleteRoleAsync(id, modifiedBy, ct);
            return result ? NoContent() : NotFound();
        }

        /// <summary>
        /// Activate or deactivate a role.
        /// </summary>
        /// <param name="id">[REQUIRED] Role ID.</param>
        /// <param name="activate">[OPTIONAL] Whether to activate (default: true).</param>
        /// <param name="modifiedBy">[OPTIONAL] User performing the action.</param>
        /// <param name="ct">[OPTIONAL] Cancellation token.</param>
        [HttpPost("{id:guid}/activate")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Activate(Guid id, [FromQuery] bool activate = true, [FromQuery] string? modifiedBy = null, CancellationToken ct = default)
        {
            var result = await _adminService.ActivateRoleAsync(id, activate, modifiedBy, ct);
            return result ? NoContent() : NotFound();
        }
    }
}
