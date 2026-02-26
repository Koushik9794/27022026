namespace GssWebApi.Dto;

/// <summary>
/// Request to create a new warehouse configuration
/// </summary>
public record CreateConfigurationRequest
{
    /// <summary>
    /// Optional enquiry ID to link this configuration to
    /// </summary>
    public Guid? EnquiryId { get; init; }
    
    /// <summary>
    /// Configuration name
    /// </summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>
    /// Configuration description
    /// </summary>
    public string? Description { get; init; }
    
    /// <summary>
    /// Site details including building footprint and capacity
    /// </summary>
    public SiteDetails SiteDetails { get; init; } = new();
    
    /// <summary>
    /// Product groups to include (e.g., PALLET_RACKING, SHELVING)
    /// </summary>
    public List<string> ProductGroups { get; init; } = new();
    
    /// <summary>
    /// Initial layout data (optional)
    /// </summary>
    public object? InitialLayout { get; init; }
}

public record SiteDetails
{
    public BuildingFootprint BuildingFootprint { get; init; } = new();
    public int TargetCapacity { get; init; }
    public string Country { get; init; } = string.Empty;
    public string Currency { get; init; } = string.Empty;
}

public record BuildingFootprint
{
    public decimal Length { get; init; }
    public decimal Width { get; init; }
    public string Unit { get; init; } = "METERS";
}

/// <summary>
/// Configuration response with validation status
/// </summary>
public record ConfigurationResponse
{
    public Guid ConfigurationId { get; init; }
    public Guid SessionId { get; init; }
    public string Name { get; init; } = string.Empty;
    public string? Description { get; init; }
    public string Status { get; init; } = "DRAFT";
    public int Version { get; init; }
    public SiteDetails SiteDetails { get; init; } = new();
    public object? ConfigurationData { get; init; }
    public ValidationStatus ValidationStatus { get; init; } = new();
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
    public Guid CreatedBy { get; init; }
    public Dictionary<string, string> Links { get; init; } = new();
}

public record ValidationStatus
{
    public bool IsValid { get; init; }
    public DateTime? LastValidatedAt { get; init; }
    public List<ValidationResult> Errors { get; init; } = new();
    public List<ValidationResult> Warnings { get; init; } = new();
    public List<ValidationResult> Suggestions { get; init; } = new();
}

public record ValidationResult
{
    public string RuleId { get; init; } = string.Empty;
    public string RuleName { get; init; } = string.Empty;
    public string Category { get; init; } = string.Empty;
    public string Severity { get; init; } = string.Empty;
    public bool Passed { get; init; }
    public string Message { get; init; } = string.Empty;
    public List<string> AffectedComponents { get; init; } = new();
    public SuggestedFix? SuggestedFix { get; init; }
}

public record SuggestedFix
{
    public string Action { get; init; } = string.Empty;
    public string? Parameter { get; init; }
    public object? SuggestedValue { get; init; }
    public bool AutoApplicable { get; init; }
}

/// <summary>
/// Request to validate a configuration
/// </summary>
public record ValidateConfigurationRequest
{
    /// <summary>
    /// Rule categories to evaluate (SPATIAL, STRUCTURAL, SAFETY, PRICING)
    /// </summary>
    public List<string> RuleCategories { get; init; } = new();
    
    /// <summary>
    /// Whether to automatically apply fixes where possible
    /// </summary>
    public bool ApplyAutoFixes { get; init; }
}

/// <summary>
/// Validation response with detailed results
/// </summary>
public record ValidationResponse
{
    public Guid ConfigurationId { get; init; }
    public DateTime ValidationTimestamp { get; init; }
    public string OverallStatus { get; init; } = string.Empty;
    public ValidationSummary Summary { get; init; } = new();
    public List<ValidationResult> Results { get; init; } = new();
    public bool AutoFixesAvailable { get; init; }
    public Dictionary<string, string> Links { get; init; } = new();
}

public record ValidationSummary
{
    public int TotalRulesEvaluated { get; init; }
    public int ErrorCount { get; init; }
    public int WarningCount { get; init; }
    public int SuggestionCount { get; init; }
}
