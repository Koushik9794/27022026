using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace GssWebApi.Services;

/// <summary>
/// HTTP client for communicating with the Catalog Service.
/// </summary>
public interface ICatalogServiceClient
{
    //MHE
    Task<HttpResponseMessage> GetMheTypesAsync();
    Task<HttpResponseMessage> GetMheTypeByIdAsync(Guid id);
    Task<HttpResponseMessage> CreateMheTypeAsync(object request);
    Task<HttpResponseMessage> UpdateMheTypeAsync(Guid id, object request);
    Task<HttpResponseMessage> DeleteMheTypeAsync(Guid id);


    // SKU Types
    Task<HttpResponseMessage> GetSkuTypesAsync();
    Task<HttpResponseMessage> GetSkuTypeByIdAsync(Guid id);
    Task<HttpResponseMessage> CreateSkuTypeAsync(object request);
    Task<HttpResponseMessage> UpdateSkuTypeAsync(Guid id, object request);
    Task<HttpResponseMessage> DeleteSkuTypeAsync(Guid id);

    // Pallet Types
    Task<HttpResponseMessage> GetPalletTypesAsync(bool includeInactive = false);
    Task<HttpResponseMessage> GetPalletTypeByIdAsync(Guid id);
    Task<HttpResponseMessage> GetPalletTypeByCodeAsync(string code);
    Task<HttpResponseMessage> CreatePalletTypeAsync(object request);
    Task<HttpResponseMessage> UpdatePalletTypeAsync(Guid id, object request);
    Task<HttpResponseMessage> DeletePalletTypeAsync(Guid id);

    // Taxonomy - Component Groups
    Task<HttpResponseMessage> GetComponentGroupsAsync(bool includeInactive = false);
    Task<HttpResponseMessage> GetComponentGroupByIdAsync(Guid id);
    Task<HttpResponseMessage> GetComponentGroupByCodeAsync(string code);
    Task<HttpResponseMessage> CreateComponentGroupAsync(object request);
    Task<HttpResponseMessage> UpdateComponentGroupAsync(Guid id, object request);
    Task<HttpResponseMessage> DeleteComponentGroupAsync(Guid id);

    // Taxonomy - Component Types
    Task<HttpResponseMessage> GetComponentTypesAsync(string? componentGroupCode = null, bool includeInactive = false);
    Task<HttpResponseMessage> GetComponentTypeByIdAsync(Guid id);
    Task<HttpResponseMessage> GetComponentTypeByCodeAsync(string code);
    Task<HttpResponseMessage> CreateComponentTypeAsync(object request);
    Task<HttpResponseMessage> UpdateComponentTypeAsync(Guid id, object request);
    Task<HttpResponseMessage> DeleteComponentTypeAsync(Guid id);

    // Taxonomy - Component Names
    Task<HttpResponseMessage> GetComponentNamesAsync(bool includeInactive = false);
    Task<HttpResponseMessage> GetComponentNamesByTypeAsync(Guid typeId, bool includeInactive = false);
    Task<HttpResponseMessage> GetComponentNameByIdAsync(Guid id);
    Task<HttpResponseMessage> CreateComponentNameAsync(object request);
    Task<HttpResponseMessage> UpdateComponentNameAsync(Guid id, object request);
    Task<HttpResponseMessage> DeleteComponentNameAsync(Guid id);

    // Taxonomy - Product Groups
    Task<HttpResponseMessage> GetProductGroupsAsync(bool includeInactive = false);
    Task<HttpResponseMessage> GetProductGroupByIdAsync(Guid id);
    Task<HttpResponseMessage> GetProductGroupByCodeAsync(string code);
    Task<HttpResponseMessage> GetProductGroupVariantsAsync(Guid parentGroupId);
    Task<HttpResponseMessage> CreateProductGroupAsync(object request);
    Task<HttpResponseMessage> UpdateProductGroupAsync(Guid id, object request);
    Task<HttpResponseMessage> DeleteProductGroupAsync(Guid id);

    // Taxonomy - WareHouse Types
    Task<HttpResponseMessage> GetWareHouseTypesAsync(bool includeInactive = false);
    Task<HttpResponseMessage> GetWareHouseTypesByIdAsync(Guid id);
    Task<HttpResponseMessage> CreateWareHouseTypesAsync(object request);
    Task<HttpResponseMessage> UpdateWareHouseTypesAsync(Guid id, object request);
    Task<HttpResponseMessage> DeleteWareHouseTypesAsync(Guid id);

