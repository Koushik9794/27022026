using AdminService.Application.Queries;
using AdminService.Infrastructure.Persistence;

namespace AdminService.Application.Handlers
{
    /// <summary>
    /// Handler for GetAllUsersQuery
    /// </summary>
    public sealed class GetAllUsersQueryHandler
    {
        private readonly IUserRepository _repository;

        public GetAllUsersQueryHandler(IUserRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<List<UserDto>> Handle(GetAllUsersQuery request, CancellationToken cancellationToken)
        {
            var users = await _repository.GetAllAsync();

            return users.Select(user => new UserDto(
                user.Id,
                user.Email.Value,
                user.DisplayName.Value,
                user.Role.Value,
                user.Status.ToString(),
                user.CreatedAt,
                user.LastLoginAt)).ToList();
        }
    }
}
