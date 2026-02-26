using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
namespace GssWebApi.Dto;
// Orchestrated Response
public record MheBrowserResponse<T>(T Data, List<AttributeDefinitionDto> AttributeDefinitions);
// Catalog Service DTOs (Mirrors)
public record MheDto(
     Guid Id,
     string Code,
     string Name,
     string? Manufacturer,
     string? Brand,
     string? Model,
     string? MheType,
     string? MheCategory,
     string? GlbFilePath,
     Dictionary<string, JsonElement> Attributes,
     bool IsActive,
     bool IsDeleted,
     DateTimeOffset CreatedAt,
     string? CreatedBy,
     DateTimeOffset? UpdatedAt,
     string? UpdatedBy
);
public record PartDto(
    Guid Id,
    string PartCode,
    string CountryCode,
    string? UnspscCode,
    Guid ComponentGroupId,
    string? ComponentGroupCode,
    string? ComponentGroupName,
    Guid ComponentTypeId,
    string? ComponentTypeCode,
    string? ComponentTypeName,
    Guid? ComponentNameId,
    string? ComponentNameCode,
    string? ComponentNameName,
    string? Colour,
    string? PowderCode,
    bool GfaFlag,
    decimal UnitBasicPrice,
    decimal? Cbm,
    string? ShortDescription,
    string? Description,
    string? DrawingNo,
    string? RevNo,
    string? InstallationRefNo,
    Dictionary<string, JsonElement>? Attributes,
    string? GlbFilepath,
    string? ImageUrl,
    string Status,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? CreatedBy,
    string? UpdatedBy
);
public record AttributeDefinitionDto(
    Guid Id,
    string AttributeKey,
    string DisplayName,
    string? Unit,
    int DataType, // Keeping as int/enum based on JSON
    decimal? MinValue,
    decimal? MaxValue,
    JsonElement? DefaultValue,
    bool IsRequired,
    JsonElement? AllowedValues,
    string? Description,
    bool IsActive,
    bool IsDeleted
);
public record WarehouseTypeDto(
    Guid Id,
    string Name,
    string Label,
    string Icon,
    string Tooltip,
    string Category,
    string? TemplatePath,
    Dictionary<string, object>? Attributes,
    List<string> AllowedRoles
);
public record SkuDto(
    Guid Id,
    string Name,
    string Label,
    string Icon,
    string Tooltip,
    string Category,
    Dictionary<string, object>? Attributes,
    List<string> AllowedRoles
);
public record PalletDto(
    Guid Id,
    string Name,
    string Label,
    string Icon,
    string Tooltip,
    string Category,
    string Type,
    Dictionary<string, object>? Attributes,
    List<string> AllowedRoles
);
public record MheCatalogDto(
    Guid Id,
    string Name,
    string Label,
    string Icon,
    string Tooltip,
    string Category,
    string Type,
    Dictionary<string, object>? Attributes,
    List<string> AllowedRoles
);
public record ProductGroupDto(
    Guid Id,
    string Name,
    string Label,
    string Icon,
    string Tooltip,
    string Category,
    object DefaultElement,
    List<string> AllowedRoles
);
public record StructureElementDto(
    Guid Id,
    string Name,
    string Label,
    string Icon,
    string Tooltip,
    string Category,
    object DefaultElement,
    List<string> AllowedRoles
);
public record OpeningDto(
    Guid Id,
    string Name,
    string Label,
    string Icon,
    string Tooltip,
    string Category,
    object DefaultElement,
    List<string> AllowedRoles
);
// Add this class to deserialize the Result<T> wrapper
public class CatalogServiceResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public CatalogError? Error { get; set; }
}
// Ensure this DTO exists for Structure/Opening
public record CivilComponentDto(
    Guid Id,
    string Name,
    string Label,
    string Icon,
    string Tooltip,
    string Category,
    object DefaultElement,
    List<string> AllowedRoles
);
// Generic Result Wrapper to handle Catalog Service standard response
public class CatalogResult<T>
{
    public bool IsSuccess { get; set; }
    public T? Value { get; set; }
    public CatalogError? Error { get; set; }
}
public class CatalogError
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
}
/// <summary>
/// Request DTO for creating a new MHE.
/// </summary>
/// <param name="Code">[REQUIRED] Unique code.</param>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Manufacturer">[OPTIONAL] Manufacturer name.</param>
/// <param name="Brand">[OPTIONAL] Brand name.</param>
/// <param name="Model">[OPTIONAL] Model identifier.</param>
/// <param name="MheType">[OPTIONAL] Type of MHE.</param>
/// <param name="MheCategory">[OPTIONAL] Category of MHE.</param>
/// <param name="GlbFile">[OPTIONAL] 3D model file (GLB format).</param>
/// <param name="Attributes">[OPTIONAL] JSON string of dynamic attributes.</param>
/// <param name="IsActive">[REQUIRED] Whether the MHE is active.</param>
/// <param name="CreatedBy">[OPTIONAL] User identifier.</param>
public record CreateMheRequest(string? Code, string? Name, string? Manufacturer, string? Brand, string? Model, string? MheType, string? MheCategory, IFormFile? GlbFile, string? Attributes, bool IsActive, string? CreatedBy);
/// <summary>
/// Request DTO for updating an existing MHE.
/// </summary>
/// <param name="Code">[REQUIRED] Unique code.</param>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Manufacturer">[OPTIONAL] Manufacturer name.</param>
/// <param name="Brand">[OPTIONAL] Brand name.</param>
/// <param name="Model">[OPTIONAL] Model identifier.</param>
/// <param name="MheType">[OPTIONAL] Type of MHE.</param>
/// <param name="MheCategory">[OPTIONAL] Category of MHE.</param>
/// <param name="GlbFile">[OPTIONAL] 3D model file (GLB format).</param>
/// <param name="Attributes">[OPTIONAL] JSON string of dynamic attributes.</param>
/// <param name="IsActive">[REQUIRED] Whether the MHE is active.</param>
/// <param name="UpdatedBy">[OPTIONAL] User identifier.</param>
public record UpdateMheRequest(string? Code, string? Name, string? Manufacturer, string? Brand, string? Model, string? MheType, string? MheCategory, IFormFile? GlbFile, string? Attributes, bool IsActive, string? UpdatedBy);

