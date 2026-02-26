
using AdminService.Application.Commands;
using AdminService.Domain.Aggregates;
using AdminService.Domain.ValueObjects;
using AdminService.Infrastructure.Persistence;

namespace AdminService.Application.Handlers
{
    /// <summary>
    /// Handler for RegisterUserCommand
    /// </summary>
    public sealed class RegisterUserCommandHandler
    {
        private readonly IUserRepository _repository;

        public RegisterUserCommandHandler(IUserRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<RegisterUserResult> Handle(RegisterUserCommand request, CancellationToken cancellationToken)
        {
            // Create value objects
            var email = Email.Create(request.Email);
            var displayName = DisplayName.Create(request.DisplayName);
            var role = UserRole.Create(request.Role);

            // Check if email already exists
            var existingUser = await _repository.GetByEmailAsync(email.Value);
            if (existingUser != null)
                throw new InvalidOperationException($"User with email {email.Value} already exists");

            // Create aggregate
            var user = User.Register(email, displayName, role);

            // Persist
            await _repository.AddAsync(user);

            return new RegisterUserResult(user.Id);
        }
    }
}
