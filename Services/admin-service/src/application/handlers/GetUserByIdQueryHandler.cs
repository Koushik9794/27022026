using AdminService.Application.Queries;
using AdminService.Infrastructure.Persistence;

namespace AdminService.Application.Handlers
{
    /// <summary>
    /// Handler for GetUserByIdQuery
    /// </summary>
    public sealed class GetUserByIdQueryHandler
    {
        private readonly IUserRepository _repository;

        public GetUserByIdQueryHandler(IUserRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<UserDto?> Handle(GetUserByIdQuery request, CancellationToken cancellationToken)
        {
            var user = await _repository.GetByIdAsync(request.UserId);
            if (user == null)
                return null;

            return new UserDto(
                user.Id,
                user.Email.Value,
                user.DisplayName.Value,
                user.Role.Value,
                user.Status.ToString(),
                user.CreatedAt,
                user.LastLoginAt);
        }
    }
}
