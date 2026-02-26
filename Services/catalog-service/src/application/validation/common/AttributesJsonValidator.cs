using System.Text.Json;
using CatalogService.Application.dtos;
using CatalogService.Domain.Enums;

namespace CatalogService.Application.Validation.Common;

public sealed class AttributesJsonValidator : IAttributesJsonValidator
{
    private readonly IAttributeDefinitionProvider _provider;

    public AttributesJsonValidator(IAttributeDefinitionProvider provider)
        => _provider = provider;

    public async Task<IReadOnlyList<(string Path, string Message)>> ValidateAsync(
        string screenKey,
        object? attributesPayload,
        CancellationToken ct)
    {
        var failures = new List<(string Path, string Message)>();

        if (!TryNormalizeToObject(attributesPayload, out var root, out var error))
        {
            failures.Add(("Attributes", error ?? "Invalid Attributes payload."));
            return failures;
        }

        // No attributes => ok
        if (root.ValueKind == JsonValueKind.Undefined)
            return failures;

        // âœ… Fetch DTO definitions for this screenKey
        var defs = await _provider.GetByScreenKeyAsync(screenKey, ct);
        var defByKey = defs.ToDictionary(d => d.AttributeKey, d => d, StringComparer.OrdinalIgnoreCase);

        var props = root.EnumerateObject().ToList();

        if (props.Count > 50)
            failures.Add(("Attributes", "Attributes contain too many keys (max 50)."));

        foreach (var p in props)
        {
            if (!defByKey.ContainsKey(p.Name))
                failures.Add(($"Attributes.{p.Name}", "Unknown attribute key."));
        }

        foreach (var d in defs.Where(x => x.IsRequired))
        {
            if (!props.Any(p => string.Equals(p.Name, d.AttributeKey, StringComparison.OrdinalIgnoreCase)))
                failures.Add(($"Attributes.{d.AttributeKey}", "Value is required."));
        }

        foreach (var p in props)
        {
            if (!defByKey.TryGetValue(p.Name, out var def))
                continue;

            foreach (var msg in ValidateValue(p.Value, def))
                failures.Add(($"Attributes.{p.Name}", msg));
        }

        return failures;
    }

    // ---------------- Normalization ----------------

    private static bool TryNormalizeToObject(object? payload, out JsonElement root, out string? error)
    {
        root = default;
        error = null;

        if (payload is null)
        {
            root = default; // Undefined => treat as absent
            return true;
        }

        if (payload is string s)
        {
            if (string.IsNullOrWhiteSpace(s))
            {
                root = default;
                return true;
            }

            try
            {
                using var doc = JsonDocument.Parse(s);
                if (doc.RootElement.ValueKind != JsonValueKind.Object)
                {
                    error = "Attributes must be a JSON object (key/value pairs).";
                    return false;
                }

                root = doc.RootElement.Clone();
                return true;
            }
            catch
            {
                error = "Attributes must be a valid JSON object.";
                return false;
            }
        }

        if (payload is IDictionary<string, object?> dict)
        {
            root = JsonSerializer.SerializeToElement(dict);
            if (root.ValueKind != JsonValueKind.Object)
            {
                error = "Attributes must be an object (key/value pairs).";
                return false;
            }

            return true;
        }

        if (payload is JsonElement e)
        {
            if (e.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
            {
                root = default;
                return true;
            }

            if (e.ValueKind != JsonValueKind.Object)
            {
                error = "Attributes must be a JSON object (key/value pairs).";
                return false;
            }

            root = e;
            return true;
        }

        if (payload is JsonDocument d)
        {
            if (d.RootElement.ValueKind != JsonValueKind.Object)
            {
                error = "Attributes must be a JSON object (key/value pairs).";
                return false;
            }

            root = d.RootElement.Clone();
            return true;
        }

        error = $"Unsupported Attributes payload type: {payload.GetType().Name}.";
        return false;
    }

    // ---------------- Value validation ----------------

    private static IEnumerable<string> ValidateValue(JsonElement value, AttributeDefinitionDto def)
    {
        if (value.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            if (def.IsRequired)
                yield return "Value is required.";
            yield break;
        }

        switch (def.DataType)
        {
            case AttributeDataType.Number:
                {
                    if (value.ValueKind != JsonValueKind.Number || !value.TryGetDecimal(out var num))
                    {
                        yield return "Value must be a number.";
                        yield break;
                    }

                    if (def.MinValue is not null && def.MinValue !=0 && num < def.MinValue.Value)
                        yield return $"Value must be >= {def.MinValue.Value}.";

                    if (def.MaxValue is not null && def.MaxValue !=0 && num > def.MaxValue.Value)
                        yield return $"Value must be <= {def.MaxValue.Value}.";

                    if (HasNonEmptyArray(def.AllowedValues) && !NumberInAllowed(num, def.AllowedValues!.Value))
                        yield return "Value must be within AllowedValues.";

                    break;
                }

            case AttributeDataType.Boolean:
                if (value.ValueKind is not (JsonValueKind.True or JsonValueKind.False))
                    yield return "Value must be boolean (true/false).";
                break;

            case AttributeDataType.Enum:
                {
                    if (value.ValueKind != JsonValueKind.String)
                    {
                        yield return "Value must be a string.";
                        yield break;
                    }

                    var s = value.GetString();
                    if (string.IsNullOrWhiteSpace(s))
                    {
                        yield return "Value cannot be empty.";
                        yield break;
                    }

                    if (!HasNonEmptyArray(def.AllowedValues))
                    {
                        yield return "AllowedValues is required for Enum type.";
                        yield break;
                    }

                    if (!StringInAllowed(s!, def.AllowedValues!.Value))
                        yield return "Value must be within AllowedValues.";

                    break;
                }

            case AttributeDataType.Strings:
                {
                    if (value.ValueKind != JsonValueKind.String)
                    {
                        yield return "Value must be a string.";
                        yield break;
                    }

                    var s = value.GetString();
                    if (string.IsNullOrWhiteSpace(s))
                        yield return "Value cannot be empty.";

                    if (HasNonEmptyArray(def.AllowedValues) && !StringInAllowed(s!, def.AllowedValues!.Value))
                        yield return "Value must be within AllowedValues.";

                    break;
                }

            default:
                yield return $"Unsupported DataType '{def.DataType}'.";
                break;
        }
    }

    private static bool HasNonEmptyArray(JsonElement? e)
        => e is not null
           && e.Value.ValueKind == JsonValueKind.Array
           && e.Value.GetArrayLength() > 0;

    private static bool StringInAllowed(string value, JsonElement arr)
        => arr.EnumerateArray().Any(x =>
            x.ValueKind == JsonValueKind.String &&
            string.Equals(x.GetString(), value, StringComparison.OrdinalIgnoreCase));

    private static bool NumberInAllowed(decimal value, JsonElement arr)
        => arr.EnumerateArray().Any(x =>
            x.ValueKind == JsonValueKind.Number &&
            x.TryGetDecimal(out var n) && n == value);
}
