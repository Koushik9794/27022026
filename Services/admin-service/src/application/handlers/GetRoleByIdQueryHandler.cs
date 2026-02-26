using AdminService.Application.Queries;
using AdminService.Domain.Aggregates;
using AdminService.Domain.Services;

namespace AdminService.Application.Handlers;

public sealed class GetRoleByIdQueryHandler
{
    private readonly IRoleRepository _roles;
    public GetRoleByIdQueryHandler(IRoleRepository roles) => _roles = roles;

    public Task<Role?> Handle(GetRoleByIdQuery q, CancellationToken ct)
        => _roles.GetByIdAsync(q.Id, ct);
}
