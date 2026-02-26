
namespace AdminService.Api.Dto;

public record RoleResponse(
    Guid Id,
    string RoleName,
    string? Description,
    bool IsActive,
    bool IsDeleted,
    string? CreatedBy,
    DateTimeOffset CreatedAt,
    string? ModifiedBy,
    DateTimeOffset? ModifiedAt,
    int PermissionCount = 0);
