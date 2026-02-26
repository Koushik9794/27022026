// src/application/commands/permissions/CreatePermissionCommand.cs
using MediatR;

namespace AdminService.Application.Commands;

public sealed record CreatePermissionCommand(
    string PermissionName,
    string? Description,
    string ModuleName,
    string? EntityName,
    string CreatedBy
) : IRequest<Guid>;
