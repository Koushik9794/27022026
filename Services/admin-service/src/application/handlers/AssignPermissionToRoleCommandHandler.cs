using AdminService.Application.Commands;
using AdminService.Domain.Entities;
using AdminService.Domain.Services;
using AdminService.Infrastructure.Persistence;
using MediatR;

namespace AdminService.Application.Handlers;

public sealed class AssignPermissionToRoleCommandHandler : IRequestHandler<AssignPermissionToRoleCommand, Guid>
{
    private readonly IRoleRepository _roles;
    private readonly IPermissionRepository _permissions;
    private readonly IRolePermissionRepository _rp;

    public AssignPermissionToRoleCommandHandler(IRoleRepository roles, IPermissionRepository permissions, IRolePermissionRepository rp)
    {
        _roles = roles; _permissions = permissions; _rp = rp;
    }

    public async Task<Guid> Handle(AssignPermissionToRoleCommand request, CancellationToken ct)
    {
        // Ensure both ends exist and not deleted
        _ = await _roles.GetByIdAsync(request.RoleId, ct) ?? throw new KeyNotFoundException("Role not found.");
        _ = await _permissions.GetByIdAsync(request.PermissionId, ct) ?? throw new KeyNotFoundException("Permission not found.");

        if (await _rp.ExistsAsync(request.RoleId, request.PermissionId, ct))
            throw new InvalidOperationException("Mapping already exists.");

        var rp = new RolePermission { Id = Guid.NewGuid(), RoleId = request.RoleId, PermissionId = request.PermissionId, CreatedBy = request.CreatedBy, CreatedAt = DateTime.UtcNow };
        return await _rp.AssignAsync(rp, ct);
    }
}
