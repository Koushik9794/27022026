using AdminService.Application.Commands;
using AdminService.Infrastructure.Persistence;

namespace AdminService.Application.Handlers
{
    /// <summary>
    /// Handler for activating a dealer
    /// </summary>
    public sealed class ActivateDealerCommandHandler
    {
        private readonly IDealerRepository _repository;

        public ActivateDealerCommandHandler(IDealerRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Handles the activation of a dealer
        /// </summary>
        /// <param name="request">The activate command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="KeyNotFoundException">Thrown if dealer not found</exception>
        public async Task Handle(ActivateDealerCommand request, CancellationToken cancellationToken)
        {
            var dealer = await _repository.GetByIdAsync(request.Id);
            if (dealer == null)
            {
                throw new KeyNotFoundException($"Dealer with ID {request.Id} not found");
            }

            dealer.Activate(request.UpdatedBy);
            await _repository.UpdateAsync(dealer);
        }
    }
}
