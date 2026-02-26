using ConfigurationService.Application.Commands;
using ConfigurationService.Application.Dtos;
using ConfigurationService.Application.Queries;
using ConfigurationService.application.errors;
using ConfigurationService.Domain.Aggregates;
using ConfigurationService.Infrastructure.Persistence;
using GssCommon.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ConfigurationService.Application.Handlers;

public class CreateRackConfigurationHandler(IRackConfigurationRepository repository, IEnquiryRepository enquiryRepository)
{
    private readonly IRackConfigurationRepository _repository = repository;
    private readonly IEnquiryRepository _enquiryRepository = enquiryRepository;

    public async Task<Result<Guid>> Handle(CreateRackConfigurationCommand request, CancellationToken cancellationToken)
    {
        Guid? enquiryGuid = null;
        if (!string.IsNullOrWhiteSpace(request.EnquiryId))
        {
            if (!Guid.TryParse(request.EnquiryId, out var g))
                return Result.Failure<Guid>(RackConfigurationErrors.InvalidEnquiryId(request.EnquiryId));
            
            if (!await _enquiryRepository.ExistsAsync(g))
                return Result.Failure<Guid>(RackConfigurationErrors.EnquiryNotFound(g));
            
            enquiryGuid = g;
        }

        var config = RackConfiguration.Create(
            request.Name,
            request.ConfigurationLayout,
            request.ProductCode,
            request.Scope,
            enquiryGuid,
            request.CreatedBy,
            request.IsAdmin
        );

        await _repository.AddAsync(config);
        
        return Result.Success(config.Id);
    }
}

public class UpdateRackConfigurationHandler(IRackConfigurationRepository repository, IEnquiryRepository enquiryRepository)
{
    private readonly IRackConfigurationRepository _repository = repository;
    private readonly IEnquiryRepository _enquiryRepository = enquiryRepository;

    public async Task<Result<Guid>> Handle(UpdateRackConfigurationCommand request, CancellationToken cancellationToken)
    {
        Guid? enquiryGuid = null;
        if (!string.IsNullOrWhiteSpace(request.EnquiryId))
        {
            if (!Guid.TryParse(request.EnquiryId, out var g))
                return Result.Failure<Guid>(RackConfigurationErrors.InvalidEnquiryId(request.EnquiryId));
            
            if (!await _enquiryRepository.ExistsAsync(g))
                return Result.Failure<Guid>(RackConfigurationErrors.EnquiryNotFound(g));
            
            enquiryGuid = g;
        }

        var config = await _repository.GetByIdAsync(request.Id);
        if (config == null) return Result.Failure<Guid>(RackConfigurationErrors.NotFound(request.Id));

        config.Update(
            request.Name,
            request.ConfigurationLayout,
            request.ProductCode,
            request.Scope,
            enquiryGuid,
            request.UpdatedBy,
            request.IsAdmin
        );

        await _repository.UpdateAsync(config);
        
        return Result.Success(config.Id);
    }
}

public class DeleteRackConfigurationHandler(IRackConfigurationRepository repository)
{
    private readonly IRackConfigurationRepository _repository = repository;

    public async Task<Result<Guid>> Handle(DeleteRackConfigurationCommand request, CancellationToken cancellationToken)
    {
        var config = await _repository.GetByIdAsync(request.Id);
        if (config == null) return Result.Failure<Guid>(RackConfigurationErrors.NotFound(request.Id));

        config.Deactivate(request.UpdatedBy);
        await _repository.DeleteAsync(request.Id, request.UpdatedBy);
        
        return Result.Success(config.Id);
    }
}

public class GetRackConfigurationHandler(IRackConfigurationRepository repository)
{
    private readonly IRackConfigurationRepository _repository = repository;

    public async Task<Result<RackConfigurationResponse>> Handle(GetRackConfigurationByIdQuery request, CancellationToken cancellationToken)
    {
        var config = await _repository.GetByIdAsync(request.Id);
        if (config == null) return Result.Failure<RackConfigurationResponse>(RackConfigurationErrors.NotFound(request.Id));

        return Result.Success(MapToResponse(config));
    }

    public async Task<Result<IEnumerable<RackConfigurationResponse>>> Handle(ListRackConfigurationsQuery request, CancellationToken cancellationToken)
    {
        IEnumerable<RackConfiguration> configs;
        if (request.EnquiryId.HasValue)
        {
            configs = await _repository.GetByEnquiryIdAsync(request.EnquiryId.Value, request.IncludeInactive);
        }
        else
        {
            configs = await _repository.GetAllAsync(request.IncludeInactive);
        }

        return Result.Success(configs.Select(MapToResponse));
    }

    private RackConfigurationResponse MapToResponse(RackConfiguration config)
    {
        return new RackConfigurationResponse(
            config.Id,
            config.Name,
            config.Scope,
            config.EnquiryId,
            config.IsApprovedByAdmin,
            config.IsActive,
            config.CreatedOn,
            config.ConfigurationLayout,
            config.CreatedBy ?? "system"
        );
    }
}
