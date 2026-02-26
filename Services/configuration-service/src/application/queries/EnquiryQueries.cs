using ConfigurationService.Application.Dtos;


namespace ConfigurationService.Application.Queries;

// ============ Enquiry Queries ============

public record GetEnquiryByIdQuery(Guid Id);

public record GetEnquiryByExternalIdQuery(string ExternalEnquiryId);

public record GetAllEnquiriesQuery(bool IncludeDeleted = false) ;

// ============ Configuration Queries ============

public record GetConfigurationByIdQuery(Guid Id) ;

public record GetConfigurationsByEnquiryIdQuery(
    Guid EnquiryId,
    bool IncludeInactive = false
);

public record GetConfigurationListByEnquiryIdQuery(
    Guid EnquiryId,
    bool IncludeInactive = false
);
// ============ Configuration Version Queries ============

public record GetConfigurationWithVersionsQuery(Guid ConfigurationId) ;

public record GetCurrentVersionQuery(Guid ConfigurationId) ;
