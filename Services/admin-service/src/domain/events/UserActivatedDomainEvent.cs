namespace AdminService.Domain.Events
{
    /// <summary>
    /// Domain event raised when a user is activated
    /// </summary>
    public sealed record UserActivatedDomainEvent(Guid UserId);
}
