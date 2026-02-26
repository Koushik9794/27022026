
using AdminService.Application.Commands;
using AdminService.Domain.ValueObjects;
using AdminService.Infrastructure.Persistence;

namespace AdminService.Application.Handlers
{
    /// <summary>
    /// Handler for UpdateUserCommand
    /// </summary>
    public sealed class UpdateUserCommandHandler
    {
        private readonly IUserRepository _repository;

        public UpdateUserCommandHandler(IUserRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task Handle(UpdateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _repository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new InvalidOperationException($"User with ID {request.UserId} not found");

            var newDisplayName = DisplayName.Create(request.DisplayName);
            user.UpdateProfile(newDisplayName);

            await _repository.UpdateAsync(user);
        }
    }
}
