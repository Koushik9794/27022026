namespace AdminService.Application.Queries
{
    /// <summary>
    /// Query to retrieve all dealers
    /// </summary>
    public sealed record GetAllDealersQuery();

    /// <summary>
    /// Query to retrieve a specific dealer by ID
    /// </summary>
    /// <param name="Id">Dealer ID</param>
    public sealed record GetDealerByIdQuery(Guid Id);
}
