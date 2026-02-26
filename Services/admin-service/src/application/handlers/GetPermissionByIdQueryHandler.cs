using AdminService.Application.Queries;
using AdminService.Domain.Aggregates;
using AdminService.Infrastructure.Persistence;
using MediatR;

namespace AdminService.Application.Handlers;

public sealed class GetPermissionByIdQueryHandler : IRequestHandler<GetPermissionByIdQuery, Permission?>
{
    private readonly IPermissionRepository _permissions;
    public GetPermissionByIdQueryHandler(IPermissionRepository permissions) => _permissions = permissions;

    public Task<Permission?> Handle(GetPermissionByIdQuery q, CancellationToken ct)
        => _permissions.GetByIdAsync(q.Id, ct);
}
