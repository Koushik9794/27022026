using AdminService.Application.Commands;
using AdminService.Domain.Aggregates;
using AdminService.Infrastructure.Persistence;
using AdminService.Domain.ValueObjects;
using FluentValidation;

namespace AdminService.Application.Handlers
{
    /// <summary>
    /// Handler for creating a new dealer
    /// </summary>
    public sealed class CreateDealerCommandHandler
    {
        private readonly IDealerRepository _repository;

        public CreateDealerCommandHandler(IDealerRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Handles the creation of a dealer
        /// </summary>
        /// <param name="request">The create command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <returns>The ID of the created dealer</returns>
        public async Task<Guid> Handle(CreateDealerCommand request, CancellationToken cancellationToken)
        {
            var email = !string.IsNullOrEmpty(request.ContactEmail) ? Email.Create(request.ContactEmail) : null;

            var dealer = Dealer.Create(
                request.Code,
                request.Name,
                request.ContactName,
                email,
                request.ContactPhone,
                request.CountryCode,
                request.State,
                request.City,
                request.Address,
                request.CreatedBy);

            await _repository.AddAsync(dealer);

            return dealer.Id;
        }
    }

    /// <summary>
    /// Handler for updating an existing dealer
    /// </summary>
    public sealed class UpdateDealerCommandHandler
    {
        private readonly IDealerRepository _repository;

        public UpdateDealerCommandHandler(IDealerRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Handles the update of a dealer
        /// </summary>
        /// <param name="request">The update command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="KeyNotFoundException">Thrown if dealer not found</exception>
        public async Task Handle(UpdateDealerCommand request, CancellationToken cancellationToken)
        {
            var dealer = await _repository.GetByIdAsync(request.Id);
            if (dealer == null)
            {
                throw new KeyNotFoundException($"Dealer with ID {request.Id} not found");
            }

            var email = !string.IsNullOrEmpty(request.ContactEmail) ? Email.Create(request.ContactEmail) : null;

            dealer.Update(
                request.Name,
                request.ContactName,
                email,
                request.ContactPhone,
                request.CountryCode,
                request.State,
                request.City,
                request.Address,
                request.IsActive,
                request.UpdatedBy);

            await _repository.UpdateAsync(dealer);
        }
    }

    /// <summary>
    /// Handler for deleting a dealer
    /// </summary>
    public sealed class DeleteDealerCommandHandler
    {
        private readonly IDealerRepository _repository;

        public DeleteDealerCommandHandler(IDealerRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        /// <summary>
        /// Handles the deletion of a dealer
        /// </summary>
        /// <param name="request">The delete command</param>
        /// <param name="cancellationToken">Cancellation token</param>
        /// <exception cref="KeyNotFoundException">Thrown if dealer not found</exception>
        public async Task Handle(DeleteDealerCommand request, CancellationToken cancellationToken)
        {
            var dealer = await _repository.GetByIdAsync(request.Id);
            if (dealer == null)
            {
                throw new KeyNotFoundException($"Dealer with ID {request.Id} not found");
            }

            dealer.Delete(request.UpdatedBy);
            await _repository.UpdateAsync(dealer);
        }
    }
}