/// <summary>
/// Request DTO for creating a new pallet type.
/// </summary>
/// <param name="Code">[REQUIRED] Unique code.</param>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="AttributeSchema">[OPTIONAL] JSON object defining dynamic attributes.</param>
/// <param name="GlbFile">[OPTIONAL] 3D model file (GLB format).</param>
/// <param name="IsActive">[OPTIONAL] Whether the pallet type is active (default: true).</param>
public record CreatePalletRequest(string Code, string Name, string? Description, string? AttributeSchema, IFormFile? GlbFile = null, bool IsActive = true);
/// <summary>
/// Request DTO for updating an existing pallet type.
/// </summary>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="AttributeSchema">[OPTIONAL] JSON object defining dynamic attributes.</param>
/// <param name="GlbFile">[OPTIONAL] 3D model file (GLB format).</param>
/// <param name="IsActive">[REQUIRED] Whether the pallet type is active.</param>
public record UpdatePalletRequest(string Name, string? Description, string? AttributeSchema, IFormFile? GlbFile, bool IsActive);

/// <summary>
/// Request DTO for creating a new SKU type.
/// </summary>
/// <param name="Code">[REQUIRED] Unique code.</param>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="AttributeSchema">[OPTIONAL] JSON object defining dynamic attributes.</param>
/// <param name="GlbFile">[OPTIONAL] 3D model file (GLB format).</param>
/// <param name="IsActive">[OPTIONAL] Whether the SKU type is active (default: true).</param>
public record CreateSkuRequest(string Code, string Name, string? Description, string? AttributeSchema, IFormFile? GlbFile, bool IsActive = true);
/// <summary>
/// Request DTO for updating an existing SKU type.
/// </summary>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="AttributeSchema">[OPTIONAL] JSON object defining dynamic attributes.</param>
/// <param name="GlbFile">[OPTIONAL] 3D model file (GLB format).</param>
/// <param name="IsActive">[REQUIRED] Whether the SKU type is active.</param>
public record UpdateSkuRequest(string Name, string? Description, string? AttributeSchema, IFormFile? GlbFile, bool IsActive);

