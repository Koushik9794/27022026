using MediatR;

namespace AdminService.Application.Commands;

/// <summary>
/// Updates a permission's metadata (description, module, entity) and audit fields.
/// </summary>
public sealed record UpdatePermissionCommand(
    Guid Id,
    string? Description,
    string ModuleName,
    string? EntityName,
    string ModifiedBy
) : IRequest<bool>;
