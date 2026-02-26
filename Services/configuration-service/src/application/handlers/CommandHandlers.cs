using ConfigurationService.Application.Errors;
using ConfigurationService.Application.Commands;
using ConfigurationService.Domain.Aggregates;
using ConfigurationService.Domain.Enums;
using ConfigurationService.Infrastructure.Persistence;
using GssCommon.Common;
namespace ConfigurationService.Application.Handlers;

// ============ Enquiry Command Handlers ============

public class CreateEnquiryHandler(IEnquiryRepository repository)
{
    private readonly IEnquiryRepository _repository = repository;

    public async Task<Result<Guid>> Handle(CreateEnquiryCommand request, CancellationToken cancellationToken)
    {
        if (await _repository.ExternalIdExistsAsync(request.ExternalEnquiryId))
        {
            return Result.Failure<Guid>(EnquiryErrors.ExtExists(request.ExternalEnquiryId));

        }
        //if (await _repository.ExistsEnquiryNoAsync(request.EnquiryNo))
        //{
        //    return Result.Failure<Guid>(EnquiryErrors.NoExists(request.EnquiryNo));

        //}

        var enquiry = Enquiry.Create(
            request.ExternalEnquiryId,
            request.Name,
            request.Description,
            request.EnquiryNo,
            request.CustomerName,
            request.CustomerContact,
            request.CustomerMail,
            request.ProductGroup,
            request.BillingDetails,
            request.Source,
            request.DealerId,
            request.CreatedBy
        );

        await _repository.CreateAsync(enquiry);
        return Result<Guid>.Success(enquiry.Id);
    }
}

public class UpdateEnquiryHandler(IEnquiryRepository repository)
{
    private readonly IEnquiryRepository _repository = repository;

    public async Task<Result<bool>> Handle(UpdateEnquiryCommand request, CancellationToken cancellationToken)
    {
        var enquiry = await _repository.GetByIdAsync(request.Id);
        if (enquiry == null)   return Result.Failure<bool>(EnquiryErrors.NoExists(request.EnquiryNo)); 

        enquiry.Update(request.Name, request.Description, request.EnquiryNo, request.CustomerName, request.CustomerContact, request.CustomerMail, request.ProductGroup, request.BillingDetails, request.Source, request.DealerId, request.UpdatedBy);
        await _repository.UpdateAsync(enquiry);
        return Result<bool>.Success(true);
    }
}

public class UpdateEnquiryStatusHandler(IEnquiryRepository repository)
{
    private readonly IEnquiryRepository _repository = repository;

    public async Task<Result<bool>> Handle(UpdateEnquiryStatusCommand request, CancellationToken cancellationToken)
    {
        var enquiry = await _repository.GetByIdAsync(request.Id);
        if (enquiry == null) return Result.Failure<bool>(EnquiryErrors.NotFound(request.Id)); ;

        if (!Enum.TryParse<EnquiryStatus>(request.Status, true, out var status))
        {
            return Result.Failure<bool>(EnquiryErrors.InvalidStatus(request.Status));
           
        }

        enquiry.UpdateStatus(status, request.UpdatedBy);
        await _repository.UpdateAsync(enquiry);
        return Result<bool>.Success(true);
    }
}

public class DeleteEnquiryHandler(IEnquiryRepository repository)
{
    private readonly IEnquiryRepository _repository = repository;

    public async Task<Result<bool>> Handle(DeleteEnquiryCommand request, CancellationToken cancellationToken)
    {
        var enquiry = await _repository.GetByIdAsync(request.Id);
        if (enquiry == null) return Result.Failure<bool>(EnquiryErrors.NotFound(request.Id));

        enquiry.Delete(request.DeletedBy);
        await _repository.UpdateAsync(enquiry);
        return Result<bool>.Success(true);
    }
}

// ============ Configuration Command Handlers ============

public class CreateConfigurationHandler(IConfigurationRepository repository, IEnquiryRepository enquiryRepository)
{
    private readonly IConfigurationRepository _repository = repository;
    private readonly IEnquiryRepository _enquiryRepository = enquiryRepository;