/// <summary>
/// Request DTO for creating a new part.
/// </summary>
/// <param name="PartCode">[REQUIRED] Unique part code identifier.</param>
/// <param name="CountryCode">[REQUIRED] 2-character ISO country code.</param>
/// <param name="UnspscCode">[OPTIONAL] UNSPSC category code.</param>
/// <param name="ComponentGroupId">[REQUIRED] Associated component group identifier.</param>
/// <param name="ComponentTypeId">[REQUIRED] Associated component type identifier.</param>
/// <param name="ComponentNameId">[OPTIONAL] Associated component name identifier.</param>
/// <param name="Colour">[OPTIONAL] Part colour details.</param>
/// <param name="PowderCode">[OPTIONAL] Powder coating code.</param>
/// <param name="GfaFlag">[REQUIRED] General functionality flag.</param>
/// <param name="UnitBasicPrice">[REQUIRED] Base unit price.</param>
/// <param name="Cbm">[OPTIONAL] Cubic meters measurement.</param>
/// <param name="ShortDescription">[OPTIONAL] Concise part description.</param>
/// <param name="Description">[OPTIONAL] Detailed part description.</param>
/// <param name="DrawingNo">[OPTIONAL] Technical drawing number.</param>
/// <param name="RevNo">[OPTIONAL] Revision number.</param>
/// <param name="InstallationRefNo">[OPTIONAL] Installation reference number.</param>
/// <param name="Attributes">[OPTIONAL] JSON string of dynamic attributes.</param>
/// <param name="GlbFile">[OPTIONAL] 3D model file (GLB format).</param>
/// <param name="ImageFile">[OPTIONAL] Visual image file (JPG/PNG format).</param>
/// <param name="CreatedBy">[OPTIONAL] User identifier.</param>
public record CreatePartRequest(string PartCode, string CountryCode, string? UnspscCode, Guid ComponentGroupId, Guid ComponentTypeId, Guid? ComponentNameId, string? Colour, string? PowderCode, bool GfaFlag, decimal UnitBasicPrice, decimal? Cbm, string? ShortDescription, string? Description, string? DrawingNo, string? RevNo, string? InstallationRefNo, string? Attributes, IFormFile? GlbFile, IFormFile? ImageFile, string? CreatedBy);
/// <summary>
/// Request DTO for updating an existing part.
/// </summary>
/// <param name="UnspscCode">[OPTIONAL] UNSPSC category code.</param>
/// <param name="ComponentGroupId">[REQUIRED] Associated component group identifier.</param>
/// <param name="ComponentTypeId">[REQUIRED] Associated component type identifier.</param>
/// <param name="ComponentNameId">[OPTIONAL] Associated component name identifier.</param>
/// <param name="Colour">[OPTIONAL] Part colour details.</param>
/// <param name="PowderCode">[OPTIONAL] Powder coating code.</param>
/// <param name="GfaFlag">[REQUIRED] General functionality flag.</param>
/// <param name="UnitBasicPrice">[REQUIRED] Base unit price.</param>
/// <param name="Cbm">[OPTIONAL] Cubic meters measurement.</param>
/// <param name="ShortDescription">[OPTIONAL] Concise part description.</param>
/// <param name="Description">[OPTIONAL] Detailed part description.</param>
/// <param name="DrawingNo">[OPTIONAL] Technical drawing number.</param>
/// <param name="RevNo">[OPTIONAL] Revision number.</param>
/// <param name="InstallationRefNo">[OPTIONAL] Installation reference number.</param>
/// <param name="Attributes">[OPTIONAL] JSON string of dynamic attributes.</param>
/// <param name="GlbFile">[OPTIONAL] 3D model file (GLB format).</param>
/// <param name="ImageFile">[OPTIONAL] Visual image file (JPG/PNG format).</param>
/// <param name="Status">[REQUIRED] Current part status (e.g., 'Draft', 'Published').</param>
/// <param name="UpdatedBy">[OPTIONAL] User identifier.</param>
public record UpdatePartRequest(string? UnspscCode, Guid ComponentGroupId, Guid ComponentTypeId, Guid? ComponentNameId, string? Colour, string? PowderCode, bool GfaFlag, decimal UnitBasicPrice, decimal? Cbm, string? ShortDescription, string? Description, string? DrawingNo, string? RevNo, string? InstallationRefNo, string? Attributes, IFormFile? GlbFile, IFormFile? ImageFile, string Status, string? UpdatedBy);

