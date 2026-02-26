using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Dtos;

/// <summary>
/// Data transfer object for Country information.
/// </summary>
/// <param name="Id">Unique identifier of the country.</param>
/// <param name="CountryCode">2-character ISO country code (e.g., 'IN').</param>
/// <param name="CountryName">Full name of the country (e.g., 'India').</param>
/// <param name="CurrencyCode">Primary currency code used in the country (e.g., 'INR').</param>
/// <param name="IsActive">Whether the country is active.</param>
/// <param name="IsDelete">Whether the country is soft-deleted.</param>
/// <param name="CreatedAt">Timestamp of creation.</param>
/// <param name="CreatedBy">User who created the record.</param>
/// <param name="UpdatedBy">User who last updated the record.</param>
/// <param name="UpdatedAt">Timestamp of last update.</param>
public record CountryDto(
    Guid Id,
    string CountryCode,
    string CountryName,
    string CurrencyCode,
    bool IsActive,
    bool IsDelete,
    DateTime CreatedAt,
    string? CreatedBy,
    string? UpdatedBy,
    DateTime UpdatedAt
);

/// <summary>
/// Request DTO for creating a new country.
/// </summary>
/// <param name="CountryCode">[REQUIRED] 2-character ISO code.</param>
/// <param name="CountryName">[REQUIRED] Full name of the country.</param>
/// <param name="CurrencyCode">[REQUIRED] 3-character currency code.</param>
/// <param name="CreatedBy">[OPTIONAL] User identifier.</param>
public record CreateCountryRequest(
    [Required] [StringLength(2, MinimumLength = 2)] string CountryCode,
    [Required] [MaxLength(100)] string CountryName,
    [Required] [StringLength(3, MinimumLength = 3)] string CurrencyCode,
    string? CreatedBy = null
);

/// <summary>
/// Request DTO for updating an existing country.
/// </summary>
/// <param name="CountryName">[REQUIRED] Updated name.</param>
/// <param name="CurrencyCode">[REQUIRED] Updated 3-character currency code.</param>
/// <param name="IsActive">[REQUIRED] Update active status.</param>
/// <param name="UpdatedBy">[OPTIONAL] User identifier.</param>
public record UpdateCountryRequest(
    [Required] [MaxLength(100)] string CountryName,
    [Required] [StringLength(3, MinimumLength = 3)] string CurrencyCode,
    [Required] bool IsActive,
    string? UpdatedBy = null
);
