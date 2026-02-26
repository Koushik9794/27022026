using System.Text.Json;
using CatalogService.Domain.Enums;

namespace CatalogService.Domain.Entities;

public sealed class AttributeDefinition
{
    public Guid Id { get; private set; } // Added Id
    public string AttributeKey { get; private set; } = default!;
    public string DisplayName { get; private set; } = default!;
    public string? Unit { get; private set; }

    //define data type(number/string/bool/enum)
    public AttributeDataType DataType { get; private set; }

    public decimal? MinValue { get; private set; }
    public decimal? MaxValue { get; private set; }

    // jsonb scalar value (number/string/bool) or null
    public JsonElement? DefaultValue { get; private set; }

    public bool IsRequired { get; private set; }   // You may keep this, but you said API validation won't enforce it.
    public JsonElement? AllowedValues { get; private set; } // enum only (JSON array)
    public string? Description { get; private set; }

    public AttributeScreen? Screen { get; private set; } // UI metadata

    public bool IsActive { get; private set; } = true;
    public bool IsDeleted { get; private set; }

    public DateTimeOffset CreatedAt { get; private set; }
    public string? CreatedBy { get; private set; }

    public DateTimeOffset? UpdatedAt { get; private set; }
    public string? UpdatedBy { get; private set; }

    private AttributeDefinition() { }

    // -------------------------
    // Factory: Create
    // -------------------------
    public static AttributeDefinition Create(
        string attributeKey,
        string displayName,
        AttributeDataType dataType,
        string? unit = null,
        decimal? minValue = null,
        decimal? maxValue = null,
        JsonElement? defaultValue = null,
        bool isRequired = false,
        JsonElement? allowedValues = null,
        string? description = null,
        AttributeScreen? screen = null,
        bool isActive = true,
        string? createdBy = null)
    {
        ValidateKey(attributeKey);
        ValidateDisplayName(displayName);
        ValidateText(unit, 50, nameof(unit));
        ValidateText(description, 1000, nameof(description));
        ValidateScreen(screen);
        ValidateDataTypeRules(
            dataType: dataType,
            minValue: minValue,
            maxValue: maxValue,
            allowedValues: allowedValues,
            defaultValue: defaultValue);

        //ValidateScreen(screen);

        var now = DateTimeOffset.UtcNow;

        return new AttributeDefinition
        {
            Id = Guid.NewGuid(),
            AttributeKey = attributeKey.Trim(),
            DisplayName = displayName.Trim(),
            Unit = unit?.Trim(),
            DataType = dataType,

            MinValue = minValue,
            MaxValue = maxValue,

            // Clone JsonElements to avoid JsonDocument lifetime issues
            DefaultValue = Clone(defaultValue),
            AllowedValues = Clone(allowedValues),
            Description = description,
            Screen = screen,

            IsRequired = isRequired,
            IsActive = isActive,
            IsDeleted = false,

            CreatedAt = now,
            CreatedBy = createdBy,
            UpdatedAt = now,
            UpdatedBy = createdBy
        };
    }

    // -------------------------
    // Factory: Rehydrate
    // -------------------------
    public static AttributeDefinition Rehydrate(
        Guid Id,
        string attributeKey,
        string displayName,
        string? unit,
        AttributeDataType dataType,
        decimal? minValue,
        decimal? maxValue,
        JsonElement? defaultValue,
        bool isRequired,
        JsonElement? allowedValues,
        string? description,

        AttributeScreen screen,
        bool isActive,
        bool isDeleted,
        DateTimeOffset createdAt,
        string? createdBy,
        DateTimeOffset? updatedAt,
        string? updatedBy)
    {
        ValidateKey(attributeKey);
        ValidateDisplayName(displayName);
        ValidateText(unit, 50, nameof(unit));
        ValidateText(description, 1000, nameof(description));

        ValidateDataTypeRules(
            dataType: dataType,
            minValue: minValue,
            maxValue: maxValue,
            allowedValues: allowedValues,
            defaultValue: defaultValue);



        return new AttributeDefinition
        {
            Id = Id,
            AttributeKey = attributeKey.Trim(),
            DisplayName = displayName.Trim(),
            Unit = unit?.Trim(),
            DataType = dataType,

            MinValue = minValue,
            MaxValue = maxValue,

            DefaultValue = Clone(defaultValue),
            AllowedValues = Clone(allowedValues),
            Description = description,
            Screen = screen,

            IsRequired = isRequired,
            IsActive = isActive,
            IsDeleted = isDeleted,

            CreatedAt = createdAt,
            CreatedBy = createdBy,
            UpdatedAt = updatedAt,
            UpdatedBy = updatedBy
        };
    }

