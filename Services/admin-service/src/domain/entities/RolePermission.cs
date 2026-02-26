namespace AdminService.Domain.Entities;

public sealed class RolePermission
{
    public Guid Id { get; init; }
    public Guid RoleId { get; init; }
    public Guid PermissionId { get; init; }
    public string CreatedBy { get; init; } = default!;
    public DateTime CreatedAt { get; init; }
}
