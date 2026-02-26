using CatalogService.Domain.Aggregates;

namespace CatalogService.Application.Queries;

public record GetAllComponentNamesQuery(bool IncludeInactive = false);

public record GetComponentNameByIdQuery(Guid Id);

public record GetComponentNameByCodeQuery(string Code);

public record GetComponentNamesByTypeQuery(Guid ComponentTypeId, bool IncludeInactive = false);
