using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using GssWebApi.Dto;

namespace GssWebApi.Api.Controllers;

/// <summary>
/// User context and personalization endpoints
/// </summary>
[ApiController]
[Route("api/v1/me")]
[Authorize]
[Produces("application/json")]
public class UserContextController : ControllerBase
{
    private readonly ILogger<UserContextController> _logger;

    public UserContextController(ILogger<UserContextController> logger)
    {
        _logger = logger;
    }

    /// <summary>
    /// Get current user context including permissions, dealer info, and preferences
    /// </summary>
    /// <remarks>
    /// Retrieves complete user context after authentication. This endpoint aggregates:
    /// - User profile from admin-service
    /// - Dealer information (if applicable)
    /// - Regional preferences (currency, language, measurement units)
    /// - Feature flags enabled for this user
    /// 
    /// **User Journey:** 01_Login (Post-authentication context resolution)
    /// 
    /// **Example JWT Token Structure:**
    /// ```
    /// {
    ///   "sub": "550e8400-e29b-41d4-a716-446655440000",
    ///   "email": "dealer@example.com",
    ///   "role": "DEALER",
    ///   "exp": 1704672000
    /// }
    /// ```
    /// 
    /// **Sample Request:**
    /// ```
    /// GET /api/v1/me/context
    /// Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...
    /// X-Correlation-ID: abc-123-def
    /// ```
    /// </remarks>
    /// <response code="200">Returns user context with all permissions and preferences</response>
    /// <response code="401">Unauthorized - Invalid or expired JWT token</response>
    /// <response code="500">Internal server error</response>
    /// <param name="ct">[OPTIONAL] Cancellation token.</param>
    [HttpGet("context")]
    [ProducesResponseType(typeof(UserContextResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status500InternalServerError)]
    public async Task<ActionResult<UserContextResponse>> GetUserContext([FromQuery] CancellationToken ct)
    {
        // TODO: Implement orchestration to fetch user context from admin-service
        // For now, return mock data for Swagger documentation
        
        var response = new UserContextResponse
        {
            UserId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000"),
            Email = "dealer@example.com",
            FullName = "John Dealer",
            Role = "DEALER",
            Permissions = new List<string>
            {
                "configurations.create",
                "configurations.read",
                "configurations.update",
                "bom.generate",
                "quote.generate"
            },
            Dealer = new DealerInfo
            {
                DealerId = Guid.Parse("d123e456-e89b-12d3-a456-426614174000"),
                DealerCode = "DLR-001",
                CompanyName = "ABC Warehouse Solutions",
                Territory = "NORTH_INDIA"
            },
            Preferences = new UserPreferences
            {
                Region = "ap-south-1",
                Country = "IN",
                Currency = "INR",
                Language = "en",
                MeasurementUnit = "METRIC"
            },
            FeatureFlags = new FeatureFlags
            {
                Enable3DView = true,
                EnableRealTimeValidation = true,
                EnableBulkImport = false
            }
        };

        return Ok(response);
    }

    /// <summary>
    /// Get user preferences
    /// </summary>
    /// <remarks>
    /// Retrieves user-specific preferences including regional settings and UI preferences.
    /// </remarks>
    /// <response code="200">Returns user preferences</response>
    /// <response code="401">Unauthorized</response>
    /// <param name="ct">[OPTIONAL] Cancellation token.</param>
    [HttpGet("preferences")]
    [ProducesResponseType(typeof(UserPreferences), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ErrorResponse), StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<UserPreferences>> GetUserPreferences([FromQuery] CancellationToken ct)
    {
        // TODO: Implement
        var preferences = new UserPreferences
        {
            Region = "ap-south-1",
            Country = "IN",
            Currency = "INR",
            Language = "en",
            MeasurementUnit = "METRIC"
        };

        return Ok(preferences);
    }
}
