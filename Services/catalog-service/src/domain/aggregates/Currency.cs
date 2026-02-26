namespace CatalogService.Domain.Aggregates;

public sealed class Currency
{
    public Guid Id { get; private set; }
    public string CurrencyCode { get; private set; } = default!;
    public string CurrencyName { get; private set; } = default!;
    public string? CurrencyValue { get; private set; }
    public short DecimalUnit { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDelete { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public string? UpdatedBy { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private Currency() { }

    public static Currency Create(
        string currencyCode,
        string currencyName,
        string? currencyValue = null,
        short decimalUnit = 2,
        string? createdBy = null)
    {
        ValidateCurrencyCode(currencyCode);
        ValidateCurrencyName(currencyName);

        return new Currency
        {
            Id = Guid.NewGuid(),
            CurrencyCode = currencyCode.Trim().ToUpperInvariant(),
            CurrencyName = currencyName.Trim(),
            CurrencyValue = currencyValue?.Trim(),
            DecimalUnit = decimalUnit,
            IsActive = true,
            IsDelete = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        string currencyName,
        string? currencyValue,
        short decimalUnit,
        string? updatedBy)
    {
        ValidateCurrencyName(currencyName);

        CurrencyName = currencyName.Trim();
        CurrencyValue = currencyValue?.Trim();
        DecimalUnit = decimalUnit;
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

    private static void ValidateCurrencyCode(string code)
    {
        if (string.IsNullOrWhiteSpace(code) || code.Length != 3)
        {
            throw new ArgumentException("Currency code must be exactly 3 characters.", nameof(code));
        }
    }

    private static void ValidateCurrencyName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Currency name cannot be empty.", nameof(name));
        }
    }

    public static Currency Rehydrate(
        Guid id,
        string currencyCode,
        string currencyName,
        string? currencyValue,
        short decimalUnit,
        bool isActive,
        bool isDelete,
        DateTime createdAt,
        string? createdBy,
        string? updatedBy,
        DateTime updatedAt)
    {
        return new Currency
        {
            Id = id,
            CurrencyCode = currencyCode,
            CurrencyName = currencyName,
            CurrencyValue = currencyValue,
            DecimalUnit = decimalUnit,
            IsActive = isActive,
            IsDelete = isDelete,
            CreatedAt = createdAt,
            CreatedBy = createdBy,
            UpdatedBy = updatedBy,
            UpdatedAt = updatedAt
        };
    }
}
