#pragma warning disable CS8618
namespace RuleService.Domain.ValueObjects
{
    /// <summary>
    /// Parameter Definition - describes a parameter required by a rule
    /// </summary>
    public class ParameterDefinition
    {
        public string Name { get; private set; }
        public ParameterSource Source { get; private set; }
        public string DataType { get; private set; } // "number", "string", "boolean"
        public string? Unit { get; private set; } // "mm", "kg", etc.
        public object? DefaultValue { get; private set; }
        public bool IsRequired { get; private set; }
        public string? Description { get; private set; }
        
        // For lookup parameters
        public string? LookupTable { get; private set; }
        public string? LookupKey { get; private set; }
        
        private ParameterDefinition() { }
        
        /// <summary>
        /// Create a parameter definition
        /// </summary>
        public static ParameterDefinition Create(
            string name,
            ParameterSource source,
            string dataType,
            bool isRequired = true,
            string? unit = null,
            object? defaultValue = null,
            string? description = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Parameter name is required", nameof(name));
            if (string.IsNullOrWhiteSpace(dataType))
                throw new ArgumentException("Data type is required", nameof(dataType));
                
            return new ParameterDefinition
            {
                Name = name,
                Source = source,
                DataType = dataType,
                IsRequired = isRequired,
                Unit = unit,
                DefaultValue = defaultValue,
                Description = description
            };
        }
        
        /// <summary>
        /// Create a lookup parameter definition
        /// </summary>
        public static ParameterDefinition CreateLookup(
            string name,
            string lookupTable,
            string lookupKey,
            string dataType,
            string? unit = null,
            string? description = null)
        {
            var param = Create(name, ParameterSource.Lookup, dataType, true, unit, null, description);
            param.LookupTable = lookupTable;
            param.LookupKey = lookupKey;
            return param;
        }
    }
}

