using System.Text.Json;
using Microsoft.AspNetCore.Http;

namespace GssWebApi.Dto;

/// <summary>
/// Response DTO containing the DesignDeck component palette.
/// </summary>
/// <param name="PaletteVersion">[OPTIONAL] The version of the palette (default: "v1").</param>
/// <param name="Roles">[OPTIONAL] Roles allowed to access this palette.</param>
/// <param name="catalogservice">[OPTIONAL] Raw JSON data of the palette from the catalog service.</param>
public class PaletteResponse
{
    public string PaletteVersion { get; set; } = "v1";
    public IReadOnlyList<string> Roles { get; set; } = Array.Empty<string>();
    public JsonElement catalogservice { get; set; } = new();
}

// ============ Enquiry DTOs ============

public record EnquiryDto(
    Guid Id,
    string ExternalEnquiryId,
    string Name,
    string? Description,
    string? EnquiryNo,
    string? CustomerName,
    long? CustomerContact,
    string? CustomerMail,
    string? ProductGroup,
    string? BillingDetails,
    string? Source,
    Guid? DealerId,
    string Status,
    int Version,
    DateTime CreatedAt,
    string? CreatedBy,
    IEnumerable<ConfigurationDto>? Configurations = null
);

/// <summary>
/// Request DTO for creating a new enquiry.
/// </summary>
/// <param name="ExternalEnquiryId">[REQUIRED] External identifier for the enquiry.</param>
/// <param name="Name">[REQUIRED] Name of the enquiry.</param>
/// <param name="EnquiryNo">[OPTIONAL] Internal enquiry number.</param>
/// <param name="Description">[OPTIONAL] Detailed description.</param>
/// <param name="CustomerName">[OPTIONAL] Name of the customer.</param>
/// <param name="CustomerContact">[OPTIONAL] Contact number of the customer.</param>
/// <param name="CustomerMail">[OPTIONAL] Email address of the customer.</param>
/// <param name="ProductGroup">[OPTIONAL] Associated product group code.</param>
/// <param name="BillingDetails">[OPTIONAL] Billing information details.</param>
/// <param name="Source">[OPTIONAL] Source of the enquiry (e.g., 'Web', 'Referral').</param>
/// <param name="DealerId">[OPTIONAL] Unique identifier of the associated dealer.</param>
public record CreateEnquiryRequest(
    string ExternalEnquiryId,
    string Name,
    string? EnquiryNo,
    string? Description,
    string? CustomerName,
    long? CustomerContact,
    string? CustomerMail,
    string? ProductGroup,
    string? BillingDetails,
    string? Source,
    Guid? DealerId
);

/// <summary>
/// Request DTO for updating an existing enquiry.
/// </summary>
/// <param name="Name">[REQUIRED] Name of the enquiry.</param>
/// <param name="Description">[OPTIONAL] Detailed description.</param>
/// <param name="EnquiryNo">[OPTIONAL] Internal enquiry number.</param>
/// <param name="CustomerName">[OPTIONAL] Name of the customer.</param>
/// <param name="CustomerContact">[OPTIONAL] Contact number of the customer.</param>
/// <param name="CustomerMail">[OPTIONAL] Email address of the customer.</param>
/// <param name="ProductGroup">[OPTIONAL] Associated product group code.</param>
/// <param name="BillingDetails">[OPTIONAL] Billing information details.</param>
/// <param name="Source">[OPTIONAL] Source of the enquiry.</param>
/// <param name="DealerId">[OPTIONAL] Unique identifier of the associated dealer.</param>
/// <param name="Status">[OPTIONAL] Current status of the enquiry.</param>
public record UpdateEnquiryRequest(
    string Name,
    string? Description,
    string? EnquiryNo,
    string? CustomerName,
    long? CustomerContact,
    string? CustomerMail,
    string? ProductGroup,
    string? BillingDetails,
    string? Source,
    Guid? DealerId,
    string? Status
);

// ============ Configuration DTOs ============

public record ConfigurationDto(
    Guid Id,
    Guid EnquiryId,
    string Name,
    string? Description,
    bool IsActive,
    bool IsPrimary,
    DateTime CreatedAt,
    string? CreatedBy,
    IEnumerable<ConfigurationVersionDto>? Versions = null,
    IEnumerable<CivilLayoutDto>? Civil = null,
    IEnumerable<RackLayoutDto>? Racks = null
);

