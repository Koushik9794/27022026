// src/application/handlers/CreatePermissionHandler.cs
using System;
using System.Threading;
using System.Threading.Tasks;
using AdminService.Application.Commands;
using AdminService.Domain.Aggregates;
using AdminService.Domain.ValueObjects;
using MediatR;
using AdminService.Infrastructure.Persistence;
using FluentValidation;
using AdminService.Domain.Exceptions;

namespace AdminService.Application.Handlers
{
    public sealed class CreatePermissionHandler : IRequestHandler<CreatePermissionCommand, Guid>
    {
        private readonly IPermissionRepository _repo;
        private readonly IValidator<CreatePermissionCommand>? _validator;
 
        public CreatePermissionHandler(IPermissionRepository repo, IValidator<CreatePermissionCommand>? validator = null)
        {
            _repo = repo;
            _validator = validator;
        }

        // Wolverine discovers this 'Handle' method by convention and returns Guid.
        public async Task<Guid> Handle(CreatePermissionCommand cmd, CancellationToken ct)
        {
            if (_validator != null)
                await _validator.ValidateAndThrowAsync(cmd, ct);
            if (string.IsNullOrWhiteSpace(cmd.PermissionName))
                throw new DomainException("PermissionName is required.");
            if (string.IsNullOrWhiteSpace(cmd.ModuleName))
                throw new DomainException("ModuleName is required.");
            if (string.IsNullOrWhiteSpace(cmd.CreatedBy))
                throw new DomainException("CreatedBy is required.");

            var permission = Permission.Rehydrate(
                id: Guid.NewGuid(),
                name: PermissionName.Create(cmd.PermissionName),
                description: cmd.Description,
                moduleName: ModuleName.Create(cmd.ModuleName),
                entityName: cmd.EntityName is null ? null : EntityName.Create(cmd.EntityName),
                isActive: true,
                isDeleted: false,
                createdBy: cmd.CreatedBy,
                createdAt: DateTime.UtcNow,
                modifiedBy: null,
                modifiedAt: null
            );

            return await _repo.CreateAsync(permission, ct);
        }
    }
}
