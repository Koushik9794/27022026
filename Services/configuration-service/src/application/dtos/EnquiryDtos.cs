using System.Text.Json;
using ConfigurationService.Application.Dtos;

namespace ConfigurationService.Application.Dtos;

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
    IEnumerable<ConfigurationDto> Configurations = null
);

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





public record CreateConfigurationRequest(
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

public record CreateVersionRequest(
    string? Description
);

// ============ ConfigurationSku DTOs ============

public record ConfigurationSkuDto(
    Guid Id,
    Guid? SkuTypeId,
    string Code,
    string Name,
    string? Description,
    JsonDocument? Attributes,
    int? UnitsPerLayer,
    int? LayersPerPallet,
    bool IsActive
);

public record CreateConfigurationSkuRequest(
    string Code,
    string Name,
    Guid? SkuTypeId,
    string? Description,
    JsonDocument? Attributes,
    int? UnitsPerLayer,
    int? LayersPerPallet
);

// ============ ConfigurationPallet DTOs ============

public record ConfigurationPalletDto(
    Guid Id,
    Guid? PalletTypeId,
    string Code,
    string Name,
    string? Description,
    JsonDocument? Attributes,
    bool IsActive
);

public record CreateConfigurationPalletRequest(
    string Code,
    string Name,
    Guid? PalletTypeId,
    string? Description,
    JsonDocument? Attributes
);

// ============ MheDto DTOs ============

public record MheDto(
    Guid Id,
    Guid? MheTypeId,
    string Name,
    string? Description,
    JsonDocument? Attributes,
    bool IsActive
);

public record CreateMheRequest(
    string Name,
    Guid? MheTypeId,
    string? Description,
    JsonDocument? Attributes
);

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
