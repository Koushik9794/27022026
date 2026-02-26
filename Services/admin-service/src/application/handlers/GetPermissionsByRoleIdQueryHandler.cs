using AdminService.Application.Queries;
using AdminService.Domain.Aggregates;
using AdminService.Infrastructure.Persistence;
using MediatR;

namespace AdminService.Application.Handlers;

public sealed class GetPermissionsByRoleIdQueryHandler : IRequestHandler<GetPermissionsByRoleIdQuery, IEnumerable<Permission>>
{
    private readonly IRolePermissionRepository _rp;
    public GetPermissionsByRoleIdQueryHandler(IRolePermissionRepository rp) => _rp = rp;

    public Task<IEnumerable<Permission>> Handle(GetPermissionsByRoleIdQuery q, CancellationToken ct)
        => _rp.GetPermissionsByRoleAsync(q.RoleId, ct);
}
