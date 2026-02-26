using System.Collections.Generic;
using AdminService.Domain.Aggregates;
using MediatR;

namespace AdminService.Application.Queries;

public sealed record GetRolesByPermissionIdQuery(Guid PermissionId) : IRequest<IEnumerable<Role>>;
