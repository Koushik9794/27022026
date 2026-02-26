using AdminService.Application.Queries;
using AdminService.Domain.Entities;
using AdminService.Infrastructure.Persistence;
using MediatR;

namespace AdminService.Application.Handlers;

public sealed class GetAllEntitiesQueryHandler : IRequestHandler<GetAllEntitiesQuery, IEnumerable<AppEntity>>
{
    private readonly IEntityRepository _entities;
    public GetAllEntitiesQueryHandler(IEntityRepository entities) => _entities = entities;

    public Task<IEnumerable<AppEntity>> Handle(GetAllEntitiesQuery q, CancellationToken ct)
        => _entities.GetAllAsync(ct);
}