// Component Master DTOs
public record ComponentMasterDto(
    Guid Id,
    string ComponentMasterCode,
    string CountryCode,
    string? UnspscCode,
    Guid ComponentGroupId,
    string? ComponentGroupCode,
    string? ComponentGroupName,
    Guid ComponentTypeId,
    string? ComponentTypeCode,
    string? ComponentTypeName,
    Guid? ComponentNameId,
    string? ComponentNameCode,
    string? ComponentNameName,
    string? Colour,
    string? PowderCode,
    bool GfaFlag,
    decimal UnitBasicPrice,
    decimal? Cbm,
    string? ShortDescription,
    string? Description,
    string? DrawingNo,
    string? RevNo,
    string? InstallationRefNo,
    Dictionary<string, JsonElement>? Attributes,
    string? GlbFilepath,
    string? ImageUrl,
    string Status,
    bool IsDeleted,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? CreatedBy,
    string? UpdatedBy
);

public record CreateComponentMasterRequest(
    string ComponentMasterCode,
    string CountryCode,
    string? UnspscCode,
    Guid ComponentGroupId,
    Guid ComponentTypeId,
    Guid? ComponentNameId,
    string? Colour,
    string? PowderCode,
    bool GfaFlag,
    decimal UnitBasicPrice,
    decimal? Cbm,
    string? ShortDescription,
    string? Description,
    string? DrawingNo,
    string? RevNo,
    string? InstallationRefNo,
    string? Attributes, 
    IFormFile? GlbFile,
    IFormFile? ImageFile,
    string? CreatedBy
);

public record UpdateComponentMasterRequest(
    string? UnspscCode,
    Guid ComponentGroupId,
    Guid ComponentTypeId,
    Guid? ComponentNameId,
    string? Colour,
    string? PowderCode,
    bool GfaFlag,
    decimal UnitBasicPrice,
    decimal? Cbm,
    string? ShortDescription,
    string? Description,
    string? DrawingNo,
    string? RevNo,
    string? InstallationRefNo,
    string? Attributes, 
    IFormFile? GlbFile,
    IFormFile? ImageFile,
    string Status,
    string? UpdatedBy
);

// Taxonomy DTOs
public record ComponentGroupDto(Guid Id, string Code, string Name, string? Description, int SortOrder, bool IsActive, DateTime CreatedAt, DateTime? UpdatedAt);
/// <summary>
/// Request DTO for creating a new component group.
/// </summary>
/// <param name="Code">[REQUIRED] Unique code.</param>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="SortOrder">[OPTIONAL] Order in list (default: 0).</param>
public record CreateComponentGroupRequest(string Code, string Name, string? Description, int SortOrder = 0);
/// <summary>
/// Request DTO for updating an existing component group.
/// </summary>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="SortOrder">[REQUIRED] Order in list.</param>
public record UpdateComponentGroupRequest(string Name, string? Description, int SortOrder);

