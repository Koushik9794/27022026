using MediatR;
namespace AdminService.Application.Commands;

public sealed record AssignPermissionToRoleCommand(Guid RoleId, Guid PermissionId, string CreatedBy) : IRequest<Guid>;
