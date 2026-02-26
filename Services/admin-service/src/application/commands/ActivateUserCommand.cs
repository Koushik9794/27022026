
namespace AdminService.Application.Commands
{
    /// <summary>
    /// Command to activate a user
    /// </summary>
    public sealed record ActivateUserCommand(Guid UserId);
}