public record ComponentTypeDto(Guid Id, string Code, string Name, string? Description, Guid ComponentGroupId, string? ComponentGroupCode, string? ComponentGroupName, Guid? ParentTypeId, string? ParentTypeCode, object? AttributeSchema, bool IsActive, DateTime CreatedAt, DateTime? UpdatedAt);
/// <summary>
/// Request DTO for creating a new component type.
/// </summary>
/// <param name="Code">[REQUIRED] Unique code.</param>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="ComponentGroupCode">[REQUIRED] Associated component group code.</param>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="ParentTypeCode">[OPTIONAL] Code of the parent type if any.</param>
/// <param name="AttributeSchema">[OPTIONAL] JSON object defining dynamic attributes.</param>
public record CreateComponentTypeRequest(string Code, string Name, string ComponentGroupCode, string? Description, string? ParentTypeCode, object? AttributeSchema);
/// <summary>
/// Request DTO for updating an existing component type.
/// </summary>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="ComponentGroupCode">[REQUIRED] Associated component group code.</param>
/// <param name="ParentTypeCode">[OPTIONAL] Code of the parent type if any.</param>
/// <param name="AttributeSchema">[OPTIONAL] JSON object defining dynamic attributes.</param>
public record UpdateComponentTypeRequest(string Name, string? Description, string ComponentGroupCode, string? ParentTypeCode, object? AttributeSchema);

public record ComponentNameDto(Guid Id, string Code, string Name, string? Description, Guid ComponentTypeId, string? ComponentTypeCode, string? ComponentTypeName, bool IsActive, DateTime CreatedAt, DateTime? UpdatedAt);
/// <summary>
/// Request DTO for creating a new component name.
/// </summary>
/// <param name="Code">[REQUIRED] Unique code.</param>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="ComponentTypeId">[REQUIRED] Associated component type identifier.</param>
public record CreateComponentNameRequest(string Code, string Name, string? Description, Guid ComponentTypeId);
/// <summary>
/// Request DTO for updating an existing component name.
/// </summary>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="ComponentTypeId">[REQUIRED] Associated component type identifier.</param>
public record UpdateComponentNameRequest(string Name, string? Description, Guid ComponentTypeId);

/// <summary>
/// Request DTO for creating a new product group.
/// </summary>
/// <param name="Code">[REQUIRED] Unique code.</param>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="ParentGroupCode">[OPTIONAL] Code of the parent group if any.</param>
public record CreateProductGroupRequest(string Code, string Name, string? Description = null, string? ParentGroupCode = null);
/// <summary>
/// Request DTO for updating an existing product group.
/// </summary>
/// <param name="Name">[REQUIRED] Display name.</param>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="ParentGroupCode">[OPTIONAL] Code of the parent group if any.</param>
public record UpdateProductGroupRequest(string Name, string? Description, string? ParentGroupCode);

// Country DTOs
/// <summary>
/// Data transfer object for Country information.
/// </summary>
/// <param name="Id">Unique identifier of the country.</param>
/// <param name="CountryCode">2-character ISO country code (e.g., 'IN').</param>
/// <param name="CountryName">Full name of the country (e.g., 'India').</param>
/// <param name="CurrencyCode">Primary currency code used in the country (e.g., 'INR').</param>
/// <param name="IsActive">Whether the country is active.</param>
/// <param name="IsDelete">Whether the country is soft-deleted.</param>
/// <param name="CreatedAt">Timestamp of creation.</param>
/// <param name="CreatedBy">User who created the record.</param>
/// <param name="UpdatedBy">User who last updated the record.</param>
/// <param name="UpdatedAt">Timestamp of last update.</param>
public record CountryDto(
    Guid Id,
    string CountryCode,
    string CountryName,
    string CurrencyCode,
    bool IsActive,
    bool IsDelete,
    DateTime CreatedAt,
    string? CreatedBy,
    string? UpdatedBy,
    DateTime UpdatedAt
);

