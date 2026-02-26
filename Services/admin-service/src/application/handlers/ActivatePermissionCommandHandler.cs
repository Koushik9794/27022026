using AdminService.Application.Commands;
using AdminService.Infrastructure.Persistence;
using MediatR;

namespace AdminService.Application.Handlers;

public sealed class ActivatePermissionCommandHandler : IRequestHandler<ActivatePermissionCommand, bool>
{
    private readonly IPermissionRepository _permissions;
    public ActivatePermissionCommandHandler(IPermissionRepository permissions) => _permissions = permissions;

    public async Task<bool> Handle(ActivatePermissionCommand cmd, CancellationToken ct)
    {
        var existing = await _permissions.GetByIdAsync(cmd.Id, ct);
        if (existing is null) return false;

        if (cmd.Activate) existing.Activate(cmd.ModifiedBy, DateTime.UtcNow);
        else existing.Deactivate(cmd.ModifiedBy, DateTime.UtcNow);

        await _permissions.UpdateAsync(existing, ct);
        return true;
    }
}
