using MediatR;

namespace AdminService.Application.Commands;

/// <summary>
/// Soft-deletes a permission (is_deleted=true, is_active=false) with audit.
/// </summary>
public sealed record DeletePermissionCommand(
    Guid Id,
    string ModifiedBy
) : IRequest<bool>;
