using ConfigurationService.Domain.Aggregates;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ConfigurationService.Infrastructure.Persistence;

public interface IRackConfigurationRepository
{
    Task AddAsync(RackConfiguration rackConfiguration);
    Task<RackConfiguration?> GetByIdAsync(Guid id);
    Task UpdateAsync(RackConfiguration rackConfiguration);
    Task DeleteAsync(Guid id, string? updatedBy);
    Task<IEnumerable<RackConfiguration>> GetAllAsync(bool includeInactive = false);
    Task<IEnumerable<RackConfiguration>> GetByEnquiryIdAsync(Guid enquiryId, bool includeInactive = false);
}
