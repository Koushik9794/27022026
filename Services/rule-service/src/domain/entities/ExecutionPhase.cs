#pragma warning disable CS8618
namespace RuleService.Domain.Entities
{
    /// <summary>
    /// Execution Phase - determines when the rule is evaluated in the pipeline
    /// </summary>
    public enum ExecutionPhase
    {
        /// <summary>
        /// Validate input parameters before any calculations
        /// </summary>
        InputValidation = 100,
        
        /// <summary>
        /// Calculate derived values (e.g., FrameDepth, BeamSpan)
        /// </summary>
        Calculation = 200,
        
        /// <summary>
        /// Validate structural constraints (e.g., height-to-depth ratio)
        /// </summary>
        Structural = 300,
        
        /// <summary>
        /// Check compliance with standards (e.g., IS 15635)
        /// </summary>
        Compliance = 400,
        
        /// <summary>
        /// Optimize design (e.g., beam selection)
        /// </summary>
        Optimization = 500,
        
        /// <summary>
        /// Generate final output values (e.g., BOM quantities)
        /// </summary>
        OutputGeneration = 600
    }
}

