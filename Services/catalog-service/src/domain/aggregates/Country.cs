namespace CatalogService.Domain.Aggregates;

public sealed class Country
{
    public Guid Id { get; private set; }
    public string CountryCode { get; private set; } = default!;
    public string CountryName { get; private set; } = default!;
    public string CurrencyCode { get; private set; } = default!;
    public bool IsActive { get; private set; }
    public bool IsDelete { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public string? UpdatedBy { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Country() { }

    public static Country Create(
        string countryCode,
        string countryName,
        string currencyCode,
        string? createdBy = null)
    {
        ValidateCountryCode(countryCode);
        ValidateCountryName(countryName);
        ValidateCurrencyCode(currencyCode);

        return new Country
        {
            Id = Guid.NewGuid(),
            CountryCode = countryCode.Trim().ToUpperInvariant(),
            CountryName = countryName.Trim(),
            CurrencyCode = currencyCode.Trim().ToUpperInvariant(),
            IsActive = true,
            IsDelete = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string countryName,
        string currencyCode,
        string? updatedBy)
    {
        ValidateCountryName(countryName);
        ValidateCurrencyCode(currencyCode);

        CountryName = countryName.Trim();
        CurrencyCode = currencyCode.Trim().ToUpperInvariant();
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Deactivate()
    {
        IsActive = false;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Activate()
    {
        IsActive = true;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Delete(string? updatedBy)
    {
        IsDelete = true;
        UpdatedBy = updatedBy;
        UpdatedAt = DateTime.UtcNow;
    }

    private static void ValidateCountryCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 2)
        {
            throw new ArgumentException("Country code must be exactly 2 characters (ISO 3166-1 alpha-2).", nameof(code));
        }
    }

    private static void ValidateCountryName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Country name cannot be empty.", nameof(name));
        }
    }

    private static void ValidateCurrencyCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 3)
        {
            throw new ArgumentException("Currency code must be exactly 3 characters.", nameof(code));
        }
    }

    public static Country Rehydrate(
        Guid id,
        string countryCode,
        string countryName,
        string currencyCode,
        bool isActive,
        bool isDelete,
        DateTime createdAt,
        string? createdBy,
        string? updatedBy,
        DateTime updatedAt)
    {
        return new Country
        {
            Id = id,
            CountryCode = countryCode,
            CountryName = countryName,
            CurrencyCode = currencyCode,
            IsActive = isActive,
            IsDelete = isDelete,
            CreatedAt = createdAt,
            CreatedBy = createdBy,
            UpdatedBy = updatedBy,
            UpdatedAt = updatedAt
        };
    }
}
