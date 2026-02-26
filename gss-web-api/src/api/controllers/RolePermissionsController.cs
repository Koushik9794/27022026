using GssWebApi.Dto;
using GssWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GssWebApi.Api.Controllers
{
    [ApiController]
    [Route("api/v1/role-permissions")]
    [AllowAnonymous]
    [Produces("application/json")]
    public class RolePermissionsController : ControllerBase
    {
        private readonly IAdminServiceClient _adminService;
        private readonly ILogger<RolePermissionsController> _logger;

        public RolePermissionsController(IAdminServiceClient adminService, ILogger<RolePermissionsController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        /// <summary>
        /// Assign a permission to a role.
        /// </summary>
        /// <param name="request">[REQUIRED] Assignment details.</param>
        /// <param name="ct">[OPTIONAL] Cancellation token.</param>
        [HttpPost]
        [AllowAnonymous]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<Guid>> Assign([FromBody] AssignPermissionRequest request, CancellationToken ct)
        {
            var id = await _adminService.AssignPermissionToRoleAsync(request, ct);
            return Ok(id);
        }

        /// <summary>
        /// Remove a permission from a role.
        /// </summary>
        /// <param name="roleId">[REQUIRED] Role ID.</param>
        /// <param name="permissionId">[REQUIRED] Permission ID.</param>
        /// <param name="ct">[OPTIONAL] Cancellation token.</param>
        [HttpDelete]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Remove([FromQuery] Guid roleId, [FromQuery] Guid permissionId, CancellationToken ct)
        {
            var result = await _adminService.RemovePermissionFromRoleAsync(roleId, permissionId, ct);
            return result ? NoContent() : NotFound();
        }

        /// <summary>
        /// Get all permissions assigned to a role.
        /// </summary>
        /// <param name="roleId">[REQUIRED] Role ID.</param>
        /// <param name="ct">[OPTIONAL] Cancellation token.</param>
        [HttpGet("roles/{roleId:guid}/permissions")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<PermissionResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<PermissionResponse>>> GetPermissionsByRole(Guid roleId, CancellationToken ct)
        {
            var permissions = await _adminService.GetPermissionsByRoleAsync(roleId, ct);
            return Ok(permissions);
        }

        /// <summary>
        /// Get all roles assigned to a permission.
        /// </summary>
        /// <param name="permissionId">[REQUIRED] Permission ID.</param>
        /// <param name="ct">[OPTIONAL] Cancellation token.</param>
        [HttpGet("permissions/{permissionId:guid}/roles")]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<RoleResponse>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<IEnumerable<RoleResponse>>> GetRolesByPermission(Guid permissionId, CancellationToken ct)
        {
            var roles = await _adminService.GetRolesByPermissionAsync(permissionId, ct);
            return Ok(roles);
        }
    }
}
