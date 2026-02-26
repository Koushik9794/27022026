using MediatR;
namespace AdminService.Application.Commands;

public sealed record RemovePermissionFromRoleCommand(Guid RoleId, Guid PermissionId) : IRequest<bool>;
