using GssWebApi.Dto;
using GssWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GssWebApi.Api.Controllers
{
    [ApiController]
    [Route("api/v1/permissions")]
    [AllowAnonymous]
    [Produces("application/json")]
    public class PermissionsController : ControllerBase
    {
        private readonly IAdminServiceClient _adminService;
        private readonly ILogger<PermissionsController> _logger;

        public PermissionsController(IAdminServiceClient adminService, ILogger<PermissionsController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        /// <summary>
        /// Get all permissions.
        /// </summary>
        /// <param name="module">[OPTIONAL] Filter by module name.</param>
        /// <param name="entityName">[OPTIONAL] Filter by entity name.</param>
        /// <param name="ct">[OPTIONAL] Cancellation token.</param>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<PermissionResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<PermissionResponse>>> GetAll([FromQuery] string? module, [FromQuery] string? entityName, CancellationToken ct)
        {
            var permissions = await _adminService.GetPermissionsAsync(module, entityName, ct);
            return Ok(permissions);
        }

        /// <summary>
        /// Get permission by ID.
        /// </summary>
        /// <param name="id">[REQUIRED] Permission unique identifier.</param>
        /// <param name="ct">[OPTIONAL] Cancellation token.</param>
        [HttpGet("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(PermissionResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PermissionResponse>> GetById(Guid id, CancellationToken ct)
        {
            var permission = await _adminService.GetPermissionByIdAsync(id, ct);
            return permission == null ? NotFound() : Ok(permission);
        }

        /// <summary>
        /// Create a new permission.
        /// </summary>
        /// <param name="request">[REQUIRED] Permission details.</param>
        /// <param name="ct">[OPTIONAL] Cancellation token.</param>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> Create([FromBody] CreatePermissionRequest request, CancellationToken ct)
        {
            var id = await _adminService.CreatePermissionAsync(request, ct);
            return CreatedAtAction(nameof(GetById), new { id }, id);
        }

        /// <summary>
        /// Update an existing permission.
        /// </summary>
        /// <param name="id">[REQUIRED] Permission ID to update.</param>
        /// <param name="request">[REQUIRED] Updated permission details.</param>
        /// <param name="ct">[OPTIONAL] Cancellation token.</param>
        [HttpPut("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePermissionRequest request, CancellationToken ct)
        {
            var result = await _adminService.UpdatePermissionAsync(id, request, ct);
            return result ? NoContent() : NotFound();
        }

        /// <summary>
        /// Delete a permission.
        /// </summary>
        /// <param name="id">[REQUIRED] Permission ID to delete.</param>
        /// <param name="modifiedBy">[OPTIONAL] User performing the deletion.</param>
        /// <param name="ct">[OPTIONAL] Cancellation token.</param>
        [HttpDelete("{id:guid}")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] string? modifiedBy = null, CancellationToken ct = default)
        {
            var result = await _adminService.DeletePermissionAsync(id, modifiedBy, ct);
            return result ? NoContent() : NotFound();
        }

        /// <summary>
        /// Activate or deactivate a permission.
        /// </summary>
        /// <param name="id">[REQUIRED] Permission ID.</param>
        /// <param name="activate">[REQUIRED] Whether to activate (true) or deactivate (false).</param>
        /// <param name="modifiedBy">[OPTIONAL] User performming the action.</param>
        /// <param name="ct">[OPTIONAL] Cancellation token.</param>
        [HttpPost("{id:guid}/activate")]
        [AllowAnonymous]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Activate(Guid id, [FromQuery] bool activate, [FromQuery] string? modifiedBy = null, CancellationToken ct = default)
        {
            var result = await _adminService.ActivatePermissionAsync(id, activate, modifiedBy, ct);
            return result ? NoContent() : NotFound();
        }
    }
}
