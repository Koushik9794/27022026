using MediatR;

namespace AdminService.Application.Commands
{
    /// <summary>
    /// Command to update user profile
    /// </summary>
    public sealed record UpdateUserCommand(
        Guid UserId,
        string DisplayName);
}
