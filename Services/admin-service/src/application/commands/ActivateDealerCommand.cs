using System;

namespace AdminService.Application.Commands
{
    /// <summary>
    /// Command to activate a dealer
    /// </summary>
    /// <param name="Id">Dealer ID</param>
    /// <param name="UpdatedBy">ID of the user activating the dealer</param>
    public sealed record ActivateDealerCommand(Guid Id, Guid UpdatedBy);
}
