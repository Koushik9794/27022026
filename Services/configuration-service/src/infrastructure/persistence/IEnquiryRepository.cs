using ConfigurationService.Domain.Aggregates;

namespace ConfigurationService.Infrastructure.Persistence;

/// <summary>
/// Repository interface for Enquiry aggregate.
/// </summary>
public interface IEnquiryRepository
{
    Task<Enquiry?> GetByIdAsync(Guid id);
    Task<Enquiry?> GetByExternalIdAsync(string externalEnquiryId);
    Task<IEnumerable<Enquiry>> GetAllAsync(bool includeDeleted = false);
    Task<Enquiry> CreateAsync(Enquiry enquiry);
    Task<Enquiry> UpdateAsync(Enquiry enquiry);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task<bool> ExternalIdExistsAsync(string externalEnquiryId);

    Task<bool> ExistsEnquiryNoAsync(string? EnquiryNo);
}
