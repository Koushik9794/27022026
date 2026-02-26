using System;
using System.Collections.Generic;
using System.Text;
using System.Text.Json;

namespace GssCommon.utility;

public static class Jsonb
{
    public static readonly JsonSerializerOptions Options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public static string ToJson<T>(T obj) => JsonSerializer.Serialize(obj, Options);

    public static JsonElement ToElement(string json)
        => JsonDocument.Parse(json).RootElement.Clone();

    public static string ToJsonElementDictionary(IReadOnlyDictionary<string, JsonElement> dict)
    {
        using var doc = JsonDocument.Parse("{}");
        // easiest: serialize via intermediate Dictionary<string, object?> is painful because JsonElement
        // Instead: build a JSON object by writing raw text:
        using var stream = new MemoryStream();
        using (var writer = new Utf8JsonWriter(stream))
        {
            writer.WriteStartObject();
            foreach (KeyValuePair<string, JsonElement> kv in dict)
            {
                writer.WritePropertyName(kv.Key);
                kv.Value.WriteTo(writer);
            }
            writer.WriteEndObject();
        }
        return System.Text.Encoding.UTF8.GetString(stream.ToArray());
    }
}

