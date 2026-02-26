namespace GssWebApi.Dto;

/// <summary>
/// Standard error response model
/// </summary>
public record ErrorResponse
{
    /// <summary>
    /// Correlation ID for distributed tracing
    /// </summary>
    public string CorrelationId { get; init; } = string.Empty;
    
    /// <summary>
    /// Error code (VALIDATION_ERROR, AUTHENTICATION_ERROR, etc.)
    /// </summary>
    public string ErrorCode { get; init; } = string.Empty;
    
    /// <summary>
    /// Human-readable error message
    /// </summary>
    public string Message { get; init; } = string.Empty;
    
    /// <summary>
    /// Detailed error information
    /// </summary>
    public List<ErrorDetail> Details { get; init; } = new();
    
    /// <summary>
    /// Error timestamp
    /// </summary>
    public DateTime Timestamp { get; init; }
    
    /// <summary>
    /// Request path that caused the error
    /// </summary>
    public string Path { get; init; } = string.Empty;
}

public record ErrorDetail
{
    /// <summary>
    /// Field name that caused the error (for validation errors)
    /// </summary>
    public string? Field { get; init; }
    
    /// <summary>
    /// Detailed error message
    /// </summary>
    public string Message { get; init; } = string.Empty;
}

/// <summary>
/// Health check response
/// </summary>
public record HealthResponse
{
    /// <summary>
    /// Overall health status (healthy, degraded, unhealthy)
    /// </summary>
    public string Status { get; init; } = "healthy";
    
    /// <summary>
    /// Health check timestamp
    /// </summary>
    public DateTime Timestamp { get; init; }
    
    /// <summary>
    /// Service name
    /// </summary>
    public string Service { get; init; } = "gss-web-api";
    
    /// <summary>
    /// Service version
    /// </summary>
    public string Version { get; init; } = "1.0.0";
    
    /// <summary>
    /// Dependency health status
    /// </summary>
    public Dictionary<string, string> Dependencies { get; init; } = new();
}
