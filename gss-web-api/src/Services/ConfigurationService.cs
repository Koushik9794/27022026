using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using GssWebApi.Dto;
using GssWebApi.Services;

namespace GssWebApi.src.Services;

public class ConfigurationService : IConfigurationService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<ConfigurationService> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public ConfigurationService(HttpClient httpClient, ILogger<ConfigurationService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    #region Enquiries

    public async Task<HttpResponseMessage> GetAllEnquiriesAsync(bool includeDeleted = false)
    {
        _logger.LogDebug("Getting all enquiries (includeDeleted: {IncludeDeleted})", includeDeleted);
        var url = includeDeleted ? "/api/v1/enquiries?includeDeleted=true" : "/api/v1/enquiries";
        return await _httpClient.GetAsync(url);
    }

    public async Task<HttpResponseMessage> GetEnquiryByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting enquiry by ID: {Id}", id);
        return await _httpClient.GetAsync($"/api/v1/enquiries/{id}");
    }

    public async Task<HttpResponseMessage> GetEnquiryByExternalIdAsync(string externalId)
    {
        _logger.LogDebug("Getting enquiry by external ID: {ExternalId}", externalId);
        return await _httpClient.GetAsync($"/api/v1/enquiries/external/{externalId}");
    }

    public async Task<HttpResponseMessage> CreateEnquiryAsync(CreateEnquiryRequest request)
    {
        _logger.LogDebug("Creating enquiry");
        return await PostJsonAsync("/api/v1/enquiries", request);
    }

    public async Task<HttpResponseMessage> UpdateEnquiryAsync(Guid id, UpdateEnquiryRequest request)
    {
        _logger.LogDebug("Updating enquiry: {Id}", id);
        return await PutJsonAsync($"/api/v1/enquiries/{id}", request);
    }

    public async Task<HttpResponseMessage> DeleteEnquiryAsync(Guid id)
    {
        _logger.LogDebug("Deleting enquiry: {Id}", id);
        return await _httpClient.DeleteAsync($"/api/v1/enquiries/{id}");
    }

    #endregion

    #region Configurations

    public async Task<HttpResponseMessage> GetConfigurationsByEnquiryIdAsync(Guid enquiryId, bool includeInactive = false)
    {
        _logger.LogDebug("Getting configurations for enquiry: {EnquiryId}", enquiryId);
        var url = includeInactive
            ? $"/api/v1/enquiries/{enquiryId}/configurations?includeInactive=true"
            : $"/api/v1/enquiries/{enquiryId}/configurations";
        return await _httpClient.GetAsync(url);
    }

    public async Task<HttpResponseMessage> GetConfigurationListAsync(Guid enquiryId, bool includeInactive = false)
    {
        _logger.LogDebug("Getting configuration list for enquiry: {EnquiryId}", enquiryId);
        var url = includeInactive
            ? $"/api/v1/enquiries/{enquiryId}/configurationList?includeInactive=true"
            : $"/api/v1/enquiries/{enquiryId}/configurationList";
        return await _httpClient.GetAsync(url);
    }

    public async Task<HttpResponseMessage> GetConfigurationByIdAsync(Guid enquiryId, Guid configId)
    {
        _logger.LogDebug("Getting configuration: {ConfigId} for enquiry: {EnquiryId}", configId, enquiryId);
        return await _httpClient.GetAsync($"/api/v1/enquiries/{enquiryId}/configurations/{configId}");
    }

    public async Task<HttpResponseMessage> CreateConfigurationAsync(Guid enquiryId, EnquiryCreateConfigurationRequest request)
    {
        _logger.LogDebug("Creating configuration for enquiry: {EnquiryId}", enquiryId);
        return await PostJsonAsync($"/api/v1/enquiries/{enquiryId}/configurations", request);
    }

    public async Task<HttpResponseMessage> DeleteConfigurationAsync(Guid enquiryId, Guid configId)
    {
        _logger.LogDebug("Deleting configuration: {ConfigId} for enquiry: {EnquiryId}", configId, enquiryId);
        return await _httpClient.DeleteAsync($"/api/v1/enquiries/{enquiryId}/configurations/{configId}");
    }

    #endregion

    #region Versions

    public async Task<HttpResponseMessage> CreateVersionAsync(Guid enquiryId, Guid configId, CreateVersionRequest request)
    {
        _logger.LogDebug("Creating version for configuration: {ConfigId}", configId);
        return await PostJsonAsync($"/api/v1/enquiries/{enquiryId}/configurations/{configId}/versions", request);
    }

    public async Task<HttpResponseMessage> LockVersionAsync(Guid enquiryId, Guid configId, int versionNumber, CreateVersionRequest request)
    {
        _logger.LogDebug("Locking version: {VersionNumber} for configuration: {ConfigId}", versionNumber, configId);
        return await PutJsonAsync($"/api/v1/enquiries/{enquiryId}/configurations/{configId}/version-lock/{versionNumber}", request);
    }

    public async Task<HttpResponseMessage> UnlockVersionAsync(Guid enquiryId, Guid configId, int versionNumber, CreateVersionRequest request)
    {
        _logger.LogDebug("Unlocking version: {VersionNumber} for configuration: {ConfigId}", versionNumber, configId);
        return await PutJsonAsync($"/api/v1/enquiries/{enquiryId}/configurations/{configId}/un-lock-version/{versionNumber}", request);
    }

    #endregion

    #region Layouts

    public async Task<HttpResponseMessage> GetCivilLayoutListAsync(Guid enquiryId, Guid configId)
    {
        _logger.LogDebug("Getting civil layout list for configuration: {ConfigId}", configId);
        return await _httpClient.GetAsync($"/api/v1/enquiries/{enquiryId}/configurations/{configId}/civil-layout");
    }

    public async Task<HttpResponseMessage> GetCivilLayoutByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting civil layout by ID: {Id}", id);
        return await _httpClient.GetAsync($"/api/v1/configurations/civil-layout/{id}");
    }

    public async Task<HttpResponseMessage> UpdateCivilLayoutAsync(Guid id, UpdateCivilLayoutRequest request)
    {
        _logger.LogDebug("Updating civil layout: {Id}", id);
        var content = new MultipartFormDataContent();
        if (request.WarehouseType.HasValue)
            content.Add(new StringContent(request.WarehouseType.ToString()!), "WarehouseType");
        
        AddFileContent(content, request.SourceFile, "SourceFile");
        AddFileContent(content, request.CivilJson, "CivilJson");

        return await _httpClient.PutAsync($"/api/v1/configurations/civil-layout/{id}", content);
    }

    public async Task<HttpResponseMessage> CreateCivilLayoutAsync(Guid enquiryId, Guid configId, SaveCivilLayoutRequest request)
    {
        _logger.LogDebug("Creating civil layout for configuration: {ConfigId}", configId);
        var content = new MultipartFormDataContent();
        if (request.WarehouseType.HasValue)
            content.Add(new StringContent(request.WarehouseType.ToString()!), "WarehouseType");
        
        AddFileContent(content, request.SourceFile, "SourceFile");
        AddFileContent(content, request.CivilJson, "CivilJson");

        return await _httpClient.PostAsync($"/api/v1/enquiries/{enquiryId}/configurations/{configId}/civil-layout", content);
    }

    public async Task<HttpResponseMessage> GetRackLayoutAsync(Guid enquiryId, Guid configId, int versionNumber)
    {
        _logger.LogDebug("Getting rack layout for configuration: {ConfigId} version: {VersionNumber}", configId, versionNumber);
        return await _httpClient.GetAsync($"/api/v1/enquiries/{enquiryId}/configurations/{configId}/versions/{versionNumber}/rack-layout");
    }

    public async Task<HttpResponseMessage> UpdateRackLayoutAsync(Guid id, SaveRackLayoutRequest request)
    {
        _logger.LogDebug("Updating rack layout: {Id}", id);
        var content = new MultipartFormDataContent();
        AddFileContent(content, request.RackJson, "RackJson");

        return await _httpClient.PutAsync($"/api/v1/configurations/rack-layout/{id}", content);
    }

    public async Task<HttpResponseMessage> CreateRackLayoutAsync(Guid configId, int civilVersion, int configVersion, SaveRackLayoutRequest request)
    {
        _logger.LogDebug("Creating rack layout for configuration: {ConfigId}", configId);

        var url =
            $"/api/v1/enquiries/configurations/{configId}/civil-versions/{civilVersion}/config-versions/{configVersion}/rack-layout";
        var hasFile = request?.RackJson is not null && request.RackJson.Length > 0;
        var hasConfigJson = !string.IsNullOrWhiteSpace(request?.configurationjson);
        var content = new MultipartFormDataContent();

        if (!hasFile && !hasConfigJson)
        {
            // Send a POST without a body (works if your API allows empty body)
            var msg = new HttpRequestMessage(HttpMethod.Post, url);
            return await _httpClient.SendAsync(msg);
        }

        using var contentvalue = new MultipartFormDataContent();
        if (hasFile)
        {
            AddFileContent(contentvalue, request.RackJson!, "RackJson");
        }
        if (hasConfigJson)
        {
            // If it's JSON, set correct content type
            var jsonPart = new StringContent(request.configurationjson!, Encoding.UTF8, "application/json");
            contentvalue.Add(jsonPart, "configurationjson");
        }

        return await _httpClient.PostAsync(url, contentvalue);

    }

    #endregion

    #region Storage Configurations

    public async Task<HttpResponseMessage> SaveDesignAsync(Guid id, SaveDesignRequest request)
    {
        _logger.LogDebug("Saving design for storage configuration: {Id}", id);
        return await PutJsonAsync($"/api/v1/storage-configurations/{id}/design", request);
    }

    public async Task<HttpResponseMessage> CreateStorageConfigurationAsync(CreateStorageConfigurationRequest request)
    {
        _logger.LogDebug("Creating storage configuration");
        return await PostJsonAsync("/api/v1/storage-configurations", request);
    }

    #endregion
    
    #region Rack Configurations

    public async Task<HttpResponseMessage> GetAllRackConfigurationsAsync(bool includeInactive = false)
    {
        _logger.LogDebug("Getting all rack configurations (includeInactive: {IncludeInactive})", includeInactive);
        var url = includeInactive ? "/api/v1/rack-configurations?includeInactive=true" : "/api/v1/rack-configurations";
        return await _httpClient.GetAsync(url);
    }

    public async Task<HttpResponseMessage> GetRackConfigurationByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting rack configuration by ID: {Id}", id);
        return await _httpClient.GetAsync($"/api/v1/rack-configurations/{id}");
    }

    public async Task<HttpResponseMessage> CreateRackConfigurationAsync(CreateRackConfigurationRequest request)
    {
        _logger.LogDebug("Creating rack configuration");
        return await PostJsonAsync("/api/v1/rack-configurations", request);
    }

    public async Task<HttpResponseMessage> UpdateRackConfigurationAsync(Guid id, UpdateRackConfigurationRequest request)
    {
        _logger.LogDebug("Updating rack configuration: {Id}", id);
        return await PutJsonAsync($"/api/v1/rack-configurations/{id}", request);
    }

    public async Task<HttpResponseMessage> DeleteRackConfigurationAsync(Guid id)
    {
        _logger.LogDebug("Deleting rack configuration: {Id}", id);
        return await _httpClient.DeleteAsync($"/api/v1/rack-configurations/{id}");
    }

    #endregion

    #region Helpers

    private async Task<HttpResponseMessage> PostJsonAsync(string url, object request)
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _httpClient.PostAsync(url, content);
    }

    private async Task<HttpResponseMessage> PutJsonAsync(string url, object request)
    {
        var json = JsonSerializer.Serialize(request, JsonOptions);
        var content = new StringContent(json, Encoding.UTF8, "application/json");
        return await _httpClient.PutAsync(url, content);
    }

    private void AddFileContent(MultipartFormDataContent content, IFormFile? file, string name)
    {
        if (file != null)
        {
            var streamContent = new StreamContent(file.OpenReadStream());
            streamContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
            content.Add(streamContent, name, file.FileName);
        }
    }

    #endregion
}
