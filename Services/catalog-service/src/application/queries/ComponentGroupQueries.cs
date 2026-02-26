using CatalogService.Domain.Aggregates;

namespace CatalogService.Application.Queries;

public record GetAllComponentGroupsQuery(bool IncludeInactive = false);

public record GetComponentGroupByIdQuery(Guid Id);

public record GetComponentGroupByCodeQuery(string Code);
