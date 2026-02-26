#pragma warning disable CS8618
namespace RuleService.Domain.ValueObjects
{
    /// <summary>
    /// Parameter Source - indicates where a parameter's value comes from
    /// </summary>
    public enum ParameterSource
    {
        /// <summary>
        /// Static constant value (e.g., LevelPitch = 50mm)
        /// </summary>
        Static,
        
        /// <summary>
        /// User input from configuration (e.g., PalletWidth, WarehouseClearHeight)
        /// </summary>
        Parametric,
        
        /// <summary>
        /// Calculated by a formula rule (e.g., FrameDepth, HeightToDepthRatio)
        /// </summary>
        Calculated,
        
        /// <summary>
        /// Fetched from load chart or lookup table (e.g., BeamCapacity)
        /// </summary>
        Lookup
    }
}

