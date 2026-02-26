using MediatR;

namespace AdminService.Application.Commands;

/// <summary>
/// Activates or deactivates a permission with audit.
/// </summary>
public sealed record ActivatePermissionCommand(
    Guid Id,
    bool Activate,
    string ModifiedBy
) : IRequest<bool>;
