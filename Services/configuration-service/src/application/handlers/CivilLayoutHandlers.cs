using System.Text.Json;
using System.Diagnostics;
using ConfigurationService.Application.Abstractions;
using ConfigurationService.Application.Commands;
using ConfigurationService.Application.Dtos;
using ConfigurationService.Application.Queries;
using ConfigurationService.Domain.Aggregates;
using ConfigurationService.Infrastructure.Persistence;
using ConfigurationService.Infrastructure.Clients;

namespace ConfigurationService.Application.Handlers;

// ============ Civil Layout Handlers ============

public class SaveCivilLayoutHandler
{
    private readonly ICivilLayoutRepository _repository;
    private readonly IFileServiceClient _fileServiceClient;

    public SaveCivilLayoutHandler(ICivilLayoutRepository repository, IFileServiceClient fileServiceClient)
    {
        _repository = repository;
        _fileServiceClient = fileServiceClient;
    }

    public async Task<int> Handle(SaveCivilLayoutCommand request)
    {

        string? SourceFile = null;
        string? CivilJson = null;


        if (request.SourceFile is not null)
        {
            await using var stream = request.SourceFile.OpenReadStream();
            var uploadResult = await _fileServiceClient.UploadAsync(
                stream,
                request.SourceFile.FileName,
                request.SourceFile.ContentType ?? "application/octet-stream"
                );

            SourceFile = uploadResult.FilePath;
        }
        if (request.CivilJson is not null)
        {
            await using var stream = request.CivilJson.OpenReadStream();
            var uploadResult = await _fileServiceClient.UploadAsync(
                stream,
                request.CivilJson.FileName,
                request.CivilJson.ContentType ?? "application/octet-stream"
                );

            CivilJson = uploadResult.FilePath;
        }


        var civilLayout = CivilLayout.Create(
            request.ConfigurationId,
            request.WarehouseType,
            SourceFile,
            CivilJson,

            request.UpdatedBy
        );
       var VersionNo= await _repository.CreateCivilLayoutAsync(civilLayout);
        return VersionNo;

    }
}

public class UpdateCivilLayoutHandler
{
    private readonly ICivilLayoutRepository _repository;
    private readonly IFileServiceClient _fileServiceClient;

    public UpdateCivilLayoutHandler(ICivilLayoutRepository repository, IFileServiceClient fileServiceClient)
    {
        _repository = repository;
        _fileServiceClient = fileServiceClient;
    }

    public async Task<Guid> Handle(UpdateCivilLayoutCommand request)
    {
        var existing = await _repository.GetCivilLayoutByIdAsync(request.Id);
        string? SourceFile = null;
        string? CivilJson = null;


        if (request.SourceFile is not null)
        {
            await using var stream = request.SourceFile.OpenReadStream();
            var uploadResult = await _fileServiceClient.UploadAsync(
                stream,
                request.SourceFile.FileName,
                request.SourceFile.ContentType ?? "application/octet-stream"
                );

            SourceFile = uploadResult.FilePath;
        }
        if (request.CivilJson is not null)
        {
            await using var stream = request.CivilJson.OpenReadStream();
            var uploadResult = await _fileServiceClient.UploadAsync(
                stream,
                request.CivilJson.FileName,
                request.CivilJson.ContentType ?? "application/octet-stream"
                );

            CivilJson = uploadResult.FilePath;
        }

        existing.Update(request.WarehouseType, SourceFile, CivilJson, request.UpdatedBy);
        await _repository.UpsertCivilLayoutAsync(existing);
        return existing.Id;

    }
}
public class GetCivilLayoutByConfigIdHandler
{
    private readonly ICivilLayoutRepository _repository;

    public GetCivilLayoutByConfigIdHandler(ICivilLayoutRepository repository)
    {
        _repository = repository;
    }

    public async Task<ConfigurationDto> Handle(GetCivilLayoutByConfigIdQuery request)
    {
        var civilLayout = await _repository.GetCivilLayoutByConfigurationIdAsync(request.ConfigurationId);

        if (civilLayout == null) return null;

        return civilLayout;
    }
}


public class GetCivilLayoutByIdHandler
{
    private readonly ICivilLayoutRepository _repository;

    public GetCivilLayoutByIdHandler(ICivilLayoutRepository repository)
    {
        _repository = repository;
    }

