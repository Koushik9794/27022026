namespace CatalogService.Application.Queries;

/// <summary>
/// Query to retrieve all component masters with optional filtering.
/// </summary>
public record GetAllComponentMastersQuery(
    string? CountryCode = null,
    Guid? ComponentGroupId = null,
    Guid? ComponentTypeId = null,
    bool? IsActive = true,
    bool IncludeDeleted = false,
    int Page = 1,
    int PageSize = 50
);

public record GetComponentMasterByIdQuery(Guid Id);

public record GetComponentMasterByCodeAndCountryQuery(string ComponentMasterCode, string CountryCode);
