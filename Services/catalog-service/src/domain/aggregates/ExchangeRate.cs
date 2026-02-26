namespace CatalogService.Domain.Aggregates;

public sealed class ExchangeRate
{
    public Guid Id { get; private set; }
    public string BaseCurrency { get; private set; } = default!;
    public string QuoteCurrency { get; private set; } = default!;
    public decimal Rate { get; private set; }
    public DateTime ValidFrom { get; private set; }
    public DateTime? ValidEnd { get; private set; }
    public bool IsActive { get; private set; }
    public bool IsDelete { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }
    public string? UpdatedBy { get; private set; }
    public DateTime UpdatedAt { get; private set; }

    private ExchangeRate() { }

    public static ExchangeRate Create(
        string baseCurrency,
        string quoteCurrency,
        decimal rate,
        DateTime validFrom,
        DateTime? validEnd = null,
        string? createdBy = null)
    {
        ValidateCurrencyPair(baseCurrency, quoteCurrency);
        ValidateRate(rate);
        ValidateValidityPeriod(validFrom, validEnd);

        return new ExchangeRate
        {
            Id = Guid.NewGuid(),
            BaseCurrency = baseCurrency.Trim().ToUpperInvariant(),
            QuoteCurrency = quoteCurrency.Trim().ToUpperInvariant(),
            Rate = rate,
            ValidFrom = validFrom.ToUniversalTime().Date,
            ValidEnd = validEnd?.ToUniversalTime().Date,
            IsActive = true,
            IsDelete = false,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy,
            UpdatedAt = DateTime.UtcNow
        };
    }

    public void Update(
        decimal rate,
        DateTime validFrom,
        DateTime? validEnd,
        string? updatedBy)
    {
        ValidateRate(rate);
        ValidateValidityPeriod(validFrom, validEnd);

        Rate = rate;
        ValidFrom = validFrom.ToUniversalTime().Date;
        ValidEnd = validEnd?.ToUniversalTime().Date;
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

    private static void ValidateCurrencyPair(string baseCurrency, string quoteCurrency)
    {
        if (string.IsNullOrWhiteSpace(baseCurrency) || baseCurrency.Length != 3)
        {
            throw new ArgumentException("Base currency code must be 3 characters.", nameof(baseCurrency));
        }
        if (string.IsNullOrWhiteSpace(quoteCurrency) || quoteCurrency.Length != 3)
        {
            throw new ArgumentException("Quote currency code must be 3 characters.", nameof(quoteCurrency));
        }
        if (baseCurrency.Trim().Equals(quoteCurrency.Trim(), StringComparison.OrdinalIgnoreCase))
        {
            throw new ArgumentException("Base and Quote currencies cannot be the same.");
        }
    }

    private static void ValidateRate(decimal rate)
    {
        if (rate <= 0)
        {
            throw new ArgumentException("Exchange rate must be greater than zero.", nameof(rate));
        }
    }

    private static void ValidateValidityPeriod(DateTime validFrom, DateTime? validEnd)
    {
        if (validEnd.HasValue && validEnd.Value < validFrom)
        {
            throw new ArgumentException("Valid End date cannot be before Valid From date.");
        }
    }

    public static ExchangeRate Rehydrate(
        Guid id,
        string baseCurrency,
        string quoteCurrency,
        decimal rate,
        DateTime validFrom,
        DateTime? validEnd,
        bool isActive,
        bool isDelete,
        DateTime createdAt,
        string? createdBy,
        string? updatedBy,
        DateTime updatedAt)
    {
        return new ExchangeRate
        {
            Id = id,
            BaseCurrency = baseCurrency,
            QuoteCurrency = quoteCurrency,
            Rate = rate,
            ValidFrom = validFrom,
            ValidEnd = validEnd,
            IsActive = isActive,
            IsDelete = isDelete,
            CreatedAt = createdAt,
            CreatedBy = createdBy,
            UpdatedBy = updatedBy,
            UpdatedAt = updatedAt
        };
    }
}
