using System.Security.Claims;

namespace GssCommon.Auth;

/// <summary>
/// Provides access to the current authenticated user's information.
/// </summary>
public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Email { get; }
    string? DisplayName { get; }
    string? TenantId { get; }
    IReadOnlyCollection<string> Roles { get; }
    IReadOnlyCollection<Claim> Claims { get; }

    bool IsInRole(string role);
    bool HasPermission(string permission);
}
