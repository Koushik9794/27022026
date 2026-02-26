namespace GssWebApi.Dto;

/// <summary>
/// User context response containing permissions, dealer info, and preferences
/// </summary>
public record UserContextResponse
{
    /// <summary>
    /// [REQUIRED] Unique user identifier
    /// </summary>
    public Guid UserId { get; init; }
    
    /// <summary>
    /// [REQUIRED] User email address
    /// </summary>
    public string Email { get; init; } = string.Empty;
    
    /// <summary>
    /// [REQUIRED] User full name
    /// </summary>
    public string FullName { get; init; } = string.Empty;
    
    /// <summary>
    /// [REQUIRED] User role (DEALER, ADMIN, DESIGNER, VIEWER)
    /// </summary>
    public string Role { get; init; } = string.Empty;
    
    /// <summary>
    /// [OPTIONAL] List of permissions assigned to the user
    /// </summary>
    public List<string> Permissions { get; init; } = new();
    
    /// <summary>
    /// [OPTIONAL] Dealer information (if user is a dealer)
    /// </summary>
    public DealerInfo? Dealer { get; init; }
    
    /// <summary>
    /// [OPTIONAL] User preferences including region, currency, language
    /// </summary>
    public UserPreferences Preferences { get; init; } = new();
    
    /// <summary>
    /// [OPTIONAL] Feature flags enabled for this user
    /// </summary>
    public FeatureFlags FeatureFlags { get; init; } = new();
}

public record DealerInfo
{
    /// <summary>[REQUIRED] Unique dealer identifier.</summary>
    public Guid DealerId { get; init; }
    /// <summary>[REQUIRED] Unique dealer code.</summary>
    public string DealerCode { get; init; } = string.Empty;
    /// <summary>[REQUIRED] Company name of the dealer.</summary>
    public string CompanyName { get; init; } = string.Empty;
    /// <summary>[OPTIONAL] Geographic territory.</summary>
    public string Territory { get; init; } = string.Empty;
}

public record UserPreferences
{
    /// <summary>[OPTIONAL] Cloud region.</summary>
    public string Region { get; init; } = string.Empty;
    /// <summary>[OPTIONAL] 2-character country code.</summary>
    public string Country { get; init; } = string.Empty;
    /// <summary>[OPTIONAL] Local currency code.</summary>
    public string Currency { get; init; } = string.Empty;
    /// <summary>[OPTIONAL] Language preference (default: "en").</summary>
    public string Language { get; init; } = "en";
    /// <summary>[OPTIONAL] Measurement unit (default: "METRIC").</summary>
    public string MeasurementUnit { get; init; } = "METRIC";
}

public record FeatureFlags
{
    /// <summary>[OPTIONAL] Whether 3D view is enabled.</summary>
    public bool Enable3DView { get; init; }
    /// <summary>[OPTIONAL] Whether real-time validation is enabled.</summary>
    public bool EnableRealTimeValidation { get; init; }
    /// <summary>[OPTIONAL] Whether bulk import is enabled.</summary>
    public bool EnableBulkImport { get; init; }
}
