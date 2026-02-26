namespace GssWebApi.Dto;

/// <summary>
/// Request to generate BOM
/// </summary>
public record GenerateBomRequest
{
    /// <summary>
    /// Configuration ID to generate BOM for
    /// </summary>
    public Guid ConfigurationId { get; init; }
    
    /// <summary>
    /// Output format (EXCEL, CSV, PDF)
    /// </summary>
    public string Format { get; init; } = "EXCEL";
    
    /// <summary>
    /// Include alternative components in BOM
    /// </summary>
    public bool IncludeAlternatives { get; init; }
}

/// <summary>
/// Request to generate quote
/// </summary>
public record GenerateQuoteRequest
{
    /// <summary>
    /// Configuration ID to generate quote for
    /// </summary>
    public Guid ConfigurationId { get; init; }
    
    /// <summary>
    /// Pricing rules to apply (DEALER_DISCOUNT, VOLUME_DISCOUNT)
    /// </summary>
    public List<string> PricingRules { get; init; } = new();
    
    /// <summary>
    /// Currency for the quote
    /// </summary>
    public string Currency { get; init; } = "INR";
    
    /// <summary>
    /// Quote validity in days
    /// </summary>
    public int ValidityDays { get; init; } = 30;
}

/// <summary>
/// Job creation response
/// </summary>
public record JobResponse
{
    /// <summary>
    /// Unique job identifier
    /// </summary>
    public Guid JobId { get; init; }
    
    /// <summary>
    /// Job type (BOM_GENERATION, QUOTE_GENERATION, FILE_PROCESSING)
    /// </summary>
    public string JobType { get; init; } = string.Empty;
    
    /// <summary>
    /// Current job status (PENDING, RUNNING, COMPLETED, FAILED)
    /// </summary>
    public string Status { get; init; } = "PENDING";
    
    /// <summary>
    /// Job creation timestamp
    /// </summary>
    public DateTime CreatedAt { get; init; }
    
    /// <summary>
    /// Estimated completion time
    /// </summary>
    public DateTime? EstimatedCompletionTime { get; init; }
    
    /// <summary>
    /// Links to related resources
    /// </summary>
    public Dictionary<string, string> Links { get; init; } = new();
}

/// <summary>
/// Job status response with results
/// </summary>
public record JobStatusResponse
{
    public Guid JobId { get; init; }
    public string JobType { get; init; } = string.Empty;
    public string Status { get; init; } = string.Empty;
    public int Progress { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? CompletedAt { get; init; }
    public JobResult? Result { get; init; }
    public List<string> Errors { get; init; } = new();
}

public record JobResult
{
    public Guid ResultId { get; init; }
    public string Format { get; init; } = string.Empty;
    public string DownloadUrl { get; init; } = string.Empty;
    public DateTime ExpiresAt { get; init; }
    public JobResultSummary? Summary { get; init; }
}

public record JobResultSummary
{
    public int TotalItems { get; init; }
    public decimal TotalCost { get; init; }
    public string Currency { get; init; } = string.Empty;
}
