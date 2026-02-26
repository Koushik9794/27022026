namespace GssWebApi.Dto;

/// <summary>
/// Login request with credentials
/// </summary>
/// <summary>
/// Login request with credentials.
/// </summary>
/// <param name="Email">[REQUIRED] User email or username.</param>
/// <param name="Password">[REQUIRED] User password.</param>
/// <param name="MfaCode">[OPTIONAL] MFA code (if MFA is enabled and this is second step).</param>
public record LoginRequest
{
    public string Email { get; init; } = string.Empty;
    public string Password { get; init; } = string.Empty;
    public string? MfaCode { get; init; }
}

/// <summary>
/// Login response with JWT tokens
/// </summary>
public record LoginResponse
{
    /// <summary>
    /// JWT access token
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;
    
    /// <summary>
    /// Refresh token for obtaining new access tokens
    /// </summary>
    public string RefreshToken { get; init; } = string.Empty;
    
    /// <summary>
    /// Access token expiration time in seconds
    /// </summary>
    public int ExpiresIn { get; init; }
    
    /// <summary>
    /// Token type (always "Bearer")
    /// </summary>
    public string TokenType { get; init; } = "Bearer";
    
    /// <summary>
    /// User ID
    /// </summary>
    public Guid UserId { get; init; }
    
    /// <summary>
    /// Whether MFA is required (if true, submit mfaCode)
    /// </summary>
    public bool RequiresMfa { get; init; }
}

/// <summary>
/// Refresh token request
/// </summary>
/// <summary>
/// Refresh token request.
/// </summary>
/// <param name="RefreshToken">[REQUIRED] Valid refresh token.</param>
public record RefreshTokenRequest
{
    public string RefreshToken { get; init; } = string.Empty;
}

/// <summary>
/// Refresh token response
/// </summary>
public record RefreshTokenResponse
{
    /// <summary>
    /// New JWT access token
    /// </summary>
    public string AccessToken { get; init; } = string.Empty;
    
    /// <summary>
    /// Access token expiration time in seconds
    /// </summary>
    public int ExpiresIn { get; init; }
    
    /// <summary>
    /// Token type (always "Bearer")
    /// </summary>
    public string TokenType { get; init; } = "Bearer";
}

/// <summary>
/// Logout response
/// </summary>
public record LogoutResponse
{
    /// <summary>
    /// Logout message
    /// </summary>
    public string Message { get; init; } = "Logout successful";
}
