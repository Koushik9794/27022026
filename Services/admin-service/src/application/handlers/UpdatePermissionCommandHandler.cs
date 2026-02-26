using AdminService.Application.Commands;
using AdminService.Domain.Aggregates;
using AdminService.Domain.ValueObjects;
using AdminService.Infrastructure.Persistence;
using MediatR;
using FluentValidation;

namespace AdminService.Application.Handlers;

public sealed class UpdatePermissionCommandHandler : IRequestHandler<UpdatePermissionCommand, bool>
{
    private readonly IPermissionRepository _permissions;
    private readonly IValidator<UpdatePermissionCommand>? _validator;

    public UpdatePermissionCommandHandler(IPermissionRepository permissions, IValidator<UpdatePermissionCommand>? validator = null)
    {
        _permissions = permissions;
        _validator = validator;
    }

    // Request/Reply: returns bool indicating success
    public async Task<bool> Handle(UpdatePermissionCommand cmd, CancellationToken ct)
    {
        if (_validator != null)
            await _validator.ValidateAndThrowAsync(cmd, ct);
        var existing = await _permissions.GetByIdAsync(cmd.Id, ct);
        if (existing is null) return false;

        existing.Update(
            cmd.Description,
            ModuleName.Create(cmd.ModuleName),
            string.IsNullOrWhiteSpace(cmd.EntityName) ? null : EntityName.Create(cmd.EntityName),
            cmd.ModifiedBy,
            DateTime.UtcNow
        );

        await _permissions.UpdateAsync(existing, ct);
        return true;
    }
}
