namespace AdminService.Domain.Events
{
    /// <summary>
    /// Domain event raised when a user is registered
    /// </summary>
    public sealed record UserRegisteredDomainEvent(
        Guid UserId,
        string Email,
        string DisplayName,
        string Role);
}
