namespace AdminService.Domain.Events
{
    /// <summary>
    /// Domain event raised when a user is deactivated
    /// </summary>
    public sealed record UserDeactivatedDomainEvent(
        Guid UserId,
        string Reason);
}
