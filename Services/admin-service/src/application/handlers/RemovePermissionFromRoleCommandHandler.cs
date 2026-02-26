using AdminService.Application.Commands;
using AdminService.Infrastructure.Persistence;
using MediatR;

namespace AdminService.Application.Handlers;

public sealed class RemovePermissionFromRoleCommandHandler : IRequestHandler<RemovePermissionFromRoleCommand, bool>
{
    private readonly IRolePermissionRepository _rp;

    public RemovePermissionFromRoleCommandHandler(IRolePermissionRepository rp)
    {
        _rp = rp;
    }

    public async Task<bool> Handle(RemovePermissionFromRoleCommand request, CancellationToken ct)
    {
        if (!await _rp.ExistsAsync(request.RoleId, request.PermissionId, ct))
            return false;

        await _rp.RemoveAsync(request.RoleId, request.PermissionId, ct);
        return true;
    }
}
