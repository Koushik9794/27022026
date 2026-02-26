using AdminService.Application.Queries;
using AdminService.Domain.Aggregates;
using AdminService.Infrastructure.Persistence;
using MediatR;

namespace AdminService.Application.Handlers;

public sealed class GetRolesByPermissionIdQueryHandler : IRequestHandler<GetRolesByPermissionIdQuery, IEnumerable<Role>>
{
    private readonly IRolePermissionRepository _rp;
    public GetRolesByPermissionIdQueryHandler(IRolePermissionRepository rp) => _rp = rp;

    public Task<IEnumerable<Role>> Handle(GetRolesByPermissionIdQuery q, CancellationToken ct)
        => _rp.GetRolesByPermissionAsync(q.PermissionId, ct);
}
