using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace GssCommon.utility;

public static class JsonUtil
{
    public static List<string>? DeserializeStringList(string? json)
        => string.IsNullOrWhiteSpace(json) ? null : JsonSerializer.Deserialize<List<string>>(json);

    public static string? SerializeStringList(IReadOnlyList<string>? list)
        => list is null ? null : JsonSerializer.Serialize(list);

    public static JsonElement? ParseElement(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return null;
        }

        return JsonDocument.Parse(json).RootElement.Clone();
    }

    public static Dictionary<string, JsonElement> ParseObjectToDict(string? json)
    {
        var dict = new Dictionary<string, JsonElement>(StringComparer.OrdinalIgnoreCase);
        if (string.IsNullOrWhiteSpace(json))
        {
            return dict;
        }

        JsonElement root = JsonDocument.Parse(json).RootElement;
        if (root.ValueKind != JsonValueKind.Object)
        {
            return dict;
        }

        foreach (JsonProperty p in root.EnumerateObject())
        {
            dict[p.Name] = p.Value.Clone();
        }

        return dict;
    }

    public static string SerializeDictToJson(Dictionary<string, JsonElement> dict)
    {
        using var stream = new MemoryStream();
        using var writer = new Utf8JsonWriter(stream);
        writer.WriteStartObject();
        foreach (KeyValuePair<string, JsonElement> kv in dict)
        {
            writer.WritePropertyName(kv.Key);
            kv.Value.WriteTo(writer);
        }
        writer.WriteEndObject();
        writer.Flush();
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }
}
