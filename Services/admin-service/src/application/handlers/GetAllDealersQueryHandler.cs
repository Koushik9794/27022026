using AdminService.Application.Queries;
using AdminService.Application.Dtos;
using AdminService.Infrastructure.Persistence;

namespace AdminService.Application.Handlers
{
    /// <summary>
    /// Handler for retrieving all dealers
    /// </summary>
    public sealed class GetAllDealersQueryHandler
    {
        private readonly IDealerRepository _repository;

        public GetAllDealersQueryHandler(IDealerRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Handles the query to get all dealers
        /// </summary>
        /// <param name="request">The query</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>List of dealer DTOs</returns>
        public async Task<List<DealerDto>> Handle(GetAllDealersQuery request, CancellationToken cancellationToken)
        {
            var dealers = await _repository.GetAllAsync();
            return dealers.Select(dealer => new DealerDto(
                dealer.Id,
                dealer.Code,
                dealer.Name,
                dealer.ContactName,
                dealer.ContactEmail?.Value,
                dealer.ContactPhone,
                dealer.CountryCode,
                dealer.State,
                dealer.City,
                dealer.Address,
                dealer.IsActive,
                dealer.CreatedBy,
                dealer.UpdatedBy,
                dealer.CreatedAt,
                dealer.UpdatedAt)).ToList();
        }
    }
}
