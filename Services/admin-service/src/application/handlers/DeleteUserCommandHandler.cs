using AdminService.Application.Commands;
using AdminService.Infrastructure.Persistence;

namespace AdminService.Application.Handlers
{
    /// <summary>
    /// Handler for DeleteUserCommand
    /// </summary>
    public sealed class DeleteUserCommandHandler 
    {
        private readonly IUserRepository _repository;

        public DeleteUserCommandHandler(IUserRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task Handle(DeleteUserCommand request, CancellationToken cancellationToken)
        {
            var user = await _repository.GetByIdAsync(request.UserId);
            if (user == null)
                throw new InvalidOperationException($"User with ID {request.UserId} not found");

            user.Deactivate(request.Reason);
            await _repository.UpdateAsync(user);
        }
    }
}
