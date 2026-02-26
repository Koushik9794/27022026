
namespace AdminService.Api.Dto;

public record PermissionResponse(
    Guid Id,
    string PermissionName,
    string? Description,
    string ModuleName,
    string? EntityName,
    bool IsActive,
    bool IsDeleted,
    string CreatedBy,
    DateTime CreatedAt,
    string? ModifiedBy,
    DateTime? ModifiedAt);
