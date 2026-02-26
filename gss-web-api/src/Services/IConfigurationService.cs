using GssWebApi.Dto;

namespace GssWebApi.src.Services;

public interface IConfigurationService
{
    // Enquiry endpoints
    Task<HttpResponseMessage> GetAllEnquiriesAsync(bool includeDeleted = false);
    Task<HttpResponseMessage> GetEnquiryByIdAsync(Guid id);
    Task<HttpResponseMessage> GetEnquiryByExternalIdAsync(string externalId);
    Task<HttpResponseMessage> CreateEnquiryAsync(CreateEnquiryRequest request);
    Task<HttpResponseMessage> UpdateEnquiryAsync(Guid id, UpdateEnquiryRequest request);
    Task<HttpResponseMessage> DeleteEnquiryAsync(Guid id);

    // Configuration endpoints
    Task<HttpResponseMessage> GetConfigurationsByEnquiryIdAsync(Guid enquiryId, bool includeInactive = false);
    Task<HttpResponseMessage> GetConfigurationListAsync(Guid enquiryId, bool includeInactive = false);
    Task<HttpResponseMessage> GetConfigurationByIdAsync(Guid enquiryId, Guid configId);
    Task<HttpResponseMessage> CreateConfigurationAsync(Guid enquiryId, EnquiryCreateConfigurationRequest request);
    Task<HttpResponseMessage> DeleteConfigurationAsync(Guid enquiryId, Guid configId);

    // Version endpoints
    Task<HttpResponseMessage> CreateVersionAsync(Guid enquiryId, Guid configId, CreateVersionRequest request);
    Task<HttpResponseMessage> LockVersionAsync(Guid enquiryId, Guid configId, int versionNumber, CreateVersionRequest request);
    Task<HttpResponseMessage> UnlockVersionAsync(Guid enquiryId, Guid configId, int versionNumber, CreateVersionRequest request);

    // Layout endpoints
    Task<HttpResponseMessage> GetCivilLayoutListAsync(Guid enquiryId, Guid configId);
    Task<HttpResponseMessage> GetCivilLayoutByIdAsync(Guid id);
    Task<HttpResponseMessage> UpdateCivilLayoutAsync(Guid id, UpdateCivilLayoutRequest request);
    Task<HttpResponseMessage> CreateCivilLayoutAsync(Guid enquiryId, Guid configId, SaveCivilLayoutRequest request);
    Task<HttpResponseMessage> GetRackLayoutAsync(Guid enquiryId, Guid configId, int versionNumber);
    Task<HttpResponseMessage> UpdateRackLayoutAsync(Guid id, SaveRackLayoutRequest request);
    Task<HttpResponseMessage> CreateRackLayoutAsync(Guid configId, int civilVersion, int configVersion, SaveRackLayoutRequest request);

    // Storage Configuration endpoints
    Task<HttpResponseMessage> SaveDesignAsync(Guid id, SaveDesignRequest request);
    Task<HttpResponseMessage> CreateStorageConfigurationAsync(CreateStorageConfigurationRequest request);

    // Rack Configuration endpoints
    Task<HttpResponseMessage> GetAllRackConfigurationsAsync(bool includeInactive = false);
    Task<HttpResponseMessage> GetRackConfigurationByIdAsync(Guid id);
    Task<HttpResponseMessage> CreateRackConfigurationAsync(CreateRackConfigurationRequest request);
    Task<HttpResponseMessage> UpdateRackConfigurationAsync(Guid id, UpdateRackConfigurationRequest request);
    Task<HttpResponseMessage> DeleteRackConfigurationAsync(Guid id);
}
