using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using System.Reflection;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.Collections.Generic;

namespace GssWebApi.Services;

/// <summary>
/// HTTP client implementation for communicating with the Catalog Service.
/// </summary>
public class CatalogServiceClient : ICatalogServiceClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<CatalogServiceClient> _logger;
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public CatalogServiceClient(HttpClient httpClient, ILogger<CatalogServiceClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;

    }

    #region SKU Types

    public async Task<HttpResponseMessage> GetSkuTypesAsync()
    {
        _logger.LogDebug("Getting all SKU types");
        return await _httpClient.GetAsync("/api/v1/Sku");
    }

    public async Task<HttpResponseMessage> GetSkuTypeByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting SKU type by ID: {Id}", id);
        return await _httpClient.GetAsync($"/api/v1/Sku/{id}");
    }

    public async Task<HttpResponseMessage> CreateSkuTypeAsync(object request)
    {
        _logger.LogDebug("Creating SKU type");
        return await PostMultipartAsync("/api/v1/Sku", request);
    }

    public async Task<HttpResponseMessage> UpdateSkuTypeAsync(Guid id, object request)
    {
        _logger.LogDebug("Updating SKU type: {Id}", id);
        return await PutMultipartAsync($"/api/v1/Sku/{id}", request);
    }

    public async Task<HttpResponseMessage> DeleteSkuTypeAsync(Guid id)
    {
        _logger.LogDebug("Deleting SKU type: {Id}", id);
        return await _httpClient.DeleteAsync($"/api/v1/Sku/{id}");
    }

    #endregion

    #region Pallet Types

    public async Task<HttpResponseMessage> GetPalletTypesAsync(bool includeInactive = false)
    {
        _logger.LogDebug("Getting all pallet types (includeInactive: {IncludeInactive})", includeInactive);
        var url = includeInactive ? "/api/v1/pallet-types?includeInactive=true" : "/api/v1/pallet-types";
        return await _httpClient.GetAsync(url);
    }

    public async Task<HttpResponseMessage> GetPalletTypeByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting pallet type by ID: {Id}", id);
        return await _httpClient.GetAsync($"/api/v1/pallet-types/{id}");
    }

    public async Task<HttpResponseMessage> GetPalletTypeByCodeAsync(string code)
    {
        _logger.LogDebug("Getting pallet type by code: {Code}", code);
        return await _httpClient.GetAsync($"/api/v1/pallet-types/code/{code}");
    }

    public async Task<HttpResponseMessage> CreatePalletTypeAsync(object request)
    {
        _logger.LogDebug("Creating pallet type");
        return await PostMultipartAsync("/api/v1/pallet-types", request);
    }

    public async Task<HttpResponseMessage> UpdatePalletTypeAsync(Guid id, object request)
    {
        _logger.LogDebug("Updating pallet type: {Id}", id);
        return await PutMultipartAsync($"/api/v1/pallet-types/{id}", request);
    }

    public async Task<HttpResponseMessage> DeletePalletTypeAsync(Guid id)
    {
        _logger.LogDebug("Deleting pallet type: {Id}", id);
        return await _httpClient.DeleteAsync($"/api/v1/pallet-types/{id}");
    }

    #endregion

    #region Taxonomy - Component Groups

    public async Task<HttpResponseMessage> GetComponentGroupsAsync(bool includeInactive = false)
    {
        _logger.LogDebug("Getting all component groups (includeInactive: {IncludeInactive})", includeInactive);
        var url = includeInactive ? "/api/v1/taxonomy/groups?includeInactive=true" : "/api/v1/taxonomy/groups";
        return await _httpClient.GetAsync(url);
    }

    public async Task<HttpResponseMessage> GetComponentGroupByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting component group by ID: {Id}", id);
        return await _httpClient.GetAsync($"/api/v1/taxonomy/groups/{id}");
    }

    public async Task<HttpResponseMessage> GetComponentGroupByCodeAsync(string code)
    {
        _logger.LogDebug("Getting component group by code: {Code}", code);
        return await _httpClient.GetAsync($"/api/v1/taxonomy/groups/code/{code}");
    }

    public async Task<HttpResponseMessage> CreateComponentGroupAsync(object request)
    {
        _logger.LogDebug("Creating component group");
        return await PostJsonAsync("/api/v1/taxonomy/groups", request);
    }

    public async Task<HttpResponseMessage> UpdateComponentGroupAsync(Guid id, object request)
    {
        _logger.LogDebug("Updating component group: {Id}", id);
        return await PutJsonAsync($"/api/v1/taxonomy/groups/{id}", request);
    }

    public async Task<HttpResponseMessage> DeleteComponentGroupAsync(Guid id)
    {
        _logger.LogDebug("Deleting component group: {Id}", id);
        return await _httpClient.DeleteAsync($"/api/v1/taxonomy/groups/{id}");
    }

    #endregion

    #region Taxonomy - Component Types

    public async Task<HttpResponseMessage> GetComponentTypesAsync(string? componentGroupCode = null, bool includeInactive = false)
    {
        _logger.LogDebug("Getting all component types (componentGroupCode: {GroupCode}, includeInactive: {IncludeInactive})", componentGroupCode, includeInactive);
        var query = new List<string>();
        if (!string.IsNullOrEmpty(componentGroupCode)) query.Add($"componentGroupCode={componentGroupCode}");
        if (includeInactive) query.Add("includeInactive=true");

        var queryString = query.Count > 0 ? "?" + string.Join("&", query) : "";
        return await _httpClient.GetAsync($"/api/v1/taxonomy/types{queryString}");
    }

    public async Task<HttpResponseMessage> GetComponentTypeByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting component type by ID: {Id}", id);
        return await _httpClient.GetAsync($"/api/v1/taxonomy/types/{id}");
    }

    public async Task<HttpResponseMessage> GetComponentTypeByCodeAsync(string code)
    {
        _logger.LogDebug("Getting component type by code: {Code}", code);
        return await _httpClient.GetAsync($"/api/v1/taxonomy/types/code/{code}");
    }

    public async Task<HttpResponseMessage> CreateComponentTypeAsync(object request)
    {
        _logger.LogDebug("Creating component type");
        return await PostJsonAsync("/api/v1/taxonomy/types", request);
    }

    public async Task<HttpResponseMessage> UpdateComponentTypeAsync(Guid id, object request)
    {
        _logger.LogDebug("Updating component type: {Id}", id);
        return await PutJsonAsync($"/api/v1/taxonomy/types/{id}", request);
    }

    public async Task<HttpResponseMessage> DeleteComponentTypeAsync(Guid id)
    {
        _logger.LogDebug("Deleting component type: {Id}", id);
        return await _httpClient.DeleteAsync($"/api/v1/taxonomy/types/{id}");
    }

    #endregion

    #region Taxonomy - Component Names

    public async Task<HttpResponseMessage> GetComponentNamesAsync(bool includeInactive = false)
    {
        _logger.LogDebug("Getting all component names (includeInactive: {IncludeInactive})", includeInactive);
        var url = includeInactive ? "/api/v1/taxonomy/names?includeInactive=true" : "/api/v1/taxonomy/names";
        return await _httpClient.GetAsync(url);
    }

    public async Task<HttpResponseMessage> GetComponentNamesByTypeAsync(Guid typeId, bool includeInactive = false)
    {
        _logger.LogDebug("Getting component names by type: {TypeId} (includeInactive: {IncludeInactive})", typeId, includeInactive);
        var url = includeInactive ? $"/api/v1/taxonomy/names/by-type/{typeId}?includeInactive=true" : $"/api/v1/taxonomy/names/by-type/{typeId}";
        return await _httpClient.GetAsync(url);
    }

    public async Task<HttpResponseMessage> GetComponentNameByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting component name by ID: {Id}", id);
        return await _httpClient.GetAsync($"/api/v1/taxonomy/names/{id}");
    }

    public async Task<HttpResponseMessage> CreateComponentNameAsync(object request)
    {
        _logger.LogDebug("Creating component name");
        return await PostJsonAsync("/api/v1/taxonomy/names", request);
    }

    public async Task<HttpResponseMessage> UpdateComponentNameAsync(Guid id, object request)
    {
        _logger.LogDebug("Updating component name: {Id}", id);
        return await PutJsonAsync($"/api/v1/taxonomy/names/{id}", request);
    }

    public async Task<HttpResponseMessage> DeleteComponentNameAsync(Guid id)
    {
        _logger.LogDebug("Deleting component name: {Id}", id);
        return await _httpClient.DeleteAsync($"/api/v1/taxonomy/names/{id}");
    }

    #endregion

    #region Taxonomy - Product Groups

    public async Task<HttpResponseMessage> GetProductGroupsAsync(bool includeInactive = false)
    {
        _logger.LogDebug("Getting all product groups (includeInactive: {IncludeInactive})", includeInactive);
        var url = includeInactive ? "/api/v1/taxonomy/product-groups?includeInactive=true" : "/api/v1/taxonomy/product-groups";
        return await _httpClient.GetAsync(url);
    }

    public async Task<HttpResponseMessage> GetProductGroupByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting product group by ID: {Id}", id);
        return await _httpClient.GetAsync($"/api/v1/taxonomy/product-groups/{id}");
    }

    public async Task<HttpResponseMessage> GetProductGroupByCodeAsync(string code)
    {
        _logger.LogDebug("Getting product group by code: {Code}", code);
        return await _httpClient.GetAsync($"/api/v1/taxonomy/product-groups/code/{code}");
    }

    public async Task<HttpResponseMessage> GetProductGroupVariantsAsync(Guid parentGroupId)
    {
        _logger.LogDebug("Getting product group variants for: {Id}", parentGroupId);
        return await _httpClient.GetAsync($"/api/v1/taxonomy/product-groups/{parentGroupId}/variants");
    }

    public async Task<HttpResponseMessage> CreateProductGroupAsync(object request)
    {
        _logger.LogDebug("Creating product group");
        return await PostJsonAsync("/api/v1/taxonomy/product-groups", request);
    }

    public async Task<HttpResponseMessage> UpdateProductGroupAsync(Guid id, object request)
    {
        _logger.LogDebug("Updating product group: {Id}", id);
        return await PutJsonAsync($"/api/v1/taxonomy/product-groups/{id}", request);
    }

    public async Task<HttpResponseMessage> DeleteProductGroupAsync(Guid id)
    {
        _logger.LogDebug("Deleting product group: {Id}", id);
        return await _httpClient.DeleteAsync($"/api/v1/taxonomy/product-groups/{id}");
    }

    #endregion

    #region MHE

    public async Task<HttpResponseMessage> GetMheTypesAsync()
    {
        _logger.LogDebug("Getting all MHE types");
        return await _httpClient.GetAsync("/api/v1/mhe");
    }

    public async Task<HttpResponseMessage> GetMheTypeByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting MHE type by ID: {Id}", id);
        return await _httpClient.GetAsync($"/api/v1/mhe/{id}");
    }

    public async Task<HttpResponseMessage> CreateMheTypeAsync(object request)
    {
        _logger.LogDebug("Creating MHE type");
        return await PostMultipartAsync("/api/v1/mhe", request);
    }

    public async Task<HttpResponseMessage> UpdateMheTypeAsync(Guid id, object request)
    {
        _logger.LogDebug("Updating MHE type: {Id}", id);
        return await PutMultipartAsync($"/api/v1/mhe/{id}", request);
    }

    public async Task<HttpResponseMessage> DeleteMheTypeAsync(Guid id)
    {
        _logger.LogDebug("Deleting MHE type: {Id}", id);
        return await _httpClient.DeleteAsync($"/api/v1/mhe/{id}");
    }
    #endregion


    #region Warehouse Types

    public async Task<HttpResponseMessage> GetWareHouseTypesAsync(bool includeInactive = false)
    {
        _logger.LogDebug("Getting all Warehouse types");
        var url = includeInactive ? "/api/v1/taxonomy/warehouse-types?includeInactive=true" : "/api/v1/taxonomy/warehouse-types";
        return await _httpClient.GetAsync(url);
    }

    public async Task<HttpResponseMessage> GetWareHouseTypesByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting Warehouse type by ID: {Id}", id);
        return await _httpClient.GetAsync($"/api/v1/taxonomy/warehouse-types/{id}");
    }

    public async Task<HttpResponseMessage> CreateWareHouseTypesAsync(object request)
    {
        _logger.LogDebug("Creating Warehouse type");
        return await PostMultipartAsync("/api/v1/taxonomy/warehouse-types", request);
    }

    public async Task<HttpResponseMessage> UpdateWareHouseTypesAsync(Guid id, object request)
    {
        _logger.LogDebug("Updating Warehouse type: {Id}", id);
        return await PostMultipartAsync($"/api/v1/taxonomy/warehouse-types/{id}", request);
    }

    public async Task<HttpResponseMessage> DeleteWareHouseTypesAsync(Guid id)
    {
        _logger.LogDebug("Deleting Warehouse type: {Id}", id);
        return await _httpClient.DeleteAsync($"/api/v1/taxonomy/warehouse-types/{id}");
    }
    #endregion

    #region Civil Components

    public async Task<HttpResponseMessage> GetCivilComponentsAsync(bool includeInactive = false)
    {
        _logger.LogDebug("Getting all Civil Components");
        var url = includeInactive ? "/api/v1/taxonomy/civil-components?includeInactive=true" : "/api/v1/taxonomy/civil-components";
        return await _httpClient.GetAsync(url);
    }

    public async Task<HttpResponseMessage> GetCivilComponentsByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting Civil Component by ID: {Id}", id);
        return await _httpClient.GetAsync($"/api/v1/taxonomy/civil-components/{id}");
    }

    public async Task<HttpResponseMessage> CreateCivilComponentsAsync(object request)
    {
        _logger.LogDebug("Creating Civil Component");
        return await PostMultipartAsync("/api/v1/taxonomy/civil-components", request);
    }

    public async Task<HttpResponseMessage> UpdateCivilComponentsAsync(Guid id, object request)
    {
        _logger.LogDebug("Updating Civil Component: {Id}", id);
        return await PostMultipartAsync($"/api/v1/taxonomy/civil-components/{id}", request);
    }

    public async Task<HttpResponseMessage> DeleteCivilComponentsAsync(Guid id)
    {
        _logger.LogDebug("Deleting Civil Component: {Id}", id);
        return await _httpClient.DeleteAsync($"/api/v1/taxonomy/civil-components/{id}");
    }
    #endregion

    #region Countries

    public async Task<HttpResponseMessage> GetCountriesAsync(bool includeInactive = false)
    {
        _logger.LogDebug("Getting all countries (includeInactive: {IncludeInactive})", includeInactive);
        var url = includeInactive ? "/api/v1/countries?includeInactive=true" : "/api/v1/countries";
        return await _httpClient.GetAsync(url);
    }

    public async Task<HttpResponseMessage> GetCountryByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting country by ID: {Id}", id);
        return await _httpClient.GetAsync($"/api/v1/countries/{id}");
    }

    public async Task<HttpResponseMessage> GetCountryByCodeAsync(string code)
    {
        _logger.LogDebug("Getting country by code: {Code}", code);
        return await _httpClient.GetAsync($"/api/v1/countries/code/{code}");
    }

    public async Task<HttpResponseMessage> CreateCountryAsync(object request)
    {
        _logger.LogDebug("Creating country");
        return await PostJsonAsync("/api/v1/countries", request);
    }

    public async Task<HttpResponseMessage> UpdateCountryAsync(Guid id, object request)
    {
        _logger.LogDebug("Updating country: {Id}", id);
        return await PutJsonAsync($"/api/v1/countries/{id}", request);
    }

    public async Task<HttpResponseMessage> DeleteCountryAsync(Guid id, string? updatedBy = null)
    {
        _logger.LogDebug("Deleting country: {Id}", id);
        var url = string.IsNullOrEmpty(updatedBy) 
            ? $"/api/v1/countries/{id}" 
            : $"/api/v1/countries/{id}?updatedBy={updatedBy}";
        return await _httpClient.DeleteAsync(url);
    }

    #endregion

    #region Currencies

    public async Task<HttpResponseMessage> GetCurrenciesAsync(bool includeInactive = false)
    {
        _logger.LogDebug("Getting all currencies (includeInactive: {IncludeInactive})", includeInactive);
        var url = includeInactive ? "/api/v1/currencies?includeInactive=true" : "/api/v1/currencies";
        return await _httpClient.GetAsync(url);
    }

    public async Task<HttpResponseMessage> GetCurrencyByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting currency by ID: {Id}", id);
        return await _httpClient.GetAsync($"/api/v1/currencies/{id}");
    }

    public async Task<HttpResponseMessage> GetCurrencyByCodeAsync(string code)
    {
        _logger.LogDebug("Getting currency by code: {Code}", code);
        return await _httpClient.GetAsync($"/api/v1/currencies/code/{code}");
    }

    public async Task<HttpResponseMessage> CreateCurrencyAsync(object request)
    {
        _logger.LogDebug("Creating currency");
        return await PostJsonAsync("/api/v1/currencies", request);
    }

    public async Task<HttpResponseMessage> UpdateCurrencyAsync(Guid id, object request)
    {
        _logger.LogDebug("Updating currency: {Id}", id);
        return await PutJsonAsync($"/api/v1/currencies/{id}", request);
    }

    public async Task<HttpResponseMessage> DeleteCurrencyAsync(Guid id, string? updatedBy = null)
    {
        _logger.LogDebug("Deleting currency: {Id}", id);
        var url = string.IsNullOrEmpty(updatedBy) 
            ? $"/api/v1/currencies/{id}" 
            : $"/api/v1/currencies/{id}?updatedBy={updatedBy}";
        return await _httpClient.DeleteAsync(url);
    }

    #endregion

    #region Exchange Rates

    public async Task<HttpResponseMessage> GetExchangeRatesAsync(bool includeInactive = false)
    {
        _logger.LogDebug("Getting all exchange rates (includeInactive: {IncludeInactive})", includeInactive);
        var url = includeInactive ? "/api/v1/exchangerates?includeInactive=true" : "/api/v1/exchangerates";
        return await _httpClient.GetAsync(url);
    }

    public async Task<HttpResponseMessage> GetExchangeRateByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting exchange rate by ID: {Id}", id);
        return await _httpClient.GetAsync($"/api/v1/exchangerates/{id}");
    }

    public async Task<HttpResponseMessage> GetLatestExchangeRateAsync(string baseCurrency, string quoteCurrency)
    {
        _logger.LogDebug("Getting latest exchange rate for {Base}/{Quote}", baseCurrency, quoteCurrency);
        return await _httpClient.GetAsync($"/api/v1/exchangerates/latest?baseCurrency={baseCurrency}&quoteCurrency={quoteCurrency}");
    }

    public async Task<HttpResponseMessage> CreateExchangeRateAsync(object request)
    {
        _logger.LogDebug("Creating exchange rate");
        return await PostJsonAsync("/api/v1/exchangerates", request);
    }

    public async Task<HttpResponseMessage> UpdateExchangeRateAsync(Guid id, object request)
    {
        _logger.LogDebug("Updating exchange rate: {Id}", id);
        return await PutJsonAsync($"/api/v1/exchangerates/{id}", request);
    }

    public async Task<HttpResponseMessage> DeleteExchangeRateAsync(Guid id, string? updatedBy = null)
    {
        _logger.LogDebug("Deleting exchange rate: {Id}", id);
        var url = string.IsNullOrEmpty(updatedBy) 
            ? $"/api/v1/exchangerates/{id}" 
            : $"/api/v1/exchangerates/{id}?updatedBy={updatedBy}";
        return await _httpClient.DeleteAsync(url);
    }

    #endregion

    #region Palette Types
    public async Task<HttpResponseMessage> GetPalettesAsync()
    {
        _logger.LogDebug("Getting all Warehouse types");
        return await _httpClient.GetAsync("/api/v1/palette");
    }
    #endregion

    #region Parts
    public async Task<HttpResponseMessage> GetPartsAsync(string? countryCode = null, Guid? componentGroupId = null, Guid? componentTypeId = null, bool? isActive = true, bool includeDeleted = false, int? page = null, int? pageSize = null)
    {
        _logger.LogDebug("Getting all parts with filters");
        var query = new List<string>();
        if (!string.IsNullOrEmpty(countryCode)) query.Add($"countryCode={countryCode}");
        if (componentGroupId.HasValue) query.Add($"componentGroupId={componentGroupId}");
        if (componentTypeId.HasValue) query.Add($"componentTypeId={componentTypeId}");
        if (isActive.HasValue) query.Add($"isActive={isActive.Value.ToString().ToLower()}");
        if (includeDeleted) query.Add("includeDeleted=true");
        if (page.HasValue) query.Add($"page={page.Value}");
        if (pageSize.HasValue) query.Add($"pageSize={pageSize.Value}");

        var queryString = query.Count > 0 ? "?" + string.Join("&", query) : "";
        return await _httpClient.GetAsync($"/api/v1/Parts{queryString}");
    }

    public async Task<HttpResponseMessage> GetPartByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting part by ID: {Id}", id);
        return await _httpClient.GetAsync($"/api/v1/Parts/{id}");
    }

    public async Task<HttpResponseMessage> GetPartByCodeAsync(string partCode, string countryCode)
    {
        _logger.LogDebug("Getting part by code: {PartCode}, country: {CountryCode}", partCode, countryCode);
        return await _httpClient.GetAsync($"/api/v1/Parts/code/{partCode}/country/{countryCode}");
    }

    public async Task<HttpResponseMessage> CreatePartAsync(object request)
    {
        _logger.LogDebug("Creating part");
        return await PostMultipartAsync("/api/v1/Parts", request);
    }

    public async Task<HttpResponseMessage> UpdatePartAsync(Guid id, object request)
    {
        _logger.LogDebug("Updating part: {Id}", id);
        return await PutMultipartAsync($"/api/v1/Parts/{id}", request);
    }

    public async Task<HttpResponseMessage> DeletePartAsync(Guid id, string? updatedBy = null)
    {
        _logger.LogDebug("Deleting part: {Id}", id);
        var url = string.IsNullOrEmpty(updatedBy) 
            ? $"/api/v1/Parts/{id}" 
            : $"/api/v1/Parts/{id}?updatedBy={updatedBy}";
        return await _httpClient.DeleteAsync(url);
    }

    public async Task<HttpResponseMessage> GetPartGroupsLookupAsync()
    {
        _logger.LogDebug("Getting part groups lookup");
        return await _httpClient.GetAsync("/api/v1/Parts/lookup/groups");
    }

    public async Task<HttpResponseMessage> GetPartTypesLookupAsync(Guid groupId)
    {
        _logger.LogDebug("Getting part types lookup for group: {GroupId}", groupId);
        return await _httpClient.GetAsync($"/api/v1/Parts/lookup/types/{groupId}");
    }

    public async Task<HttpResponseMessage> GetPartNamesLookupAsync(Guid typeId)
    {
        _logger.LogDebug("Getting part names lookup for type: {TypeId}", typeId);
        return await _httpClient.GetAsync($"/api/v1/Parts/lookup/names/{typeId}");
    }
    #endregion

    #region Load Charts

    public async Task<HttpResponseMessage> GetLoadChartsAsync(Guid? productGroupId = null, string? chartType = null, string? componentCode = null, Guid? componentTypeId = null, bool includeDeleted = false)
    {
        _logger.LogDebug("Getting all load charts with filters");
        var queryParams = new List<string>();
        if (productGroupId.HasValue) queryParams.Add($"productGroupId={productGroupId}");
        if (!string.IsNullOrEmpty(chartType)) queryParams.Add($"chartType={chartType}");
        if (!string.IsNullOrEmpty(componentCode)) queryParams.Add($"componentCode={componentCode}");
        if (componentTypeId.HasValue) queryParams.Add($"componentTypeId={componentTypeId}");
        if (includeDeleted) queryParams.Add("includeDeleted=true");

        var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
        return await _httpClient.GetAsync($"/api/v1/LoadChart{queryString}");
    }

    public async Task<HttpResponseMessage> GetLoadChartByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting load chart by ID: {Id}", id);
        return await _httpClient.GetAsync($"/api/v1/LoadChart/{id}");
    }

    public async Task<HttpResponseMessage> CreateLoadChartAsync(object request)
    {
        _logger.LogDebug("Creating load chart");
        return await PostJsonAsync("/api/v1/LoadChart", request);
    }

    public async Task<HttpResponseMessage> UpdateLoadChartAsync(Guid id, object request)
    {
        _logger.LogDebug("Updating load chart: {Id}", id);
        return await PutJsonAsync($"/api/v1/LoadChart/{id}", request);
    }

    public async Task<HttpResponseMessage> DeleteLoadChartAsync(Guid id, string? deletedBy = null)
    {
        _logger.LogDebug("Deleting load chart: {Id}", id);
        var url = string.IsNullOrEmpty(deletedBy) 
            ? $"/api/v1/LoadChart/{id}" 
            : $"/api/v1/LoadChart/{id}?deletedBy={deletedBy}";
        return await _httpClient.DeleteAsync(url);
    }

    public async Task<HttpResponseMessage> ImportLoadChartAsync(object request)
    {
        _logger.LogDebug("Importing load charts from Excel");
        return await PostMultipartAsync("/api/v1/LoadChart/import", request);
    }

    #endregion

    #region Component Master

    public async Task<HttpResponseMessage> GetComponentMastersAsync(Guid? componentGroupId = null, Guid? componentTypeId = null, string? status = null, bool includeDeleted = false)
    {
        _logger.LogDebug("Getting all component masters with filters");
        var queryParams = new List<string>();
        if (componentGroupId.HasValue) queryParams.Add($"componentGroupId={componentGroupId}");
        if (componentTypeId.HasValue) queryParams.Add($"componentTypeId={componentTypeId}");
        if (!string.IsNullOrEmpty(status)) queryParams.Add($"status={status}");
        if (includeDeleted) queryParams.Add("includeDeleted=true");

        var queryString = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
        return await _httpClient.GetAsync($"/api/v1/Component-Master{queryString}");
    }

    public async Task<HttpResponseMessage> GetComponentMasterByIdAsync(Guid id)
    {
        _logger.LogDebug("Getting component master by ID: {Id}", id);
        return await _httpClient.GetAsync($"/api/v1/Component-Master/{id}");
    }

    public async Task<HttpResponseMessage> GetComponentMasterByCodeAsync(string code, string countryCode)
    {
        _logger.LogDebug("Getting component master by code: {Code}, country: {Country}", code, countryCode);
        return await _httpClient.GetAsync($"/api/v1/Component-Master/code/{code}/country/{countryCode}");
    }

    public async Task<HttpResponseMessage> CreateComponentMasterAsync(object request)
    {
        _logger.LogDebug("Creating component master");
        return await PostMultipartAsync("/api/v1/Component-Master", request);
    }

    public async Task<HttpResponseMessage> UpdateComponentMasterAsync(Guid id, object request)
    {
        _logger.LogDebug("Updating component master: {Id}", id);
        return await PutMultipartAsync($"/api/v1/Component-Master/{id}", request);
    }

    public async Task<HttpResponseMessage> DeleteComponentMasterAsync(Guid id, string? updatedBy = null)
    {
        _logger.LogDebug("Deleting component master: {Id}", id);
        var url = string.IsNullOrEmpty(updatedBy) 
            ? $"/api/v1/Component-Master/{id}" 
            : $"/api/v1/Component-Master/{id}?updatedBy={updatedBy}";
        return await _httpClient.DeleteAsync(url);
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

    private async Task<HttpResponseMessage> PostMultipartAsync(string url, object request)
    {
        using var content = CreateMultipartContent(request);
        return await _httpClient.PostAsync(url, content);
    }

    private async Task<HttpResponseMessage> PutMultipartAsync(string url, object request)
    {
        using var content = CreateMultipartContent(request);
        return await _httpClient.PutAsync(url, content);
    }

    private MultipartFormDataContent CreateMultipartContent(object request)
    {
        var content = new MultipartFormDataContent();
        var properties = request.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);

        foreach (var prop in properties)
        {
            var value = prop.GetValue(request);
            if (value == null) continue;

            if (value is IFormFile file)
            {
                var fileContent = new StreamContent(file.OpenReadStream());
                fileContent.Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, prop.Name, file.FileName);
            }
            else if (value is bool boolValue)
            {
                content.Add(new StringContent(boolValue.ToString().ToLower()), prop.Name);
            }
            else
            {
                content.Add(new StringContent(value.ToString() ?? string.Empty), prop.Name);
            }
        }

        return content;
    }

    #endregion
}
