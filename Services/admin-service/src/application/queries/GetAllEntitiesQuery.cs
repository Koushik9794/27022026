using System.Collections.Generic;
using AdminService.Domain.Entities;
using MediatR;

namespace AdminService.Application.Queries;

public sealed record GetAllEntitiesQuery : IRequest<IEnumerable<AppEntity>>;
