
using AdminService.Application.Commands;
using AdminService.Infrastructure.Persistence;

namespace AdminService.Application.Handlers
{
    /// <summary>
    /// Handler for ActivateUserCommand
    /// </summary>
    public sealed class ActivateUserCommandHandler
    {
        private readonly IUserRepository _repository;

        public ActivateUserCommandHandler(IUserRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task Handle(ActivateUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _repository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new InvalidOperationException($"User with ID {request.UserId} not found");

            user.Activate();
            await _repository.UpdateAsync(user);
        }
    }
}