/// <summary>
/// Request DTO for creating a new country.
/// </summary>
/// <param name="CountryCode">[REQUIRED] 2-character ISO code.</param>
/// <param name="CountryName">[REQUIRED] Full name of the country.</param>
/// <param name="CurrencyCode">[REQUIRED] 3-character currency code.</param>
/// <param name="CreatedBy">[OPTIONAL] User identifier.</param>
public record CreateCountryRequest(
    string CountryCode,
    string CountryName,
    string CurrencyCode,
    string? CreatedBy = null
);

/// <summary>
/// Request DTO for updating an existing country.
/// </summary>
/// <param name="CountryName">[REQUIRED] Updated name.</param>
/// <param name="CurrencyCode">[REQUIRED] Updated 3-character currency code.</param>
/// <param name="IsActive">[REQUIRED] Update active status.</param>
/// <param name="UpdatedBy">[OPTIONAL] User identifier.</param>
public record UpdateCountryRequest(
    string CountryName,
    string CurrencyCode,
    bool IsActive,
    string? UpdatedBy = null
);

// Currency DTOs
/// <summary>
/// Data transfer object for Currency information.
/// </summary>
/// <param name="Id">Unique identifier of the currency.</param>
/// <param name="CurrencyCode">3-character ISO currency code (e.g., 'INR').</param>
/// <param name="CurrencyName">Full name of the currency (e.g., 'Indian Rupee').</param>
/// <param name="CurrencyValue">Optional display value or symbol.</param>
/// <param name="DecimalUnit">Number of decimal places (0-4).</param>
/// <param name="IsActive">Whether the currency is active.</param>
/// <param name="IsDelete">Whether the currency is soft-deleted.</param>
/// <param name="CreatedAt">Timestamp of creation.</param>
/// <param name="CreatedBy">User who created the record.</param>
/// <param name="UpdatedBy">User who last updated the record.</param>
/// <param name="UpdatedAt">Timestamp of last update.</param>
public record CurrencyDto(
    Guid Id,
    string CurrencyCode,
    string CurrencyName,
    string? CurrencyValue,
    short DecimalUnit,
    bool IsActive,
    bool IsDelete,
    DateTime CreatedAt,
    string? CreatedBy,
    string? UpdatedBy,
    DateTime UpdatedAt
);

/// <summary>
/// Request DTO for creating a new currency.
/// </summary>
/// <param name="CurrencyCode">[REQUIRED] 3-character ISO code.</param>
/// <param name="CurrencyName">[REQUIRED] Full name of the currency.</param>
/// <param name="CurrencyValue">[OPTIONAL] Display symbol or symbol.</param>
/// <param name="DecimalUnit">[OPTIONAL] Decimal places (default 2, range 0-4).</param>
/// <param name="CreatedBy">[OPTIONAL] User identifier.</param>
public record CreateCurrencyRequest(
    string CurrencyCode,
    string CurrencyName,
    string? CurrencyValue = null,
    short DecimalUnit = 2,
    string? CreatedBy = null
);

/// <summary>
/// Request DTO for updating an existing currency.
/// </summary>
/// <param name="CurrencyName">[REQUIRED] Updated name.</param>
/// <param name="CurrencyValue">[OPTIONAL] Updated display symbol.</param>
/// <param name="DecimalUnit">[REQUIRED] Updated decimal places (0-4).</param>
/// <param name="IsActive">[REQUIRED] Update active status.</param>
/// <param name="UpdatedBy">[OPTIONAL] User identifier.</param>
public record UpdateCurrencyRequest(
    string CurrencyName,
    string? CurrencyValue,
    short DecimalUnit,
    bool IsActive,
    string? UpdatedBy = null
);

