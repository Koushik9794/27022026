namespace AdminService.Application.Queries
{
    /// <summary>
    /// Query to get user by ID
    /// </summary>
    public sealed record GetUserByIdQuery(Guid UserId);

    public sealed record UserDto(
        Guid Id,
        string Email,
        string DisplayName,
        string Role,
        string Status,
        DateTime CreatedAt,
        DateTime? LastLoginAt);
}