    public async Task<Result<Guid>> Handle(CreateConfigurationCommand request, CancellationToken cancellationToken)
    {
        if (!await _enquiryRepository.ExistsAsync(request.EnquiryId))
        {
            return Result.Failure<Guid>(EnquiryErrors.NotFound(request.EnquiryId));
        }

        // If this is set as primary, clear any existing primary
        if (request.IsPrimary)
        {
            await _repository.ClearPrimaryForEnquiryAsync(request.EnquiryId);
        }

        var configuration = Configuration.Create(
            request.EnquiryId,
            request.Name,
            request.Description,
            request.IsPrimary,
            request.CreatedBy
        );

        await _repository.CreateAsync(configuration);
        return Result<Guid>.Success(configuration.Id);

    }
}

public class UpdateConfigurationHandler(IConfigurationRepository repository)
{
    private readonly IConfigurationRepository _repository = repository;

    public async Task<Result<bool>> Handle(UpdateConfigurationCommand request, CancellationToken cancellationToken)
    {
        var configuration = await _repository.GetByIdAsync(request.Id);
        if (configuration == null) return Result.Failure<bool>(EnquiryErrors.ConfigNotFounds(request.Id)); ;

        configuration.Update(request.Name, request.Description, request.UpdatedBy);
        await _repository.UpdateAsync(configuration);
        return Result<bool>.Success(true);
    }
}

public class LockConfigurationHandler(IConfigurationRepository repository, IEnquiryRepository enquiryRepository)
{
    private readonly IConfigurationRepository _repository = repository;
    private readonly IEnquiryRepository _enquiryRepository = enquiryRepository;

    public async Task<Result<Guid>> Handle(LockVersionCommand request, CancellationToken cancellationToken)
    {
        if (!await _enquiryRepository.ExistsAsync(request.EnquiryId))
        {
            return Result.Failure<Guid>(EnquiryErrors.NotFound(request.EnquiryId));
        }

        var result = await _repository.SetversionLockAsync(request.ConfigId, request.versionNumber, request.isLocked);
           return Result<Guid>.Success(request.ConfigId);

    }
}

public class UnLockConfigurationHandler(IConfigurationRepository repository, IEnquiryRepository enquiryRepository)
{
    private readonly IConfigurationRepository _repository = repository;
    private readonly IEnquiryRepository _enquiryRepository = enquiryRepository;

    public async Task<Result<Guid>> Handle(UnLockVersionCommand request, CancellationToken cancellationToken)
    {
        if (!await _enquiryRepository.ExistsAsync(request.EnquiryId))
        {
            return Result.Failure<Guid>(EnquiryErrors.NotFound(request.EnquiryId));
        }

        var result = await _repository.SetversionLockAsync(request.ConfigId, request.versionNumber, request.isLocked);
        return Result<Guid>.Success(request.ConfigId);

    }
}

public class SetPrimaryConfigurationHandler(IConfigurationRepository repository)
{
    private readonly IConfigurationRepository _repository = repository;

    public async Task<Result<bool>> Handle(SetPrimaryConfigurationCommand request, CancellationToken cancellationToken)
    {
        var configuration = await _repository.GetByIdAsync(request.Id);
        if (configuration == null) return Result.Failure<bool>(EnquiryErrors.ConfigNotFounds(request.Id));

        // Clear other primaries for this enquiry
        await _repository.ClearPrimaryForEnquiryAsync(configuration.EnquiryId);

        configuration.SetAsPrimary(request.UpdatedBy);
        await _repository.UpdateAsync(configuration);
        return Result<bool>.Success(true);
    }
}

public class DeleteConfigurationHandler(IConfigurationRepository repository)
{
    private readonly IConfigurationRepository _repository = repository;

    public async Task<Result<bool>> Handle(DeleteConfigurationCommand request, CancellationToken cancellationToken)
    {
        var configuration = await _repository.GetByIdAsync(request.Id);
        if (configuration == null) return Result.Failure<bool>(EnquiryErrors.ConfigNotFounds(request.Id));

        configuration.Deactivate(request.DeletedBy);
        await _repository.UpdateAsync(configuration);
        return Result<bool>.Success(true);
    }
}

// ============ Configuration Version Commands ============

