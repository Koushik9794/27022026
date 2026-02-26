
using GssWebApi.Dto;
using GssWebApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Net.Mime; // <-- if you prefer MediaTypeNames; otherwise remove and keep string literal

namespace GssWebApi.Api.Controllers
{
    [ApiController]
    [Route("api/v1/entities")]
    [Authorize]
    [Produces("application/json")] // or [Produces(MediaTypeNames.Application.Json)]
    public class EntitiesController : ControllerBase
    {
        private readonly IAdminServiceClient _adminService;
        private readonly ILogger<EntitiesController> _logger;

        public EntitiesController(IAdminServiceClient adminService, ILogger<EntitiesController> logger)
        {
            _adminService = adminService;
            _logger = logger;
        }

        /// <summary>
        /// Get all entities.
        /// </summary>
        /// <param name="ct">[OPTIONAL] Cancellation token.</param>
        [HttpGet]
        [AllowAnonymous]
        [ProducesResponseType(typeof(IEnumerable<EntityResponse>), StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<EntityResponse>>> GetAll([FromQuery] CancellationToken ct)
        {
            var entities = await _adminService.GetEntitiesAsync(ct);
            return Ok(entities);
        }

        /// <summary>
        /// Create a new entity.
        /// </summary>
        /// <param name="request">[REQUIRED] Entity details.</param>
        /// <param name="ct">[OPTIONAL] Cancellation token.</param>
        [HttpPost]
        [ProducesResponseType(typeof(string), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<string>> Create([FromBody] CreateEntityRequest request, [FromQuery] CancellationToken ct)
        {
            var id = await _adminService.CreateEntityAsync(request, ct);

            // Return 201 to match the attribute; replace with CreatedAtAction if you have a GET by id endpoint
            return Created($"/api/v1/entities/{id}", id);
        }
    }
}
