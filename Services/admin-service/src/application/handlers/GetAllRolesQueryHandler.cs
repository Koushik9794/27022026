using AdminService.Domain.Aggregates;
using AdminService.Domain.Services;
using AdminService.Application.Queries;

namespace AdminService.Application.Handlers
{
    public sealed class GetAllRolesQueryHandler
    {
        private readonly IRoleRepository _repo;

        public GetAllRolesQueryHandler(IRoleRepository repo)
        {
            _repo = repo ?? throw new ArgumentNullException(nameof(repo));
        }

        public async Task<IEnumerable<Role>> Handle(GetAllRolesQuery query, CancellationToken ct)
        {
            var (items, _) = await _repo.ListAsync(null, null, 1, 1000, ct);
            return items;
        }
    }
}
