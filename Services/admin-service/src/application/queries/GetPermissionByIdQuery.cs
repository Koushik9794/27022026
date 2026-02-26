using AdminService.Domain.Aggregates;
using MediatR;

namespace AdminService.Application.Queries;

public sealed record GetPermissionByIdQuery(Guid Id) : IRequest<AdminService.Domain.Aggregates.Permission?>;
