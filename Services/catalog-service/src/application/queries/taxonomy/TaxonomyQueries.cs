namespace CatalogService.Application.Queries.Taxonomy;

// Component Type Queries
public record GetAllComponentTypesQuery(string? ComponentGroupCode = null, Guid? ComponentGroupId = null, bool IncludeInactive = false);
public record GetComponentTypeByIdQuery(Guid Id);
public record GetComponentTypeByCodeQuery(string Code);

// Product Group Queries
public record GetAllProductGroupsQuery(bool IncludeInactive = false);
public record GetProductGroupByIdQuery(Guid Id);
public record GetProductGroupByCodeQuery(string Code);
public record GetProductGroupVariantsQuery(Guid ParentGroupId);

//Warehouse Type Queries
public sealed record GetAllWarehouseTypesQuery(bool IncludeInactive = false);
public sealed record GetWarehouseTypesByIdQuery(Guid Id);

//Civil Type Queries
public sealed record GetAllCivileComponentQuery(bool IncludeInactive = false);
public sealed record GetCivileComponentByIdQuery(Guid Id);
