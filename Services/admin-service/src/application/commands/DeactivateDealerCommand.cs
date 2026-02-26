using System;

namespace AdminService.Application.Commands
{
    /// <summary>
    /// Command to deactivate a dealer
    /// </summary>
    /// <param name="Id">Dealer ID</param>
    /// <param name="UpdatedBy">ID of the user deactivating the dealer</param>
    public sealed record DeactivateDealerCommand(Guid Id, Guid UpdatedBy);
}
