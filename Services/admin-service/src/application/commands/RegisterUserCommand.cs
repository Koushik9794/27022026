
namespace AdminService.Application.Commands
{
    /// <summary>
    /// Command to register a new user
    /// </summary>
    public sealed record RegisterUserCommand(
        string Email,
        string DisplayName,
        string Role);

    public sealed record RegisterUserResult(Guid UserId);
}
