using ConfigurationService.Application.Dtos;
using ConfigurationService.Application.Errors;
using ConfigurationService.Application.Dtos;
using ConfigurationService.Application.Queries;
using ConfigurationService.Domain.Aggregates;
using ConfigurationService.Infrastructure.Persistence;
using GssCommon.Common;
using Wolverine;

namespace ConfigurationService.Application.Handlers;

// ============ Enquiry Query Handlers ============

public class GetEnquiryByIdHandler(IEnquiryRepository repository)
{
    private readonly IEnquiryRepository _repository = repository;

    public async Task<Result<EnquiryDto?>> Handle(GetEnquiryByIdQuery request, CancellationToken cancellationToken)
    {
        var enquiry = await _repository.GetByIdAsync(request.Id);
        if (enquiry == null) return Result.Failure<EnquiryDto?>(EnquiryErrors.NotFound(request.Id)); ;

        return new EnquiryDto(
            enquiry.Id,
            enquiry.ExternalEnquiryId,
            enquiry.Name,
            enquiry.Description,
            enquiry.EnquiryNo,
            enquiry.CustomerName,
            enquiry.CustomerContact,
            enquiry.CustomerMail,
            enquiry.ProductGroup,
            enquiry.BillingDetails,
            enquiry.Source,
            enquiry.DealerId,
            enquiry.Status.ToString(),
            enquiry.Version,
            enquiry.CreatedAt,
            enquiry.CreatedBy
        );
    }
}

public class GetEnquiryByExternalIdHandler(IEnquiryRepository repository)
{
    private readonly IEnquiryRepository _repository = repository;

    public async Task<Result<EnquiryDto?>> Handle(GetEnquiryByExternalIdQuery request, CancellationToken cancellationToken)
    {
        var enquiry = await _repository.GetByExternalIdAsync(request.ExternalEnquiryId);
        if (enquiry == null) return Result.Failure<EnquiryDto?>(EnquiryErrors.ExtExists(request.ExternalEnquiryId));

        return new EnquiryDto(
            enquiry.Id,
            enquiry.ExternalEnquiryId,
            enquiry.Name,
            enquiry.Description,
            enquiry.EnquiryNo,
            enquiry.CustomerName,
            enquiry.CustomerContact,
            enquiry.CustomerMail,
            enquiry.ProductGroup,
            enquiry.BillingDetails,
            enquiry.Source,
            enquiry.DealerId,
            enquiry.Status.ToString(),
            enquiry.Version,
            enquiry.CreatedAt,
            enquiry.CreatedBy
        );
    }
}

public class GetAllEnquiriesHandler(IEnquiryRepository repository)
{
    private readonly IEnquiryRepository _repository = repository;

    public async Task<Result<IEnumerable<EnquiryDto>>> Handle(GetAllEnquiriesQuery request, CancellationToken cancellationToken)
    {
        var enquiries = await _repository.GetAllAsync(request.IncludeDeleted);


        var dtoList = (enquiries ?? [])
            .Select(e => new EnquiryDto(
                e.Id,
                e.ExternalEnquiryId,
                e.Name,
                e.Description,
                e.EnquiryNo,
                e.CustomerName,
                e.CustomerContact,
                e.CustomerMail,
                e.ProductGroup,
                e.BillingDetails,
                e.Source,
                e.DealerId,
                e.Status.ToString(),
                e.Version,
                e.CreatedAt,
                e.CreatedBy
            ));
        return Result.Success(dtoList);
    }
}

// ============ Configuration Query Handlers ============

public class GetConfigurationByIdHandler(IConfigurationRepository repository)
{
    private readonly IConfigurationRepository _repository = repository;

    public async Task<Result<ConfigurationDto?>> Handle(GetConfigurationByIdQuery request, CancellationToken cancellationToken)
    {
        var configuration = await _repository.GetByIdAsync(request.Id);
        if (configuration == null) return Result.Failure<ConfigurationDto?>(EnquiryErrors.ConfigNotFounds(request.Id));

        return new ConfigurationDto(
            configuration.Id,
            configuration.EnquiryId,
            configuration.Name,
            configuration.Description,
            configuration.IsActive,
            configuration.IsPrimary,
            configuration.CreatedAt,
            configuration.CreatedBy
        );
    }
}



