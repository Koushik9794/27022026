using Microsoft.AspNetCore.Mvc;
using Wolverine;
using AdminService.Application.Commands;
using AdminService.Application.Queries;
using AdminService.Application.Dtos;
using FluentValidation;

namespace AdminService.Api.Controllers
{
    [ApiController]
    [Route("api/v1/[controller]")]
    public class DealersController : ControllerBase
    {
        private readonly IMessageBus _mediator;

        public DealersController(IMessageBus mediator)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
        }

        /// <summary>
        /// Get all dealers
        /// </summary>
        [HttpGet]
        [ProducesResponseType(typeof(List<DealerDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAll()
        {
            try
            {
                var result = await _mediator.InvokeAsync<List<DealerDto>>(new GetAllDealersQuery());
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Get dealer by ID
        /// </summary>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(DealerDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetById(Guid id)
        {
            try
            {
                var result = await _mediator.InvokeAsync<DealerDto?>(new GetDealerByIdQuery(id));
                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Create a new dealer
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Create([FromBody] CreateDealerCommand command)
        {
            try
            {
                var id = await _mediator.InvokeAsync<Guid>(command);
                return CreatedAtAction(nameof(GetById), new { id }, id);
            }
            catch (ValidationException ex)
            {
                return BadRequest(ex.Errors);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }
        }

        /// <summary>
        /// Update an existing dealer
        /// </summary>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateDealerCommand command)
        {
            if (id != command.Id)
            {
                return BadRequest("ID mismatch");
            }

            try
            {
                await _mediator.InvokeAsync(command);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
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
                await _mediator.InvokeAsync(new DeleteDealerCommand(id, updatedBy));
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
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
                await _mediator.InvokeAsync(new ActivateDealerCommand(id, updatedBy));
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
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
                await _mediator.InvokeAsync(new DeactivateDealerCommand(id, updatedBy));
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { message = ex.Message });
            }

        }
    }
}