public class CreateConfigurationVersionHandler(IConfigurationRepository repository)
{
    private readonly IConfigurationRepository _repository = repository;

    public async Task<Result<int>> Handle(CreateConfigurationVersionCommand request, CancellationToken cancellationToken)
    {
        var configuration = await _repository.GetByIdAsync(request.ConfigurationId);
        if (configuration == null)
            return Result.Failure<int>(EnquiryErrors.ConfigNotFounds(request.ConfigurationId));

        var newVersion = configuration.CreateNewVersion(request.Description, request.CreatedBy);
        await _repository.UpdateAsync(configuration);
        return Result<int>.Success(newVersion.VersionNumber);
    }
}

public class SetCurrentVersionHandler(IConfigurationRepository repository)
{
    private readonly IConfigurationRepository _repository = repository;

    public async Task<Result<bool>> Handle(SetCurrentVersionCommand request, CancellationToken cancellationToken)
    {
        var configuration = await _repository.GetByIdAsync(request.ConfigurationId);
        if (configuration == null) return Result.Failure<bool>(EnquiryErrors.ConfigNotFounds(request.ConfigurationId)); 

        configuration.SetCurrentVersion(request.VersionNumber, request.UpdatedBy);
        await _repository.UpdateAsync(configuration);
        return Result<bool>.Success(true);
    }
}

// ============ Storage Configuration Handlers ============

public class AddStorageConfigurationHandler(IConfigurationRepository repository)
{
    private readonly IConfigurationRepository _repository = repository;

    public async Task<Result<Guid>> Handle(AddStorageConfigurationCommand request, CancellationToken cancellationToken)
    {
        var configuration = await _repository.GetByIdAsync(request.ConfigurationId);
        if (configuration == null)
            return Result.Failure<Guid>(EnquiryErrors.ConfigNotFounds(request.ConfigurationId));

        var version = configuration.GetVersion(request.VersionNumber);
        if (version == null)
            return Result.Failure<Guid>(EnquiryErrors.VersionNotFounds(request.VersionNumber));

        var storageConfig = version.AddStorageConfiguration(
            request.Name,
            request.ProductGroup,
            request.Description,
            request.FloorId,
            request.DesignData,
            request.CreatedBy
        );

        await _repository.AddStorageConfigurationAsync(storageConfig);
        return Result<Guid>.Success(storageConfig.Id);
    }
}

/// <summary>
/// Autosave handler - updates design data for an existing storage configuration.
/// Optimized for frequent calls from UI.
/// </summary>
public class SaveDesignHandler(IConfigurationRepository repository)
{
    private readonly IConfigurationRepository _repository = repository;

    public async Task<Result<bool>> Handle(SaveDesignCommand request, CancellationToken cancellationToken)
    {
        var storageConfig = await _repository.GetStorageConfigurationByIdAsync(request.StorageConfigurationId);
        if (storageConfig == null) return Result.Failure<bool>(EnquiryErrors.ConfigNotFounds(request.StorageConfigurationId)); ;

        storageConfig.UpdateDesignData(request.DesignData, request.UpdatedBy);
        await _repository.UpdateStorageConfigurationAsync(storageConfig);
        return Result<bool>.Success(true);
    }
}

// ============ MHE Configuration Handlers ============

public class AddMheConfigHandler(IConfigurationRepository repository)
{
    private readonly IConfigurationRepository _repository = repository;

    public async Task<Result<Guid>> Handle(AddMheConfigCommand request, CancellationToken cancellationToken)
    {
        var configuration = await _repository.GetByIdAsync(request.ConfigurationId);
        if (configuration == null)
            return Result.Failure<Guid>(EnquiryErrors.ConfigNotFounds(request.ConfigurationId));

        var version = configuration.GetVersion(request.VersionNumber);
        if (version == null)
            return Result.Failure<Guid>(EnquiryErrors.VersionNotFounds(request.VersionNumber));

        var mheConfig = version.AddMheConfig(
            request.Name,
            request.MheTypeId,
            request.Description,
            request.Attributes,
            request.CreatedBy
        );

        await _repository.AddMheConfigAsync(mheConfig);
        return Result<Guid>.Success(mheConfig.Id);
    }
}