    // -------------------------
    // Update
    // -------------------------
    public void Update(
        string attributeKey,
        string displayName,
        AttributeDataType dataType,
        string? unit,
        decimal? minValue,
        decimal? maxValue,
        JsonElement? defaultValue,
        bool isRequired,
        JsonElement? allowedValues,
        string? description,
        AttributeScreen? screen,
        bool isActive,
        string? updatedBy)
    {
        EnsureNotDeleted();

        ValidateKey(attributeKey);
        ValidateDisplayName(displayName);
        ValidateText(unit, 50, nameof(unit));
        ValidateText(description, 1000, nameof(description));

        ValidateDataTypeRules(
            dataType: dataType,
            minValue: minValue,
            maxValue: maxValue,
            allowedValues: allowedValues,
            defaultValue: defaultValue);

        ValidateScreen(screen);

        AttributeKey = attributeKey.Trim();
        DisplayName = displayName.Trim();
        Unit = unit?.Trim();
        DataType = dataType;

        MinValue = minValue;
        MaxValue = maxValue;

        DefaultValue = Clone(defaultValue);
        AllowedValues = Clone(allowedValues);
        Description = description;
        Screen = screen;

        IsRequired = isRequired;

        IsActive = isActive && !IsDeleted;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
    }

    // -------------------------
    // State transitions
    // -------------------------
    public void Activate(string? updatedBy)
    {
        EnsureNotDeleted();
        IsActive = true;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void Deactivate(string? updatedBy)
    {
        EnsureNotDeleted();
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
    }

    public void Delete(string? updatedBy)
    {
        IsDeleted = true;
        IsActive = false;
        UpdatedAt = DateTimeOffset.UtcNow;
        UpdatedBy = updatedBy;
    }

    private void EnsureNotDeleted()
    {
        if (IsDeleted)
            throw new InvalidOperationException($"AttributeDefinition '{AttributeKey}' is deleted and cannot be modified.");
    }

    // -------------------------
    // Validation helpers
    // -------------------------
    private static void ValidateKey(string key)
    {
        if (string.IsNullOrWhiteSpace(key))
            throw new ArgumentException("AttributeKey cannot be empty.", nameof(key));

        var trimmed = key.Trim();

        if (trimmed.Length > 100)
            throw new ArgumentException("AttributeKey cannot exceed 100 characters.", nameof(key));

        // Optional: enforce safe key chars (recommended for JSON keys)
        // if (!trimmed.All(c => char.IsLetterOrDigit(c) || c == '_' || c == '-' || c == '.'))
        //     throw new ArgumentException("AttributeKey contains invalid characters.", nameof(key));
    }

    private static void ValidateDisplayName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new ArgumentException("DisplayName cannot be empty.", nameof(name));

        if (name.Trim().Length > 200)
            throw new ArgumentException("DisplayName cannot exceed 200 characters.", nameof(name));
    }

    private static void ValidateText(string? value, int maxLength, string paramName)
    {
        if (value is null) return;

        if (value.Trim().Length > maxLength)
            throw new ArgumentException($"{paramName} cannot exceed {maxLength} characters.", paramName);
    }


    private static void ValidateScreen(AttributeScreen? screen)
    {
        if (screen is null) return;

        if (!Enum.IsDefined(typeof(AttributeScreen), screen.Value))
            throw new ArgumentOutOfRangeException(nameof(screen), $"Invalid Screen value: {screen}");
    }


