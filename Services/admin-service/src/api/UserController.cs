using Microsoft.AspNetCore.Mvc;
using Wolverine;
using AdminService.Application.Commands;
using AdminService.Application.Queries;

namespace AdminService.Api
{
    /// <summary>
    /// User management API endpoints
    /// </summary>
    [ApiController]
    [Route("api/v1/users")]
    [Produces("application/json")]
    public sealed class UserController : ControllerBase
    {
        private readonly IMessageBus _messageBus;

        public UserController(IMessageBus messageBus)
        {
            _messageBus = messageBus ?? throw new ArgumentNullException(nameof(messageBus));
        }

        /// <summary>
        /// Register a new user
        /// </summary>
        /// <param name="command">User registration details</param>
        /// <returns>Created user ID</returns>
        [HttpPost]
        [ProducesResponseType(typeof(RegisterUserResult), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> RegisterUser([FromBody] RegisterUserCommand command)
        {
            var result = await _messageBus.InvokeAsync<RegisterUserResult>(command);
            return CreatedAtAction(nameof(GetUserById), new { id = result.UserId }, result);
        }

        /// <summary>
        /// Get user by ID
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>User details</returns>
        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(Guid id)
        {
            var user = await _messageBus.InvokeAsync<UserDto?>(new GetUserByIdQuery(id));
            return user == null ? NotFound() : Ok(user);
        }

        /// <summary>
        /// Get all users
        /// </summary>
        /// <returns>List of all users</returns>
        [HttpGet]
        [ProducesResponseType(typeof(List<UserDto>), StatusCodes.Status200OK)]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _messageBus.InvokeAsync<List<UserDto>>(new GetAllUsersQuery());
            return Ok(users);
        }

        /// <summary>
        /// Activate a user account
        /// </summary>
        /// <param name="id">User ID</param>
        /// <returns>No content</returns>
        [HttpPost("{id}/activate")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> ActivateUser(Guid id)
        {
            await _messageBus.InvokeAsync(new ActivateUserCommand(id));
            return NoContent();
        }

        /// <summary>
        /// Update user profile
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="command">Update details</param>
        /// <returns>No content</returns>
        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserCommand command)
        {
            if (id != command.UserId)
                return BadRequest("User ID mismatch");

            await _messageBus.InvokeAsync(command);
            return NoContent();
        }

        /// <summary>
        /// Deactivate/delete a user
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="command">Deletion details</param>
        /// <returns>No content</returns>
        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteUser(Guid id, [FromBody] DeleteUserCommand command)
        {
            if (id != command.UserId)
                return BadRequest("User ID mismatch");

            await _messageBus.InvokeAsync(command);
            return NoContent();
        }
    }
}
