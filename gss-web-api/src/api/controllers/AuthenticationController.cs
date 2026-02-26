
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GssWebApi.Dto;

namespace GssWebApi.Api.Controllers;

/// <summary>
/// Authentication endpoints for login, logout, and token management
/// </summary>
[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthenticationController : ControllerBase
{
    private readonly ILogger<AuthenticationController> _logger;

    public AuthenticationController(ILogger<AuthenticationController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// User login
    /// </summary>
    /// <remarks>
    /// Authenticate user with email/username and password. Returns JWT access token and refresh token.
    /// 
    /// This endpoint proxies to the authentication service (Cognito or admin-service) and returns tokens.
    /// 
    /// **User Journey:** 01_Login
    /// 
    /// **Flow:**
    /// 1. User submits credentials
    /// 2. BFF validates and forwards to auth service
    /// 3. On success, returns JWT tokens
    /// 4. Frontend stores tokens securely (httpOnly cookie recommended)
    /// 5. MFA challenge may be required (returned in response)
    /// 
    /// **Sample Request:**
    /// ```json
    /// {
    ///   "email": "dealer@example.com",
    ///   "password": "SecurePassword123!"
    /// }
    /// ```
    /// 
    /// **Sample Response (Success):**
    /// ```json
    /// {
    ///   "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    ///   "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
    ///   "expiresIn": 3600,
    ///   "tokenType": "Bearer",
    ///   "userId": "550e8400-e29b-41d4-a716-446655440000",
    ///   "requiresMfa": false
    /// }
    /// ```
    /// 
    /// **Sample Response (MFA Required):**
    /// ```json
    /// {
    ///   "accessToken": "",
    ///   "refreshToken": "",
    ///   "expiresIn": 0,
    ///   "tokenType": "Bearer",
    ///   "userId": "550e8400-e29b-41d4-a716-446655440000",
    ///   "requiresMfa": true
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">[REQUIRED] Login credentials details.</param>
    /// <response code="200">Login successful - Returns JWT tokens</response>
    /// <response code="401">Invalid credentials</response>
    /// <response code="423">Account locked due to too many failed attempts</response>
    /// <response code="500">Internal server error</response>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status423Locked)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<LoginResponse>> Login([FromBody] LoginRequest request)
    {
        // TODO: Implement authentication orchestration
        // 1. Validate request
        // 2. Call admin-service or Cognito for authentication
        // 3. Handle MFA if required
        // 4. Return JWT tokens
        
        // Mock response for Swagger documentation
        var response = new LoginResponse
        {
            AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJzdWIiOiI1NTBlODQwMC1lMjliLTQxZDQtYTcxNi00NDY2NTU0NDAwMDAiLCJlbWFpbCI6ImRlYWxlckBleGFtcGxlLmNvbSIsInJvbGUiOiJERUFMRVIiLCJleHAiOjE3MDQ2NzIwMDB9.mock_signature",
            RefreshToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.mock_refresh_token",
            ExpiresIn = 3600,
            TokenType = "Bearer",
            UserId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            RequiresMfa = false
        };

        return Ok(response);
    }

    /// <summary>
    /// Refresh access token
    /// </summary>
    /// <remarks>
    /// Obtain a new access token using a refresh token.
    /// 
    /// **User Journey:** 01_Login (Token refresh flow)
    /// 
    /// **Sample Request:**
    /// ```json
    /// {
    ///   "refreshToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."
    /// }
    /// ```
    /// </remarks>
    /// <param name="request">[REQUIRED] Refresh token request details.</param>
    /// <response code="200">Token refreshed successfully</response>
    /// <response code="401">Invalid or expired refresh token</response>
    [HttpPost("refresh")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RefreshTokenResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<RefreshTokenResponse>> RefreshToken([FromBody] RefreshTokenRequest request)
    {
        // TODO: Implement token refresh orchestration
        // 1. Validate refresh token
        // 2. Call admin-service or Cognito to refresh
        // 3. Return new access token
        
        var response = new RefreshTokenResponse
        {
            AccessToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.new_access_token",
            ExpiresIn = 3600,
            TokenType = "Bearer"
        };

        return Ok(response);
    }

    /// <summary>
    /// User logout
    /// </summary>
    /// <remarks>
    /// Invalidate current session and tokens.
    /// 
    /// **User Journey:** 01_Login (Logout flow)
    /// 
    /// **Note:** Frontend should also clear stored tokens from secure storage.
    /// </remarks>
    /// <response code="200">Logout successful</response>
    /// <response code="401">Unauthorized</response>
    [HttpPost("logout")]
    [Authorize]
    [ProducesResponseType(typeof(LogoutResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<LogoutResponse>> Logout()
    {
        // TODO: Implement logout orchestration
        // 1. Extract user ID from JWT
        // 2. Call admin-service to invalidate session
        // 3. Optionally blacklist the token
        
        var response = new LogoutResponse
        {
            Message = "Logout successful"
        };

        return Ok(response);
    }
}
