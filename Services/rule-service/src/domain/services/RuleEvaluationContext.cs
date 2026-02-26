using RuleService.Domain.ValueObjects;
using GssCommon.Common.Models.Configurator;

namespace RuleService.Domain.Services
{
    /// <summary>
    /// Rule Evaluation Context - tracks parameters, calculated values, and execution trace
    /// </summary>
    public class RuleEvaluationContext
    {
        public Guid ProductGroupId { get; set; }
        public Guid? CountryId { get; set; }
        
        // Input parameters from configuration
        public Dictionary<string, object> InputParameters { get; set; } = new();
        
        // Calculated values from formula rules
        public Dictionary<string, object> CalculatedValues { get; set; } = new();
        
        // BOM items generated during evaluation
        public List<BomItem> BomItems { get; set; } = new();
        
        // Parameter metadata tracking (source lineage)
        public Dictionary<string, ParameterMetadata> ParameterSources { get; set; } = new();
        
        // Validation results
        public List<RuleViolation> Violations { get; set; } = new();
        
        // Execution trace for debugging
        public List<RuleExecutionStep> ExecutionTrace { get; set; } = new();
        
        // Execution warnings
        private readonly List<string> _warnings = new();
        private readonly List<string> _errors = new();
        
        /// <summary>
        /// Try to get a parameter value from either input or calculated values
        /// </summary>
        public bool TryGetValue(string parameterName, out object? value)
        {
            // First check calculated values (from previous rules)
            if (CalculatedValues.TryGetValue(parameterName, out value))
                return true;
                
            // Then check input parameters (from JSON)
            if (InputParameters.TryGetValue(parameterName, out value))
                return true;
                
            value = null;
            return false;
        }
        
        /// <summary>
        /// Check if a parameter exists in context
        /// </summary>
        public bool HasValue(string parameterName)
        {
            return CalculatedValues.ContainsKey(parameterName) || 
                   InputParameters.ContainsKey(parameterName);
        }
        
        /// <summary>
        /// Store a calculated value with lineage tracking
        /// </summary>
        public void StoreCalculatedValue(string fieldName, object value, Guid sourceRuleId, string sourceRuleName)
        {
            CalculatedValues[fieldName] = value;
            ParameterSources[fieldName] = new ParameterMetadata
            {
                Name = fieldName,
                Value = value,
                Source = ParameterSource.Calculated,
                SourceRule = sourceRuleName,
                SourceRuleId = sourceRuleId,
                Timestamp = DateTime.UtcNow
            };
        }
        
        /// <summary>
        /// Add a warning
        /// </summary>
        public void AddWarning(string message)
        {
            _warnings.Add(message);
        }
        
        /// <summary>
        /// Add an error
        /// </summary>
        public void AddError(string message)
        {
            _errors.Add(message);
        }
        
        public IReadOnlyList<string> Warnings => _warnings.AsReadOnly();
        public IReadOnlyList<string> Errors => _errors.AsReadOnly();
    }
    
    public class ParameterMetadata
    {
        public string Name { get; set; } = string.Empty;
        public object? Value { get; set; }
        public ParameterSource Source { get; set; }
        public string? SourceRule { get; set; }
        public Guid? SourceRuleId { get; set; }
        public string? LookupQuery { get; set; }
        public DateTime Timestamp { get; set; }
    }
    
    public class RuleViolation
    {
        public Guid RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string Severity { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public Dictionary<string, object> Context { get; set; } = new();
    }
    
    public class RuleExecutionStep
    {
        public int ExecutionOrder { get; set; }
        public Guid RuleId { get; set; }
        public string RuleName { get; set; } = string.Empty;
        public string? Formula { get; set; }
        public Dictionary<string, object> Inputs { get; set; } = new();
        public Dictionary<string, object> Outputs { get; set; } = new();
        public string Status { get; set; } = string.Empty; // SUCCESS, FAILED, SKIPPED
        public long ExecutionTimeMs { get; set; }
        public List<Guid> Dependencies { get; set; } = new();
        public string? ErrorMessage { get; set; }
    }
}