// Exchange Rate DTOs
/// <summary>
/// Data transfer object for Exchange Rate information.
/// </summary>
/// <param name="Id">Unique identifier of the exchange rate.</param>
/// <param name="BaseCurrency">3-character base currency code (e.g., 'USD').</param>
/// <param name="QuoteCurrency">3-character target currency code (e.g., 'INR').</param>
/// <param name="Rate">Exchange rate value (Base * Rate = Quote).</param>
/// <param name="ValidFrom">Start date of the rate validity.</param>
/// <param name="ValidEnd">Optional end date of the rate validity.</param>
/// <param name="IsActive">Whether the rate is active.</param>
/// <param name="IsDelete">Whether the record is soft-deleted.</param>
/// <param name="CreatedAt">Timestamp of creation.</param>
/// <param name="CreatedBy">User who created the record.</param>
/// <param name="UpdatedBy">User who last updated the record.</param>
/// <param name="UpdatedAt">Timestamp of last update.</param>
public record ExchangeRateDto(
    Guid Id,
    string BaseCurrency,
    string QuoteCurrency,
    decimal Rate,
    DateTime ValidFrom,
    DateTime? ValidEnd,
    bool IsActive,
    bool IsDelete,
    DateTime CreatedAt,
    string? CreatedBy,
    string? UpdatedBy,
    DateTime UpdatedAt
);

/// <summary>
/// Request DTO for creating a new exchange rate.
/// </summary>
/// <param name="BaseCurrency">[REQUIRED] 3-character base currency code.</param>
/// <param name="QuoteCurrency">[REQUIRED] 3-character target currency code.</param>
/// <param name="Rate">[REQUIRED] Positive numeric rate.</param>
/// <param name="ValidFrom">[REQUIRED] Start of validity period.</param>
/// <param name="ValidEnd">[OPTIONAL] End of validity period.</param>
/// <param name="CreatedBy">[OPTIONAL] User identifier.</param>
public record CreateExchangeRateRequest(
    string BaseCurrency,
    string QuoteCurrency,
    decimal Rate,
    DateTime ValidFrom,
    DateTime? ValidEnd = null,
    string? CreatedBy = null
);

/// <summary>
/// Request DTO for updating an existing exchange rate.
/// </summary>
/// <param name="Rate">[REQUIRED] Updated numeric rate.</param>
/// <param name="ValidFrom">[REQUIRED] Updated start of validity period.</param>
/// <param name="ValidEnd">[OPTIONAL] Updated end of validity period.</param>
/// <param name="IsActive">[REQUIRED] Update active status.</param>
/// <param name="UpdatedBy">[OPTIONAL] User identifier.</param>
public record UpdateExchangeRateRequest(
    decimal Rate,
    DateTime ValidFrom,
    DateTime? ValidEnd,
    bool IsActive,
    string? UpdatedBy = null
);
// Load Chart DTOs
public record LoadChartDto(
    Guid Id,
    Guid ProductGroupId,
    string? ProductGroupName,
    string ChartType,
    string ComponentCode,
    Guid ComponentTypeId,
    string? ComponentName,
    Dictionary<string, JsonElement> Attributes,
    bool IsActive,
    bool IsDelete,
    DateTime CreatedAt,
    DateTime? UpdatedAt,
    string? CreatedBy,
    string? UpdatedBy
);

public record CreateLoadChartRequest(
    Guid ProductGroupId,
    string ChartType,
    string ComponentCode,
    Guid ComponentTypeId,
    string? Attributes,
    string? CreatedBy
);

public record UpdateLoadChartRequest(
    Guid ProductGroupId,
    string ChartType,
    string ComponentCode,
    Guid ComponentTypeId,
    string? Attributes,
    bool IsActive,
    string? UpdatedBy
);

public record ImportLoadChartExcelRequest(
    IFormFile File,
    Guid ProductGroupId,
    string ChartType,
    string? CreatedBy
);
