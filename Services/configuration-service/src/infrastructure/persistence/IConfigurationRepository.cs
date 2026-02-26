using ConfigurationService.Application.Dtos;
using ConfigurationService.Domain.Aggregates;

namespace ConfigurationService.Infrastructure.Persistence;

/// <summary>
/// Repository interface for Configuration aggregate.
/// </summary>
public interface IConfigurationRepository
{
    Task<Configuration?> GetByIdAsync(Guid id);
    Task<IEnumerable<Configuration>> GetByEnquiryIdAsync(Guid enquiryId, bool includeInactive = false);
    Task<EnquiryDto?> GetListByEnquiryIdAsync(Guid enquiryId, bool includeInactive = false);
    Task<Configuration?> GetPrimaryByEnquiryIdAsync(Guid enquiryId);
    Task<Configuration> CreateAsync(Configuration configuration);
    Task<Configuration> UpdateAsync(Configuration configuration);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(Guid id);
    Task ClearPrimaryForEnquiryAsync(Guid enquiryId);

    Task<Guid?> SetversionLockAsync(Guid ConfigId, int version,bool isLocked);

    // Storage Configuration methods
    Task<StorageConfiguration?> GetStorageConfigurationByIdAsync(Guid id);
    Task UpdateStorageConfigurationAsync(StorageConfiguration storageConfig);
    Task AddStorageConfigurationAsync(StorageConfiguration storageConfig);
    
    // MHE Configuration methods
    Task<MheConfig?> GetMheConfigByIdAsync(Guid id);
    Task UpdateMheConfigAsync(MheConfig mheConfig);
    Task AddMheConfigAsync(MheConfig mheConfig);
}