public class GetConfigurationsByEnquiryIdHandler(IConfigurationRepository repository)
{
    private readonly IConfigurationRepository _repository = repository;

    public async Task<Result<IEnumerable<ConfigurationDto>>> Handle(GetConfigurationsByEnquiryIdQuery request, CancellationToken cancellationToken)
    {
        var configurations = await _repository.GetByEnquiryIdAsync(request.EnquiryId, request.IncludeInactive);

        var dtoList = configurations.Select(c => new ConfigurationDto(
            c.Id,
            c.EnquiryId,
            c.Name,
            c.Description,
            c.IsActive,
            c.IsPrimary,
            c.CreatedAt,
            c.CreatedBy

        ));
        return Result.Success(dtoList);
    }
}

public class GetConfigurationListByEnquiryIdHandler(IConfigurationRepository repository)
{
    private readonly IConfigurationRepository _repository = repository;

    public async Task<Result<EnquiryDto?>> Handle(GetConfigurationListByEnquiryIdQuery request, CancellationToken cancellationToken)
    {
        var configurations = await _repository.GetListByEnquiryIdAsync(request.EnquiryId, request.IncludeInactive);


        return Result.Success(configurations);
    }
}

public class GetConfigurationWithVersionsHandler(IConfigurationRepository repository)
{
    private readonly IConfigurationRepository _repository = repository;

    public async Task<Result<ConfigurationDto?>> Handle(GetConfigurationWithVersionsQuery request, CancellationToken cancellationToken)
    {
        var configuration = await _repository.GetByIdAsync(request.ConfigurationId);
        if (configuration == null) return Result.Failure<ConfigurationDto?>(EnquiryErrors.ConfigNotFounds(request.ConfigurationId)); ;

        var versions = configuration.Versions.Select(v => new ConfigurationVersionDto(
            v.Id,
            v.ConfigurationId,
            v.VersionNumber,
            v.Description,
            v.IsCurrent,
            v.CreatedAt,
            v.CreatedBy
        ));

        var Civil = configuration.civilLayouts.Select(v => new CivilLayoutDto(
    v.Id,
    v.ConfigurationId,
    v.WarehouseType,
    v.SourceFile,
    v.CivilJson,
    v.VersionNo,
    v.CreatedAt,
    v.CreatedBy,
    v.UpdatedAt,
    v.UpdatedBy
));
        var Racks = configuration.rackLayouts.Select(v => new RackLayoutDto(
v.Id,
v.CivilLayoutId,
v.ConfigurationVersionId,
v.RackJson,
v.ConfigurationLayout,
v.CreatedAt,
v.CreatedBy,
v.UpdatedAt,
v.UpdatedBy
));

        return new ConfigurationDto(
            configuration.Id,
            configuration.EnquiryId,
            configuration.Name,
            configuration.Description,
            configuration.IsActive,
            configuration.IsPrimary,
            configuration.CreatedAt,
            configuration.CreatedBy,
            versions,Civil, Racks
        );
    }
}

public class GetCurrentVersionHandler(IConfigurationRepository repository)
{
    private readonly IConfigurationRepository _repository = repository;

    public async Task<Result<ConfigurationVersionDto?>> Handle(GetCurrentVersionQuery request, CancellationToken cancellationToken)
    {
        var configuration = await _repository.GetByIdAsync(request.ConfigurationId);
        if (configuration == null) return Result.Failure<ConfigurationVersionDto?>(EnquiryErrors.ConfigNotFounds(request.ConfigurationId));

        var currentVersion = configuration.GetCurrentVersion();

        return new ConfigurationVersionDto(
            currentVersion.Id,
            currentVersion.ConfigurationId,
            currentVersion.VersionNumber,
            currentVersion.Description,
            currentVersion.IsCurrent,
            currentVersion.CreatedAt,
            currentVersion.CreatedBy
        );
    }
}
