using FluentValidation;
using AdminService.Domain.Services;
using AdminService.Application.Commands;

namespace AdminService.Application.Handlers
{
    public sealed class DeleteRoleHandler
    {
        private readonly IRoleRepository _repo;

        public DeleteRoleHandler(IRoleRepository repo)
        {
            _repo = repo;
        }

        public async Task Handle(DeleteRoleCommand command, CancellationToken ct)
        {
            var role = await _repo.GetByIdAsync(command.RoleId, ct);
            if (role is null)
                throw new KeyNotFoundException("Role not found.");

            await _repo.SoftDeleteAsync(command.RoleId, command.ModifiedBy, DateTime.UtcNow, ct);
        }
    }
}
