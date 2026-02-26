
namespace AdminService.Application.Commands
{
    /// <summary>
    /// Command to delete/deactivate a user
    /// </summary>
    public sealed record DeleteUserCommand(
        Guid UserId,
        string Reason);
}
