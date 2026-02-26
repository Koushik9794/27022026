namespace AdminService.Application.Dtos
{
    public sealed record RoleDto(
        Guid Id,
        string RoleName,
        string? Description,
        bool IsActive,
        bool IsDeleted,
        string? CreatedBy,
        DateTimeOffset CreatedAt,
        string? ModifiedBy,
        DateTimeOffset? ModifiedAt
    );
}
