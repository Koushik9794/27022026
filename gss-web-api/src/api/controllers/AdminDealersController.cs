using GssWebApi.Dto;
using GssWebApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace GssWebApi.Api.Controllers
{
    /// <summary>
    /// Admin controller for managing Dealers.
    /// Proxies requests to the Admin Service.
    /// </summary>
    [ApiController]
    [Route("api/admin/dealers")]
    [Produces("application/json")]
    public class AdminDealersController : ControllerBase
    {
        private readonly IAdminServiceClient _adminClient;
        private readonly ILogger<AdminDealersController> _logger;

        public AdminDealersController(IAdminServiceClient adminClient, ILogger<AdminDealersController> logger)
        {
            _adminClient = adminClient;
            _logger = logger;
        }

        /// <summary>
        /// Get all dealers
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<DealerDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            var result = await _adminClient.GetDealersAsync();
            return Ok(result);
        }

        /// <summary>
        /// Get dealer by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DealerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _adminClient.GetDealerByIdAsync(id);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }

        /// <summary>
        /// Create a new dealer
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateDealerRequest request)
        {
            try
            {
                var id = await _adminClient.CreateDealerAsync(request);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (HttpRequestException ex)
            {
                return StatusCode((int)(ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError), ex.Message);
            }
        }

        /// <summary>
        /// Update an existing dealer
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDealerRequest request)
        {
            if (id != request.Id)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                var success = await _adminClient.UpdateDealerAsync(id, request);
                if (!success)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (HttpRequestException ex)
            {
                return StatusCode((int)(ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError), ex.Message);
            }
        }

        /// <summary>
        /// Delete a dealer
        /// </summary>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Delete(Guid id, [FromQuery] Guid updatedBy)
        {
            try
            {
                var success = await _adminClient.DeleteDealerAsync(id, updatedBy);
                if (!success)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (HttpRequestException ex)
            {
                return StatusCode((int)(ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError), ex.Message);
            }
        }

        /// <summary>
        /// Activate a dealer
        /// </summary>
        [HttpPost("{id}/activate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Activate(Guid id, [FromQuery] Guid updatedBy)
        {
            try
            {
                var success = await _adminClient.ActivateDealerAsync(id, updatedBy);
                if (!success)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (HttpRequestException ex)
            {
                return StatusCode((int)(ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError), ex.Message);
            }
        }

        /// <summary>
        /// Deactivate a dealer
        /// </summary>
        [HttpPost("{id}/deactivate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Deactivate(Guid id, [FromQuery] Guid updatedBy)
        {
            try
            {
                var success = await _adminClient.DeactivateDealerAsync(id, updatedBy);
                if (!success)
                {
                    return NotFound();
                }
                return NoContent();
            }
            catch (HttpRequestException ex)
            {
                return StatusCode((int)(ex.StatusCode ?? System.Net.HttpStatusCode.InternalServerError), ex.Message);
            }
        }
    }
}
