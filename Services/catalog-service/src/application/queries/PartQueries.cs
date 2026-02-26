using CatalogService.Domain.Aggregates;

namespace CatalogService.Application.Queries;

/// <summary>
/// Query to retrieve all parts with optional filtering.
/// </summary>
/// <param name="CountryCode">[OPTIONAL] 2-character country code.</param>
/// <param name="ComponentGroupId">[OPTIONAL] Filter by Component Group ID.</param>
/// <param name="ComponentTypeId">[OPTIONAL] Filter by Component Type ID.</param>
/// <param name="IsActive">[OPTIONAL] Filter by active status (default: true).</param>
/// <param name="IncludeDeleted">[OPTIONAL] Set to true to include deleted parts (default: false).</param>
/// <param name="Page">[OPTIONAL] Page number (default: 1).</param>
/// <param name="PageSize">[OPTIONAL] Items per page (default: 50).</param>
public record GetAllPartsQuery(
    string? CountryCode = null,
    Guid? ComponentGroupId = null,
    Guid? ComponentTypeId = null,
    bool? IsActive = true,
    bool IncludeDeleted = false,
    int Page = 1,
    int PageSize = 50
);

public record GetPartByIdQuery(Guid Id);

public record GetPartByCodeAndCountryQuery(string PartCode, string CountryCode);
