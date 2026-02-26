using FluentValidation;

namespace AdminService.Application.Commands
{
    /// <summary>
    /// Command to create a new dealer
    /// </summary>
    /// <param name="Code">Unique dealer code</param>
    /// <param name="Name">Dealer name</param>
    /// <param name="ContactName">Primary contact person name</param>
    /// <param name="ContactEmail">Contact email address</param>
    /// <param name="ContactPhone">Contact phone number</param>
    /// <param name="CountryCode">Country code (ISO 3166-1 alpha-2)</param>
    /// <param name="State">State or region</param>
    /// <param name="City">City name</param>
    /// <param name="Address">Street address</param>
    /// <param name="CreatedBy">ID of the user creating the dealer</param>
    public sealed record CreateDealerCommand(
        string Code,
        string Name,
        string? ContactName,
        string? ContactEmail,
        string? ContactPhone,
        string? CountryCode,
        string? State,
        string? City,
        string? Address,
        Guid CreatedBy);

    /// <summary>
    /// Command to update an existing dealer
    /// </summary>
    /// <param name="Id">Dealer ID</param>
    /// <param name="Name">Dealer name</param>
    /// <param name="ContactName">Primary contact person name</param>
    /// <param name="ContactEmail">Contact email address</param>
    /// <param name="ContactPhone">Contact phone number</param>
    /// <param name="CountryCode">Country code</param>
    /// <param name="State">State or region</param>
    /// <param name="City">City name</param>
    /// <param name="Address">Street address</param>
    /// <param name="IsActive">Whether the dealer is active</param>
    /// <param name="UpdatedBy">ID of the user updating the dealer</param>
    public sealed record UpdateDealerCommand(
        Guid Id,
        string Name,
        string? ContactName,
        string? ContactEmail,
        string? ContactPhone,
        string? CountryCode,
        string? State,
        string? City,
        string? Address,
        bool IsActive,
        Guid UpdatedBy);

    /// <summary>
    /// Command to soft delete a dealer
    /// </summary>
    /// <param name="Id">Dealer ID</param>
    /// <param name="UpdatedBy">ID of the user deleting the dealer</param>
    public sealed record DeleteDealerCommand(
        Guid Id,
        Guid UpdatedBy);
}
