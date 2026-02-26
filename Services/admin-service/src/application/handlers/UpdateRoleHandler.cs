using FluentValidation;
using AdminService.Domain.Services;
using AdminService.Application.Commands;

namespace AdminService.Application.Handlers
{
    public sealed class UpdateRoleHandler
    {
        private readonly IRoleRepository _repo;
        private readonly IValidator<UpdateRoleCommand>? _validator;

        public UpdateRoleHandler(IRoleRepository repo, IValidator<UpdateRoleCommand>? validator = null)
        {
            _repo = repo;
            _validator = validator;
        }

        public async Task Handle(UpdateRoleCommand command, CancellationToken ct)
        {
            if (_validator is not null)
                await _validator.ValidateAndThrowAsync(command, ct);

            var role = await _repo.GetByIdAsync(command.RoleId, ct);
            if (role is null)
                throw new KeyNotFoundException("Role not found.");

            // Check uniqueness if name changed
            if (!string.Equals(role.RoleName, command.RoleName.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                var existing = await _repo.GetByNameAsync(command.RoleName.Trim(), ct, includeDeleted: false);
                if (existing is not null)
                    throw new InvalidOperationException($"Role '{command.RoleName}' already exists.");
            }

            role.Update(command.RoleName, command.Description, command.ModifiedBy);
            await _repo.UpdateAsync(role, ct);
        }
    }
}