    public async Task<CivilLayoutDto?> Handle(GetCivilLayoutByIdQuery request)
    {
        var civilLayout = await _repository.GetCivilLayoutByIdAsync(request.Id);

        if (civilLayout == null) return null;


        return new CivilLayoutDto(
            civilLayout.Id,
            civilLayout.ConfigurationId,
            civilLayout.WarehouseType,
            civilLayout.SourceFile,
            civilLayout.CivilJson,
            civilLayout.VersionNo,
            civilLayout.CreatedAt,
            civilLayout.CreatedBy,
            civilLayout.UpdatedAt,
            civilLayout.UpdatedBy
        );
    }
}
// ============ Rack Layout Handlers ============

public class SaveRackLayoutHandler
{
    private readonly ICivilLayoutRepository _repository;
    private readonly IFileServiceClient _fileServiceClient;

    public SaveRackLayoutHandler(ICivilLayoutRepository repository, IFileServiceClient fileServiceClient)
    {
        _repository = repository;
        _fileServiceClient = fileServiceClient;
    }



    public async Task<Guid> Handle(SaveRackLayoutCommand request)
    {
        var Config_revision_Id = await _repository.GetRevisionIdAsync(request.ConfigurationId, request.Configversion, request.Civilversion);


        string? RackJson = null;


        if (request.RackJson is not null)
        {
            await using var stream = request.RackJson.OpenReadStream();
            var uploadResult = await _fileServiceClient.UploadAsync(
                stream,
                request.RackJson.FileName,
                request.RackJson.ContentType ?? "application/octet-stream"
                );

            RackJson = uploadResult.FilePath;
        }

        var rackLayout = RackLayout.Create(
            Config_revision_Id.CivilLayoutId,
           Config_revision_Id.ConfigurationVersionId,
            RackJson,
            request.ConfigurationLayout,
            request.UpdatedBy
        );
        await _repository.CreateRackLayoutAsync(rackLayout);
        return rackLayout.Id;

    }
}

public class UpdateRackLayoutHandler
{
    private readonly ICivilLayoutRepository _repository;
    private readonly IFileServiceClient _fileServiceClient;

    public UpdateRackLayoutHandler(ICivilLayoutRepository repository, IFileServiceClient fileServiceClient)
    {
        _repository = repository;
        _fileServiceClient = fileServiceClient;
    }



    public async Task<Guid> Handle(UpdateRackLayoutCommand request)
    {
        var existing = await _repository.GetRackLayoutByIdAsync(request.Id);
        string? RackJson = null;


        if (request.RackJson is not null)
        {
            await using var stream = request.RackJson.OpenReadStream();
            var uploadResult = await _fileServiceClient.UploadAsync(
                stream,
                request.RackJson.FileName,
                request.RackJson.ContentType ?? "application/octet-stream"
                );

            RackJson = uploadResult.FilePath;
        }

        existing.Update(RackJson, request.ConfigurationLayout, request.UpdatedBy);
        await _repository.UpdateRackLayoutAsync(existing);
        return request.Id;

    }
}
public class GetRackLayoutByIdHandler
{
    private readonly ICivilLayoutRepository _repository;

    public GetRackLayoutByIdHandler(ICivilLayoutRepository repository)
    {
        _repository = repository;
    }

    public async Task<RackLayoutDto?> Handle(GetRackLayoutByIdQuery request)
    {
        var rackLayout = await _repository.GetRackLayoutByIdAsync(request.Id);
        if (rackLayout == null) return null;

        return new RackLayoutDto(
            rackLayout.Id,
            rackLayout.CivilLayoutId,
            rackLayout.ConfigurationVersionId,
            rackLayout.RackJson,
            rackLayout.ConfigurationLayout,
            rackLayout.CreatedAt,
            rackLayout.CreatedBy,
            rackLayout.UpdatedAt,
            rackLayout.UpdatedBy
        );
    }
}

public class GetRackLayoutByVersionHandler
{
    private readonly ICivilLayoutRepository _repository;

    public GetRackLayoutByVersionHandler(ICivilLayoutRepository repository)
    {
        _repository = repository;
    }

    public async Task<ConfigurationDto?> Handle(GetRackLayoutByVersionIdQuery request)
    {
        var configuration = await _repository.GetRackLayoutByVersionIdAsync(request.ConfigurationId, request.Version);
        if (configuration == null) return null;
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
            versions, Civil, Racks
        );
    }

}
