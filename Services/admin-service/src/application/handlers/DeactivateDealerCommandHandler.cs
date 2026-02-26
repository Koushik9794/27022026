using AdminService.Application.Commands;
using AdminService.Infrastructure.Persistence;

namespace AdminService.Application.Handlers
{
    /// <summary>
    /// Handler for deactivating a dealer
    /// </summary>
    public sealed class DeactivateDealerCommandHandler
    {
        private readonly IDealerRepository _repository;

        public DeactivateDealerCommandHandler(IDealerRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Handles the deactivation of a dealer
        /// </summary>
        /// <param name="request">The deactivate command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="KeyNotFoundException">Thrown if dealer not found</exception>
        public async Task Handle(DeactivateDealerCommand request, CancellationToken cancellationToken)
        {
            var dealer = await _repository.GetByIdAsync(request.Id);
            if (dealer == null)
            {
                throw new KeyNotFoundException($"Dealer with ID {request.Id} not found");
            }

            dealer.Deactivate(request.UpdatedBy);
            await _repository.UpdateAsync(dealer);
        }
    }
}
