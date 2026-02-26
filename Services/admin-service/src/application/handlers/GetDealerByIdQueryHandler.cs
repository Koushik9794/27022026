using AdminService.Application.Queries;
using AdminService.Application.Dtos;
using AdminService.Infrastructure.Persistence;

namespace AdminService.Application.Handlers
{
    /// <summary>
    /// Handler for retrieving a dealer by ID
    /// </summary>
    public sealed class GetDealerByIdQueryHandler
    {
        private readonly IDealerRepository _repository;

        public GetDealerByIdQueryHandler(IDealerRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Handles the query to get a dealer by ID
        /// </summary>
        /// <param name="request">The query containing the dealer ID</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The dealer DTO if found, otherwise null</returns>
        public async Task<DealerDto?> Handle(GetDealerByIdQuery request, CancellationToken cancellationToken)
        {
            var dealer = await _repository.GetByIdAsync(request.Id);
            if (dealer == null) return null;

            return new DealerDto(
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
                dealer.UpdatedAt);
        }
    }
}