/// <summary>
/// Request DTO for creating a new configuration within an enquiry.
/// </summary>
/// <param name="Name">[REQUIRED] Name of the configuration.</param>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="IsPrimary">[OPTIONAL] Whether this is the primary configuration (default: false).</param>
public record EnquiryCreateConfigurationRequest(
    string Name,
    string? Description,
    bool IsPrimary = false
);

// ============ Configuration Version DTOs ============

public record ConfigurationVersionDto(
    Guid Id,
    Guid ConfigurationId,
    int VersionNumber,
    string? Description,
    bool IsCurrent,
    DateTime CreatedAt,
    string? CreatedBy
);

/// <summary>
/// Request DTO for creating a new version of a configuration.
/// </summary>
/// <param name="Description">[OPTIONAL] Version description.</param>
public record CreateVersionRequest(
    string? Description
);

// ============ Civil & Rack Layout DTOs ============

public record CivilLayoutDto(
    Guid Id,
    Guid ConfigurationId,
    Guid? WarehouseType,
    string? SourceFile,
    string? CivilJson,
    int VersionNo,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy
);

/// <summary>
/// Request DTO for saving a civil layout.
/// </summary>
/// <param name="WarehouseType">[OPTIONAL] Unique identifier of the warehouse type.</param>
/// <param name="SourceFile">[OPTIONAL] DXF/CAD source file.</param>
/// <param name="CivilJson">[OPTIONAL] JSON definition of the civil layout.</param>
public record SaveCivilLayoutRequest(
    Guid? WarehouseType,
    IFormFile? SourceFile,
    IFormFile? CivilJson
);

/// <summary>
/// Request DTO for updating a civil layout.
/// </summary>
/// <param name="WarehouseType">[OPTIONAL] Unique identifier of the warehouse type.</param>
/// <param name="SourceFile">[OPTIONAL] DXF/CAD source file.</param>
/// <param name="CivilJson">[OPTIONAL] JSON definition of the civil layout.</param>
public record UpdateCivilLayoutRequest(
    Guid? WarehouseType,
    IFormFile? SourceFile,
    IFormFile? CivilJson
);

/// <summary>
/// Request DTO for saving a rack layout.
/// </summary>
/// <param name="RackJson">[REQUIRED] JSON definition of the rack layout.</param>
public record SaveRackLayoutRequest
{
    public IFormFile? RackJson { get; set; }
    public string? configurationjson { get; set; }
};

public record RackLayoutDto(
    Guid Id,
    Guid CivilLayoutId,
    Guid ConfigurationVersionId,
    string? RackJson,
    DateTime CreatedAt,
    string? CreatedBy,
    DateTime? UpdatedAt,
    string? UpdatedBy
);

// ============ Storage Configuration DTOs ============

public record StorageConfigurationDto(
    Guid Id,
    Guid? FloorId,
    string Name,
    string? Description,
    string ProductGroup,
    JsonDocument? DesignData,
    DateTime? LastSavedAt,
    bool IsActive
);

/// <summary>
/// Request DTO for saving design data.
/// </summary>
/// <param name="DesignData">[REQUIRED] The detailed design data in JSON format.</param>
public record SaveDesignRequest(JsonDocument DesignData);

/// <summary>
/// Request DTO for creating a storage configuration.
/// </summary>
/// <param name="ConfigurationId">[REQUIRED] Unique identifier of the configuration.</param>
/// <param name="VersionNumber">[REQUIRED] Version number identifier.</param>
/// <param name="Name">[REQUIRED] Name of the storage configuration.</param>
/// <param name="ProductGroup">[REQUIRED] Associated product group code.</param>
/// <param name="Description">[OPTIONAL] Technical description.</param>
/// <param name="FloorId">[OPTIONAL] Unique identifier of the associated floor.</param>
/// <param name="DesignData">[OPTIONAL] Initial design data in JSON format.</param>
public record CreateStorageConfigurationRequest(
    Guid ConfigurationId,
    int VersionNumber,
    string Name,
    string ProductGroup,
    string? Description,
    Guid? FloorId,
    JsonDocument? DesignData
);
