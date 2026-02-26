using System.Text.Json;
using CatalogService.Application.commands.attributes;
using CatalogService.Domain.Enums;
using FluentValidation;

namespace CatalogService.Application.Validation.attributes;

public class UpdateAttributeDefinitionRequestValidator : AbstractValidator<UpdateattributeCommand>
{
    public UpdateAttributeDefinitionRequestValidator()
    {


        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id is required.");

        RuleFor(x => x.AttributeKey)
            .NotEmpty().WithMessage("Attribute key is required.");

        RuleFor(x => x.DisplayName)
            .NotEmpty().WithMessage("Display name is required.");

        RuleFor(x => x.DataType)
            .IsInEnum().WithMessage("Unsupported DataType.");


        RuleFor(x => x)
                    .Must(x => x.DataType == AttributeDataType.Number || x.MinValue is null && x.MaxValue is null)
                    .WithMessage("Min/Max is allowed only for number type.");

        RuleFor(x => x)
            .Must(x => x.MinValue is null || x.MaxValue is null || x.MinValue <= x.MaxValue)
            .WithMessage("MinValue cannot be greater than MaxValue.");

        RuleFor(x => x.AllowedValues)
            .Must(BeNullOrUndefinedOrArray)
            .WithMessage("AllowedValues must be a JSON array.");

        RuleFor(x => x)
            .Must(x => x.DataType != AttributeDataType.Boolean || IsNullOrUndefined(x.AllowedValues))
            .WithMessage("AllowedValues is not valid for boolean type.");


        RuleFor(x => x)
            .Must(x => x.DataType != AttributeDataType.Enum || HasNonEmptyArray(x.AllowedValues))
            .WithMessage("Enum type must have allowed values.");

        // AllowedValues per-item validation (type + range checks)
        RuleFor(x => x)
            .Must(AllowedValuesItemsAreValidForType)
            .WithMessage("AllowedValues contains invalid items for the given DataType.");

        RuleFor(x => x)
            .Must(DefaultValueWithinAllowedValues)
            .When(x => x.DefaultValue is not null && HasNonEmptyArray(x.AllowedValues))
            .WithMessage("DefaultValue must be within AllowedValues.");

        RuleFor(x => x)
            .Must(DefaultNumberWithinMinMax)
            .When(x => x.DefaultValue is not null && x.DataType == AttributeDataType.Number)
            .WithMessage("DefaultValue is below MinValue or above MaxValue.");

    }


    private static bool IsNullOrUndefined(JsonElement? e)
        => e is null || e.Value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined;

    private static bool BeNullOrUndefinedOrArray(JsonElement? e)
        => IsNullOrUndefined(e) || e!.Value.ValueKind == JsonValueKind.Array;

    private static bool HasNonEmptyArray(JsonElement? e)
        => e is not null && e.Value.ValueKind == JsonValueKind.Array && e.Value.GetArrayLength() > 0;

    private static bool AllowedValuesItemsAreValidForType(UpdateattributeCommand x)
    {
        // If not provided => OK (Enum non-empty handled by separate rule)
        if (IsNullOrUndefined(x.AllowedValues))
        {
            return true;
        }

        JsonElement av = x.AllowedValues!.Value;
        if (av.ValueKind != JsonValueKind.Array)
        {
            return false;
        }

        foreach (JsonElement item in av.EnumerateArray())
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
                    // Already blocked earlier: AllowedValues invalid for Boolean
                    return false;

                default:
                    return false;
            }
        }

        return true;
    }



    private static bool DefaultValueWithinAllowedValues(UpdateattributeCommand x)
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
                return true;
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
                if (item.ValueKind == JsonValueKind.Number &&
                    item.TryGetDecimal(out decimal n) &&
                    n == defNum)
                {
                    return true;
                }
            }

            return false;
        }

        // Boolean has no allowed values by rule
        return true;
    }

    private static bool DefaultNumberWithinMinMax(UpdateattributeCommand x)
    {
        if (x.DefaultValue is null)
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
