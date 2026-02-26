namespace CatalogService.Application.Queries.Pallets;

/// <summary>
/// Query to get all pallets.
/// </summary>
public record GetAllPalletsQuery(bool IncludeInactive = false);

/// <summary>
/// Query to get a pallet by ID.
/// </summary>
public record GetPalletByIdQuery(Guid Id);

/// <summary>
/// Query to get a pallet by code.
/// </summary>
public record GetPalletByCodeQuery(string Code);