    private static void ValidateDataTypeRules(
        AttributeDataType dataType,
        decimal? minValue,
        decimal? maxValue,
        JsonElement? allowedValues,
        JsonElement? defaultValue)
    {
        // Min/Max only valid for Number
        if (dataType != AttributeDataType.Number)
        {
            if (minValue is not null || maxValue is not null)
                throw new ArgumentException("MinValue/MaxValue are only allowed for Number attributes.");
        }
        else
        {
            if (minValue is not null && maxValue is not null && minValue > maxValue)
                throw new ArgumentException("MinValue cannot be greater than MaxValue.");
        }

        // AllowedValues only valid for Enum
        if (dataType != AttributeDataType.Enum)
        {
            if (allowedValues is not null && allowedValues.Value.ValueKind != JsonValueKind.Null)
                throw new ArgumentException("AllowedValues is only allowed for Enum attributes.");
        }
        else
        {
            ValidateAllowedValuesArray(allowedValues);
        }

        // DefaultValue must be scalar and must match DataType constraints
        if (defaultValue is not null)
        {
            ValidateDefaultValueScalar(defaultValue.Value, dataType);

            // If Enum, default must be within allowed values
            if (dataType == AttributeDataType.Enum && allowedValues is not null)
            {
                var defStr = defaultValue.Value.ValueKind == JsonValueKind.String ? defaultValue.Value.GetString() : null;
                if (!string.IsNullOrWhiteSpace(defStr))
                {
                    var allowed = allowedValues.Value.EnumerateArray()
                        .Where(x => x.ValueKind == JsonValueKind.String)
                        .Select(x => x.GetString()!)
                        .ToHashSet(StringComparer.OrdinalIgnoreCase);

                    if (allowed.Count > 0 && !allowed.Contains(defStr))
                        throw new ArgumentException("DefaultValue must be one of the AllowedValues for Enum attributes.");
                }
            }

            // If Number, default must respect min/max
            if (dataType == AttributeDataType.Number &&
                defaultValue.Value.ValueKind == JsonValueKind.Number &&
                defaultValue.Value.TryGetDecimal(out var num))
            {
                if (minValue is not null && num < minValue) throw new ArgumentException("DefaultValue cannot be less than MinValue.");
                if (maxValue is not null && num > maxValue) throw new ArgumentException("DefaultValue cannot be greater than MaxValue.");
            }
        }
    }

    private static void ValidateAllowedValuesArray(JsonElement? allowedValues)
    {
        if (allowedValues is null) return;

        var av = allowedValues.Value;

        if (av.ValueKind == JsonValueKind.Null) return;

        if (av.ValueKind != JsonValueKind.Array)
            throw new ArgumentException("AllowedValues must be a JSON array for Enum attributes.");

        if (av.GetArrayLength() == 0)
            throw new ArgumentException("AllowedValues cannot be empty for Enum attributes.");

        // Must be all strings and unique
        var values = av.EnumerateArray().ToList();

        if (values.Any(v => v.ValueKind != JsonValueKind.String))
            throw new ArgumentException("AllowedValues must contain only strings.");

        var distinct = values.Select(v => v.GetString()!)
            .Where(s => !string.IsNullOrWhiteSpace(s))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Count();

        if (distinct != values.Count)
            throw new ArgumentException("AllowedValues contains duplicates or blank values.");
    }

    private static void ValidateDefaultValueScalar(JsonElement dv, AttributeDataType dataType)
    {
        // Only allow scalar (or null). No objects/arrays.
        if (dv.ValueKind is JsonValueKind.Object or JsonValueKind.Array)
            throw new ArgumentException("DefaultValue must be a scalar JSON value (string/number/bool) or null.");

        if (dv.ValueKind == JsonValueKind.Null) return;

        switch (dataType)
        {
            case AttributeDataType.Number:
                if (dv.ValueKind != JsonValueKind.Number || !dv.TryGetDecimal(out _))
                    throw new ArgumentException("DefaultValue must be a number.");
                break;

            case AttributeDataType.Boolean:
                if (dv.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                    throw new ArgumentException("DefaultValue must be true/false.");
                break;

            case AttributeDataType.Strings:
                if (dv.ValueKind != JsonValueKind.String)
                    throw new ArgumentException("DefaultValue must be a string.");
                break;

            case AttributeDataType.Enum:
                if (dv.ValueKind != JsonValueKind.String)
                    throw new ArgumentException("DefaultValue must be a string for Enum.");
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(dataType), "Unsupported data type.");
        }
    }

    private static void ValidateScreen(JsonElement? screen)
    {
        if (screen is null) return;

        var s = screen.Value;
        if (s.ValueKind == JsonValueKind.Null) return;

        // Keep this light: allow object or array (metadata), but not scalar
        if (s.ValueKind is not (JsonValueKind.Object or JsonValueKind.Array))
            throw new ArgumentException("Screen must be a JSON object or array if provided.");
    }

    // -------------------------
    // JsonElement cloning helper
    // -------------------------
    private static JsonElement? Clone(JsonElement? element)
    {
        if (element is null) return null;

        var e = element.Value;

        // Preserve null
        if (e.ValueKind == JsonValueKind.Null)
            return e;

        // Clone by round-tripping raw text
        using var doc = JsonDocument.Parse(e.GetRawText());
        return doc.RootElement.Clone();
    }
}

