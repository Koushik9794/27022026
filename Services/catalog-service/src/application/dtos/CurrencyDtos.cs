using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Dtos;

/// <summary>
/// Data transfer object for Currency information.
/// </summary>
/// <param name="Id">Unique identifier of the currency.</param>
/// <param name="CurrencyCode">3-character ISO currency code (e.g., 'INR').</param>
/// <param name="CurrencyName">Full name of the currency (e.g., 'Indian Rupee').</param>
/// <param name="CurrencyValue">Optional display value or symbol.</param>
/// <param name="DecimalUnit">Number of decimal places (0-4).</param>
/// <param name="IsActive">Whether the currency is active.</param>
/// <param name="IsDelete">Whether the currency is soft-deleted.</param>
/// <param name="CreatedAt">Timestamp of creation.</param>
/// <param name="CreatedBy">User who created the record.</param>
/// <param name="UpdatedBy">User who last updated the record.</param>
/// <param name="UpdatedAt">Timestamp of last update.</param>
public record CurrencyDto(
    Guid Id,
    string CurrencyCode,
    string CurrencyName,
    string? CurrencyValue,
    short DecimalUnit,
    bool IsActive,
    bool IsDelete,
    DateTime CreatedAt,
    string? CreatedBy,
    string? UpdatedBy,
    DateTime UpdatedAt
);

/// <summary>
/// Request DTO for creating a new currency.
/// </summary>
/// <param name="CurrencyCode">[REQUIRED] 3-character ISO code.</param>
/// <param name="CurrencyName">[REQUIRED] Full name of the currency.</param>
/// <param name="CurrencyValue">[OPTIONAL] Display symbol or symbol.</param>
/// <param name="DecimalUnit">[OPTIONAL] Decimal places (default 2, range 0-4).</param>
/// <param name="CreatedBy">[OPTIONAL] User identifier.</param>
public record CreateCurrencyRequest(
    [Required] [StringLength(3, MinimumLength = 3)] string CurrencyCode,
    [Required] [MaxLength(100)] string CurrencyName,
    [MaxLength(100)] string? CurrencyValue,
    [Range(0, 4)] short DecimalUnit = 2,
    string? CreatedBy = null
);

/// <summary>
/// Request DTO for updating an existing currency.
/// </summary>
/// <param name="CurrencyName">[REQUIRED] Updated name.</param>
/// <param name="CurrencyValue">[OPTIONAL] Updated display symbol.</param>
/// <param name="DecimalUnit">[REQUIRED] Updated decimal places (0-4).</param>
/// <param name="IsActive">[REQUIRED] Update active status.</param>
/// <param name="UpdatedBy">[OPTIONAL] User identifier.</param>
public record UpdateCurrencyRequest(
    [Required] [MaxLength(100)] string CurrencyName,
    [MaxLength(100)] string? CurrencyValue,
    [Range(0, 4)] short DecimalUnit,
    [Required] bool IsActive,
    string? UpdatedBy = null
);
