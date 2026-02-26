using FluentValidation;
using AdminService.Domain.Aggregates;
using AdminService.Domain.Services;
using AdminService.Domain.ValueObjects;
using AdminService.Application.Commands;
using AdminService.Application.Dtos;
using MediatR;

namespace AdminService.Application.Handlers
{
    public sealed class CreateRoleHandler : IRequestHandler<CreateRoleCommand, CreateRoleResult>
    {
        private readonly IRoleRepository _repo;
        private readonly IValidator<CreateRoleCommand>? _validator;

        public CreateRoleHandler(IRoleRepository repo, IValidator<CreateRoleCommand>? validator = null)
        {
            _repo = repo;
            _validator = validator;
        }

        public async Task<CreateRoleResult> Handle(CreateRoleCommand command, CancellationToken ct)
        {
            if (_validator is not null)
                await _validator.ValidateAndThrowAsync(command, ct);

            // Enforce uniqueness among non-deleted roles
            var name = command.RoleName.Trim();
            var existing = await _repo.GetByNameAsync(name, ct, includeDeleted: false);
            if (existing is not null)
                throw new InvalidOperationException($"Role '{name}' already exists.");

            var role = Role.Create(name, command.Description, command.CreatedBy);

            var id = await _repo.CreateAsync(role, ct);
            return new CreateRoleResult(id);
        }
    }
}