    // Taxonomy - Civil Components
    Task<HttpResponseMessage> GetCivilComponentsAsync(bool includeInactive = false);
    Task<HttpResponseMessage> GetCivilComponentsByIdAsync(Guid id);
    Task<HttpResponseMessage> CreateCivilComponentsAsync(object request);
    Task<HttpResponseMessage> UpdateCivilComponentsAsync(Guid id, object request);
    Task<HttpResponseMessage> DeleteCivilComponentsAsync(Guid id);


    // Countries
    Task<HttpResponseMessage> GetCountriesAsync(bool includeInactive = false);
    Task<HttpResponseMessage> GetCountryByIdAsync(Guid id);
    Task<HttpResponseMessage> GetCountryByCodeAsync(string code);
    Task<HttpResponseMessage> CreateCountryAsync(object request);
    Task<HttpResponseMessage> UpdateCountryAsync(Guid id, object request);
    Task<HttpResponseMessage> DeleteCountryAsync(Guid id, string? updatedBy = null);

    // Currencies
    Task<HttpResponseMessage> GetCurrenciesAsync(bool includeInactive = false);
    Task<HttpResponseMessage> GetCurrencyByIdAsync(Guid id);
    Task<HttpResponseMessage> GetCurrencyByCodeAsync(string code);
    Task<HttpResponseMessage> CreateCurrencyAsync(object request);
    Task<HttpResponseMessage> UpdateCurrencyAsync(Guid id, object request);
    Task<HttpResponseMessage> DeleteCurrencyAsync(Guid id, string? updatedBy = null);

    // Exchange Rates
    Task<HttpResponseMessage> GetExchangeRatesAsync(bool includeInactive = false);
    Task<HttpResponseMessage> GetExchangeRateByIdAsync(Guid id);
    Task<HttpResponseMessage> GetLatestExchangeRateAsync(string baseCurrency, string quoteCurrency);
    Task<HttpResponseMessage> CreateExchangeRateAsync(object request);
    Task<HttpResponseMessage> UpdateExchangeRateAsync(Guid id, object request);
    Task<HttpResponseMessage> DeleteExchangeRateAsync(Guid id, string? updatedBy = null);

    // Design Deck
    Task<HttpResponseMessage> GetPalettesAsync();

    // Parts
    Task<HttpResponseMessage> GetPartsAsync(string? countryCode = null, Guid? componentGroupId = null, Guid? componentTypeId = null, bool? isActive = true, bool includeDeleted = false, int? page = null, int? pageSize = null);
    Task<HttpResponseMessage> GetPartByIdAsync(Guid id);
    Task<HttpResponseMessage> GetPartByCodeAsync(string partCode, string countryCode);
    Task<HttpResponseMessage> CreatePartAsync(object request);
    Task<HttpResponseMessage> UpdatePartAsync(Guid id, object request);
    Task<HttpResponseMessage> DeletePartAsync(Guid id, string? updatedBy = null);
    Task<HttpResponseMessage> GetPartGroupsLookupAsync();
    Task<HttpResponseMessage> GetPartTypesLookupAsync(Guid groupId);
    Task<HttpResponseMessage> GetPartNamesLookupAsync(Guid typeId);
    // Load Charts
    Task<HttpResponseMessage> GetLoadChartsAsync(Guid? productGroupId = null, string? chartType = null, string? componentCode = null, Guid? componentTypeId = null, bool includeDeleted = false);
    Task<HttpResponseMessage> GetLoadChartByIdAsync(Guid id);
    Task<HttpResponseMessage> CreateLoadChartAsync(object request);
    Task<HttpResponseMessage> UpdateLoadChartAsync(Guid id, object request);
    Task<HttpResponseMessage> DeleteLoadChartAsync(Guid id, string? deletedBy = null);
    Task<HttpResponseMessage> ImportLoadChartAsync(object request);

    // Component Master
    Task<HttpResponseMessage> GetComponentMastersAsync(Guid? componentGroupId = null, Guid? componentTypeId = null, string? status = null, bool includeDeleted = false);
    Task<HttpResponseMessage> GetComponentMasterByIdAsync(Guid id);
    Task<HttpResponseMessage> GetComponentMasterByCodeAsync(string code, string countryCode);
    Task<HttpResponseMessage> CreateComponentMasterAsync(object request);
    Task<HttpResponseMessage> UpdateComponentMasterAsync(Guid id, object request);
    Task<HttpResponseMessage> DeleteComponentMasterAsync(Guid id, string? updatedBy = null);
}
