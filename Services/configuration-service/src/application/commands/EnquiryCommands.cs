using System.Text.Json;


namespace ConfigurationService.Application.Commands;

// ============ Enquiry Commands ============

public record CreateEnquiryCommand(
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
    string? CreatedBy
);

public record UpdateEnquiryCommand(
    Guid Id,
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
    string? UpdatedBy
);

public record UpdateEnquiryStatusCommand(
    Guid Id,
    string Status,
    string? UpdatedBy
);

public record DeleteEnquiryCommand(
    Guid Id,
    string? DeletedBy
);

// ============ Configuration Commands ============

public record CreateConfigurationCommand(
    Guid EnquiryId,
    string Name,
    string? Description,
    bool IsPrimary,
    string? CreatedBy
);

public record UpdateConfigurationCommand(
    Guid Id,
    string Name,
    string? Description,
    string? UpdatedBy
);

public record SetPrimaryConfigurationCommand(
    Guid Id,
    string? UpdatedBy
);

public record DeleteConfigurationCommand(
    Guid Id,
    string? DeletedBy
);

// ============ Configuration Version Commands ============

public record CreateConfigurationVersionCommand(
    Guid ConfigurationId,
    string? Description,
    string? CreatedBy
);
public record LockVersionCommand(
    Guid EnquiryId,
     Guid ConfigId,
     int versionNumber,
     string? UpdatedBy,
     bool isLocked=true
);

public record UnLockVersionCommand(
    Guid EnquiryId,
     Guid ConfigId,
     int versionNumber,
     string? UpdatedBy,
     bool isLocked = false
);

public record SetCurrentVersionCommand(
    Guid ConfigurationId,
    int VersionNumber,
    string? UpdatedBy
);

// ============ SKU Commands (through ConfigurationVersion) ============

public record AddSkuToVersionCommand(
    Guid ConfigurationId,
    int VersionNumber,
    string Code,
    string Name,
    Guid? SkuTypeId,
    string? Description,
    JsonDocument? Attributes,
    int? UnitsPerLayer,
    int? LayersPerPallet,
    string? CreatedBy
);

// ============ Pallet Commands (through ConfigurationVersion) ============

public record AddPalletToVersionCommand(
    Guid ConfigurationId,
    int VersionNumber,
    string Code,
    string Name,
    Guid? PalletTypeId,
    string? Description,
    JsonDocument? Attributes,
    string? CreatedBy
);

// ============ Storage Configuration Commands ============

/// <summary>
/// Creates a new storage configuration for a specific floor.
/// </summary>
public record AddStorageConfigurationCommand(
    Guid ConfigurationId,
    int VersionNumber,
    string Name,
    string ProductGroup,
    string? Description,
    Guid? FloorId,
    JsonDocument? DesignData,
    string? CreatedBy
);

/// <summary>
/// Autosave command - updates the design data for an existing storage configuration.
/// Called frequently from UI as designer makes changes.
/// </summary>
public record SaveDesignCommand(
    Guid StorageConfigurationId,
    JsonDocument DesignData,
    string? UpdatedBy
);

// ============ MHE Configuration Commands ============

public record AddMheConfigCommand(
    Guid ConfigurationId,
    int VersionNumber,
    string Name,
    Guid? MheTypeId,
    string? Description,
    JsonDocument? Attributes,
    string? CreatedBy
);
