using AdminService.Application.Commands;
using AdminService.Infrastructure.Persistence;
using MediatR;

namespace AdminService.Application.Handlers;

public sealed class DeletePermissionCommandHandler : IRequestHandler<DeletePermissionCommand, bool>
{
    private readonly IPermissionRepository _permissions;
    public DeletePermissionCommandHandler(IPermissionRepository permissions) => _permissions = permissions;

    public async Task<bool> Handle(DeletePermissionCommand cmd, CancellationToken ct)
    {
        var existing = await _permissions.GetByIdAsync(cmd.Id, ct);
        if (existing is null) return false;

        await _permissions.SoftDeleteAsync(cmd.Id, cmd.ModifiedBy, DateTime.UtcNow, ct);
        return true;
    }
}
