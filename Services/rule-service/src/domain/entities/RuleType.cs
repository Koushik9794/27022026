#pragma warning disable CS8618
namespace RuleService.Domain.Entities
{
    /// <summary>
    /// Rule Type - determines how the rule is evaluated
    /// </summary>
    public enum RuleType
    {
        /// <summary>
        /// Conditional rule - checks conditions and returns validation result
        /// </summary>
        Condition,
        
        /// <summary>
        /// Formula rule - evaluates a formula and stores result in OutputField
        /// </summary>
        Formula,
        
        /// <summary>
        /// Lookup rule - queries external data (e.g., load charts)
        /// </summary>
        Lookup,
        
        /// <summary>
        /// Formula with validation - evaluates formula AND checks conditions
        /// </summary>
        FormulaWithValidation
    }
}

