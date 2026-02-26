using System.Text.Json;
using CatalogService.Application.commands.attributes;
using CatalogService.Domain.Enums;
using FluentValidation;

namespace CatalogService.Application.Validation.attributes;

public  class CreateAttributeDefinitionRequestValidator : AbstractValidator<CreateattributeCommand>
{
    public CreateAttributeDefinitionRequestValidator()
    {


        RuleFor(x => x.AttributeKey)
            .NotEmpty().WithMessage("Attribute key is required.")
            .MaximumLength(100);

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.")
            .MaximumLength(200);

        RuleFor(x => x.DataType)
            .IsInEnum()
            .WithMessage("Unsupported DataType.");


        RuleFor(x => x.MinValue)
            .Null()
            .When(x => x.DataType != AttributeDataType.Number)
            .WithMessage("Min/Max is allowed only for number type.");

        RuleFor(x => x.MaxValue)
            .Null()
            .When(x => x.DataType != AttributeDataType.Number)
            .WithMessage("Min/Max is allowed only for number type.");


        RuleFor(x => x)
            .Must(x => x.MinValue is null || x.MaxValue is null || x.MinValue <= x.MaxValue)
            .WithMessage("MinValue cannot be greater than MaxValue.");

        RuleFor(x => x.AllowedValues)
            .Must(BeNullUndefinedOrArray)
            .WithMessage("AllowedValues must be a JSON array.");

        RuleFor(x => x.AllowedValues)
            .Must(av => IsNullOrUndefined(av))
            .When(x => x.DataType == AttributeDataType.Boolean)
            .WithMessage("AllowedValues is not valid for boolean type.");

        RuleFor(x => x.AllowedValues)
            .Must(av => av is not null
                        && av.Value.ValueKind == JsonValueKind.Array
                        && av.Value.GetArrayLength() > 0)
            .When(x => x.DataType == AttributeDataType.Enum)
            .WithMessage("Enum type must have allowed values.");

        RuleFor(x => x)
            .Must(AllowedValuesItemsAreValidForType)
            .WithMessage("AllowedValues contains invalid items for the given DataType.");

        RuleFor(x => x)
            .Must(DefaultValueTypeIsValidForDataType)
            .WithMessage("DefaultValue is not valid for the given DataType.");

        RuleFor(x => x)
            .Must(DefaultValueMustBeWithinAllowedValues)
            .WithMessage("DefaultValue must be within AllowedValues.")
            .When(x => HasNonEmptyArray(x.AllowedValues) && x.DefaultValue is not null);

        RuleFor(x => x)
            .Must(DefaultNumberWithinMinMax)
            .WithMessage("DefaultValue is below MinValue or above MaxValue.")
            .When(x => x.DataType == AttributeDataType.Number && x.DefaultValue is not null);

    }


    private static bool IsNullOrUndefined(JsonElement? e)
        => e is null || e.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined;

    private static bool HasNonEmptyArray(JsonElement? e)
        => e is not null
           && e.Value.ValueKind == JsonValueKind.Array
           && e.Value.GetArrayLength() > 0;


    private static bool BeNullUndefinedOrArray(JsonElement? e)
    {
        if (IsNullOrUndefined(e))
        {
            return true;
        }

        return e!.Value.ValueKind == JsonValueKind.Array;
    }

    private static bool AllowedValuesItemsAreValidForType(CreateattributeCommand x)
    {
        JsonElement? av = x.AllowedValues;

        // If not provided => OK (except enum rule is handled separately)
        if (IsNullOrUndefined(av))
        {
            return true;
        }

        if (av!.Value.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (JsonElement item in av.Value.EnumerateArray())
        {
            if (item.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                continue;
            }

            switch (x.DataType)
            {
                case AttributeDataType.Number:
                    if (item.ValueKind != JsonValueKind.Number)
                    {
                        return false;
                    }

                    if (!item.TryGetDecimal(out decimal num))
                    {
                        return false;
                    }

                    if (x.MinValue is not null && num < x.MinValue.Value)
                    {
                        return false;
                    }

                    if (x.MaxValue is not null && num > x.MaxValue.Value)
                    {
                        return false;
                    }

                    break;

                case AttributeDataType.Strings:
                case AttributeDataType.Enum:
                    if (item.ValueKind != JsonValueKind.String)
                    {
                        return false;
                    }

                    string? s = item.GetString();
                    if (string.IsNullOrWhiteSpace(s))
                    {
                        return false;
                    }

                    break;

                case AttributeDataType.Boolean:
                    // blocked earlier
                    return false;

                default:
                    return false;
            }
        }

        // Your domain requires enum array not empty (handled earlier)
        return true;
    }

    private static bool DefaultValueTypeIsValidForDataType(CreateattributeCommand x)
    {
        if (x.DefaultValue is null)
        {
            return true;
        }

        JsonElement dv = x.DefaultValue.Value;

        return x.DataType switch
        {
            AttributeDataType.Number => dv.ValueKind == JsonValueKind.Number,
            AttributeDataType.Boolean => dv.ValueKind is JsonValueKind.True or JsonValueKind.False,
            AttributeDataType.Strings => dv.ValueKind == JsonValueKind.String,
            AttributeDataType.Enum => dv.ValueKind == JsonValueKind.String,
            _ => false
        };
    }

    private static bool DefaultValueMustBeWithinAllowedValues(CreateattributeCommand x)
    {
        if (x.DefaultValue is null)
        {
            return true;
        }

        if (!HasNonEmptyArray(x.AllowedValues))
        {
            return true;
        }

        JsonElement av = x.AllowedValues!.Value;
        JsonElement dv = x.DefaultValue.Value;

        if (x.DataType is AttributeDataType.Enum or AttributeDataType.Strings)
        {
            if (dv.ValueKind != JsonValueKind.String)
            {
                return false;
            }

            string? def = dv.GetString();
            if (string.IsNullOrWhiteSpace(def))
            {
                return true; // domain allows empty? you block empty in allowed, default can be empty -> treat as ignore
            }

            foreach (JsonElement item in av.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.String &&
                    string.Equals(item.GetString(), def, StringComparison.OrdinalIgnoreCase))
                {
                    return true;
                }
            }

            return false;
        }

        if (x.DataType == AttributeDataType.Number)
        {
            if (dv.ValueKind != JsonValueKind.Number)
            {
                return false;
            }

            if (!dv.TryGetDecimal(out decimal defNum))
            {
                return false;
            }

            foreach (JsonElement item in av.EnumerateArray())
            {
                if (item.ValueKind == JsonValueKind.Number && item.TryGetDecimal(out decimal n) && n == defNum)
                {
                    return true;
                }
            }

            return false;
        }

        // boolean has no allowed values by rule
        return true;
    }

    private static bool DefaultNumberWithinMinMax(CreateattributeCommand x)
    {
        if (x.DefaultValue is null)
        {
            return true;
        }

        if (x.DataType != AttributeDataType.Number)
        {
            return true;
        }

        JsonElement dv = x.DefaultValue.Value;
        if (dv.ValueKind != JsonValueKind.Number)
        {
            return false;
        }

        if (!dv.TryGetDecimal(out decimal n))
        {
            return false;
        }

        if (x.MinValue is not null && n < x.MinValue.Value)
        {
            return false;
        }

        if (x.MaxValue is not null && n > x.MaxValue.Value)
        {
            return false;
        }

        return true;
    }


}
