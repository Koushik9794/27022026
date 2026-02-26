// src/application/handlers/GetAllPermissionsQueryHandler.cs
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using AdminService.Infrastructure.Persistence;   // IPermissionRepository (per your chosen path)
using AdminService.Domain.Aggregates;
using AdminService.Application.Queries;
using MediatR;

namespace AdminService.Application.Handlers
{
    public sealed class GetAllPermissionsQueryHandler : IRequestHandler<GetAllPermissionsQuery, IEnumerable<Permission>>
    {
        private readonly IPermissionRepository _repo;

        public GetAllPermissionsQueryHandler(IPermissionRepository repo)
        {
            _repo = repo;
        }

        // Wolverine discovers this by convention and returns the list to InvokeAsync<IEnumerable<Permission>>(...)
        public Task<IEnumerable<Permission>> Handle(GetAllPermissionsQuery query, CancellationToken ct)
        {
            return _repo.GetAllAsync(query.ModuleName, query.EntityName, ct);
        }
    }
}
