using System.Collections.Generic;
using AdminService.Domain.Aggregates;
using MediatR;

namespace AdminService.Application.Queries;

public sealed record GetAllPermissionsQuery(string? ModuleName = null, string? EntityName = null) : IRequest<IEnumerable<Permission>>;
