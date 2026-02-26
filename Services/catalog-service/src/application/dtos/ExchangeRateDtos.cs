using System.ComponentModel.DataAnnotations;

namespace CatalogService.Application.Dtos;

/// <summary>
/// Data transfer object for Exchange Rate information.
/// </summary>
/// <param name="Id">Unique identifier of the exchange rate.</param>
/// <param name="BaseCurrency">3-character base currency code (e.g., 'USD').</param>
/// <param name="QuoteCurrency">3-character target currency code (e.g., 'INR').</param>
/// <param name="Rate">Exchange rate value (Base * Rate = Quote).</param>
/// <param name="ValidFrom">Start date of the rate validity.</param>
/// <param name="ValidEnd">Optional end date of the rate validity.</param>
/// <param name="IsActive">Whether the rate is active.</param>
/// <param name="IsDelete">Whether the record is soft-deleted.</param>
/// <param name="CreatedAt">Timestamp of creation.</param>
/// <param name="CreatedBy">User who created the record.</param>
/// <param name="UpdatedBy">User who last updated the record.</param>
/// <param name="UpdatedAt">Timestamp of last update.</param>
public record ExchangeRateDto(
    Guid Id,
    string BaseCurrency,
    string QuoteCurrency,
    decimal Rate,
    DateTime ValidFrom,
    DateTime? ValidEnd,
    bool IsActive,
    bool IsDelete,
    DateTime CreatedAt,
    string? CreatedBy,
    string? UpdatedBy,
    DateTime UpdatedAt
);

/// <summary>
/// Request DTO for creating a new exchange rate.
/// </summary>
/// <param name="BaseCurrency">[REQUIRED] 3-character base currency code.</param>
/// <param name="QuoteCurrency">[REQUIRED] 3-character target currency code.</param>
/// <param name="Rate">[REQUIRED] Positive numeric rate.</param>
/// <param name="ValidFrom">[REQUIRED] Start of validity period.</param>
/// <param name="ValidEnd">[OPTIONAL] End of validity period.</param>
/// <param name="CreatedBy">[OPTIONAL] User identifier.</param>
public record CreateExchangeRateRequest(
    [Required] [StringLength(3, MinimumLength = 3)] string BaseCurrency,
    [Required] [StringLength(3, MinimumLength = 3)] string QuoteCurrency,
    [Required] [Range(0.00000001, double.MaxValue)] decimal Rate,
    [Required] DateTime ValidFrom,
    DateTime? ValidEnd = null,
    string? CreatedBy = null
);

/// <summary>
/// Request DTO for updating an existing exchange rate.
/// </summary>
/// <param name="Rate">[REQUIRED] Updated numeric rate.</param>
/// <param name="ValidFrom">[REQUIRED] Updated start of validity period.</param>
/// <param name="ValidEnd">[OPTIONAL] Updated end of validity period.</param>
/// <param name="IsActive">[REQUIRED] Update active status.</param>
/// <param name="UpdatedBy">[OPTIONAL] User identifier.</param>
public record UpdateExchangeRateRequest(
    [Required] [Range(0.00000001, double.MaxValue)] decimal Rate,
    [Required] DateTime ValidFrom,
    DateTime? ValidEnd,
    [Required] bool IsActive,
    string? UpdatedBy = null
);
