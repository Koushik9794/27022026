using System;
using System.Collections.Generic;
using System.Text.Json;

namespace GssCommon.Common.Models.Configurator
{
    public class GenericComponent 
    {
        public string Type { get; set; }
        public GenericComponent Parent { get; set; }
        public Dictionary<string, object> Attributes { get; set; } = new Dictionary<string, object>();
        public Dictionary<string, double> Defaults { get; set; } = new Dictionary<string, double>();
        public List<string> Rules { get; set; } = new List<string>();
        public List<GenericComponent> Children { get; set; } = new List<GenericComponent>();

        /// <summary>
        /// Safe accessor for numeric values. Handles JsonElement types and path-based Default lookups.
        /// </summary>
        public double GetNum(string key) 
        {
            // Path-based lookup for Defaults (e.g., "Defaults.MAX_RACK_HEIGHT")
            if (key.StartsWith("Defaults.", StringComparison.OrdinalIgnoreCase)) 
            {
                string defaultKey = key.Substring(9);
                if (Defaults.TryGetValue(defaultKey, out double defVal)) return defVal;
                // Check parent hierarchy for defaults if not found locally
                return Parent?.GetNum(key) ?? 0;
            }

            if (Attributes.TryGetValue(key, out var val)) 
            {
                if (val is JsonElement elem) 
                {
                    if (elem.ValueKind == JsonValueKind.Number) return elem.GetDouble();
                    if (elem.ValueKind == JsonValueKind.String && double.TryParse(elem.GetString(), out var d)) return d;
                    return 0; 
                }
                if (val is double dbl) return dbl;
                if (val is int i) return i;
                if (val is float f) return f;
                if (val is decimal dec) return (double)dec;
                return 0;
            }
            
            // Recursive check in parent if not found (useful for inherited variables)
            return Parent?.GetNum(key) ?? 0;
        }

        /// <summary>
        /// Safe accessor for boolean values. Handles JsonElement types.
        /// </summary>
        public bool GetBool(string key) 
        {
            if (Attributes.TryGetValue(key, out var val)) 
            {
                if (val is JsonElement elem) 
                {
                    if (elem.ValueKind == JsonValueKind.True) return true;
                    if (elem.ValueKind == JsonValueKind.False) return false;
                    if (elem.ValueKind == JsonValueKind.String && bool.TryParse(elem.GetString(), out var b)) return b;
                    return false;
                }
                if (val is bool bval) return bval;
                return false;
            }
            return Parent?.GetBool(key) ?? false;
        }
    }
}
