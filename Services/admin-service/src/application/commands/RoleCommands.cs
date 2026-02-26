using MediatR;
using AdminService.Application.Dtos;

namespace AdminService.Application.Commands
{
    public sealed record CreateRoleCommand(
        string RoleName,
        string? Description,
        string? CreatedBy
    ) : IRequest<CreateRoleResult>;

    public sealed record UpdateRoleCommand(
        Guid RoleId,
        string RoleName,
        string? Description,
        string? ModifiedBy
    ) : IRequest;

    public sealed record DeleteRoleCommand(
        Guid RoleId,
        string? ModifiedBy
    ) : IRequest;

    public sealed record ActivateRoleCommand(Guid RoleId, bool Activate, string? ModifiedBy) : IRequest;
    // public sealed record DeactivateRoleCommand(Guid RoleId);
}
