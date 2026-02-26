using AdminService.Application.Commands;
using AdminService.Domain.Services;
using MediatR;

namespace AdminService.Application.Handlers
{
    public sealed class ActivateRoleCommandHandler : IRequestHandler<ActivateRoleCommand>
    {
        private readonly IRoleRepository _repo;

        public ActivateRoleCommandHandler(IRoleRepository repo)
        {
            _repo = repo;
        }

        public async Task Handle(ActivateRoleCommand command, CancellationToken ct)
        {
            var role = await _repo.GetByIdAsync(command.RoleId, ct);
            if (role is null)
                throw new KeyNotFoundException("Role not found.");

            if (command.Activate)
            {
                role.Activate(command.ModifiedBy);
            }
            else
            {
                role.Deactivate(command.ModifiedBy);
            }

            await _repo.UpdateAsync(role, ct);
        }
    }
}
