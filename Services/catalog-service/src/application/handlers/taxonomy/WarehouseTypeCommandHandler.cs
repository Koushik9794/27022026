using System.Text.Json;
using System.Threading;
using CatalogService.Application.Commands.Taxonomy;
using CatalogService.Domain.Aggregates;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Clients;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;
using GssCommon.Export;

namespace CatalogService.Application.Handlers.Taxonomy;


public class CreateWarehouseTypeHandler(IWarehouseTypeRepository repository, IFileServiceClient fileServiceClient)
{
    private readonly IWarehouseTypeRepository _repository = repository;
    private readonly IFileServiceClient _fileServiceClient = fileServiceClient;



    public async Task<Result<Guid>> Handle(CreateWarehouseTypeCommand command, CancellationToken cancellationToken)
    {
        // Check for duplicate code
        if (await _repository.ExistsAsync(command.name, null, cancellationToken))
        {
            return Result.Failure<Guid>(Error.Conflict("WarehouseType.DuplicateName", $"Warehouse Type '{command.name}' already exists."));
        }


        string? IconFile = null;
        string? DXFFile = null;
        string? JsonFile = null;
        if (command.Icon is not null)
        {
            await using var stream = command.Icon.OpenReadStream();
            var uploadResult = await _fileServiceClient.UploadAsync(
                stream,
                command.Icon.FileName,
                command.Icon.ContentType ?? "application/octet-stream",
                cancellationToken
            );
            IconFile = uploadResult.FilePath;
        }
        if (command.templatePath_Civil is not null)
        {
            await using var stream = command.templatePath_Civil.OpenReadStream();
            var uploadResult = await _fileServiceClient.UploadAsync(
                stream,
                command.templatePath_Civil.FileName,
                command.templatePath_Civil.ContentType ?? "application/octet-stream",
                cancellationToken
            );
            DXFFile = uploadResult.FilePath;
        }
        if (command.templatePath_Json is not null)
        {
            await using var stream = command.templatePath_Json.OpenReadStream();
            var uploadResult = await _fileServiceClient.UploadAsync(
                stream,
                command.templatePath_Json.FileName,
                command.templatePath_Json.ContentType ?? "application/octet-stream",
                cancellationToken
            );
            JsonFile = uploadResult.FilePath;
        }
        var category = WarehouseType.Create(
            command.name,
            command.label,
            IconFile,
            command.tooltip,
           DXFFile,
            JsonFile,
            command.attributes,
            command.createdBy
        );

        return await _repository.CreateAsync(category, cancellationToken);
    }
}

public class UpdateWarehouseTypeHandler(IWarehouseTypeRepository repository, IFileServiceClient fileServiceClient)
{
    private readonly IWarehouseTypeRepository _repository = repository;
    private readonly IFileServiceClient _fileServiceClient = fileServiceClient;

    public async Task<Result<bool>> Handle(UpdateWarehouseTypeCommand command, CancellationToken cancellationToken)
    {
        var warehouseType = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (warehouseType == null) return Result.Failure<bool>(Error.NotFound("WarehouseType.NotFound", "Warehouse Type not found."));

        if (await _repository.ExistsAsync(command.name, command.Id, cancellationToken))
        {
            return Result.Failure<bool>(Error.Conflict("WarehouseType.DuplicateName", $"Warehouse Type '{command.name}' already exists."));
        }
        string? IconFile = null;
        string? DXFFile = null;
        string? JsonFile = null;
        if (command.Icon is not null)
        {
            await using var stream = command.Icon.OpenReadStream();
            var uploadResult = await _fileServiceClient.UploadAsync(
                stream,
                command.Icon.FileName,
                command.Icon.ContentType ?? "application/octet-stream",
                cancellationToken
            );
            IconFile = uploadResult.FilePath;
        }
        if (command.templatePath_Civil is not null)
        {
            await using var stream = command.templatePath_Civil.OpenReadStream();
            var uploadResult = await _fileServiceClient.UploadAsync(
                stream,
                command.templatePath_Civil.FileName,
                command.templatePath_Civil.ContentType ?? "application/octet-stream",
                cancellationToken
            );
            DXFFile = uploadResult.FilePath;
        }
        if (command.templatePath_Json is not null)
        {
            await using var stream = command.templatePath_Json.OpenReadStream();
            var uploadResult = await _fileServiceClient.UploadAsync(
                stream,
                command.templatePath_Json.FileName,
                command.templatePath_Json.ContentType ?? "application/octet-stream",
                cancellationToken
            );
            JsonFile = uploadResult.FilePath;
        }

        warehouseType.Update(command.name,
            command.label,
            IconFile,
            command.tooltip,
            DXFFile,
             JsonFile,
            command.attributes,
            command.IsActive,
            command.UpdateddBy);

        return await _repository.UpdateAsync(warehouseType, cancellationToken);

    }
}

public class DeleteWarehouseTypeHandler
{
    private readonly IWarehouseTypeRepository _repository;

    public DeleteWarehouseTypeHandler(IWarehouseTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(DeleteWarehouseTypeCommand command, CancellationToken cancellationToken)
    {
        var warehouseType = await _repository.GetByIdAsync(command.Id, cancellationToken);
        if (warehouseType == null) return Result.Failure<bool>(Error.NotFound("WarehouseType.NotFound", "Warehouse Type not found."));
        return await _repository.Delete(command.Id, cancellationToken);
    }
}
