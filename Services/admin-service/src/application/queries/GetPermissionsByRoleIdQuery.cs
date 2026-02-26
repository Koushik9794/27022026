using System.Collections.Generic;
using AdminService.Domain.Aggregates;
using MediatR;

namespace AdminService.Application.Queries;

public sealed record GetPermissionsByRoleIdQuery(Guid RoleId) : IRequest<IEnumerable<Permission>>;
