using System.Text.Json;
using CatalogService.Application.Commands.Taxonomy;
using CatalogService.Application.Errors;
using CatalogService.Domain.Entities;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;
using GssCommon.Export;
using JasperFx.Events.Daemon;

namespace CatalogService.Application.Handlers.Taxonomy;

public class CreateCivilComponentCommandHandler(ICivilComponentRepository repository, IFileServiceClient fileServiceClient)
{
    private readonly ICivilComponentRepository _repository = repository;
    private readonly IFileServiceClient _fileServiceClient = fileServiceClient;
    public async Task<Result<Guid>> Handle(CreateCivilComponentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (await repository.ExistsAsync(request.Code, null, cancellationToken))
            {

                return Result.Failure<Guid>(CivilcomponentErrors.CodeExists($"Category with code '{request.Code}' already exists."));
            }
            string? IconFile = null;
            if (request.Icon is not null)
            {
                await using var stream = request.Icon.OpenReadStream();
                var uploadResult = await _fileServiceClient.UploadAsync(
                    stream,
                    request.Icon.FileName,
                    request.Icon.ContentType ?? "application/octet-stream",
                    cancellationToken
                );
                IconFile = uploadResult.FilePath;
            }
            var component = CivilComponent.Create(
                request.Code,
                request.Name,
                request.Label,
               IconFile,
                request.Tooltip,
                request.Category,
                request.DefaultElement,
                request.CreatedBy);
            await _repository.CreateAsync(component, cancellationToken);
            return Result<Guid>.Success(component.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(CivilcomponentErrors.CreateFailed(ex.Message));
        }
    }
}

public class UpdateCivilComponentCommandHandler(ICivilComponentRepository repository, IFileServiceClient fileServiceClient)
{
    private readonly ICivilComponentRepository _repository = repository;
    private readonly IFileServiceClient _fileServiceClient = fileServiceClient;
    public async Task<Result<bool>> Handle(UpdateCivilComponentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (await repository.ExistsAsync(request.Code, request.Id, cancellationToken))
            {

                return Result.Failure<bool>(CivilcomponentErrors.CodeExists($"Category with code '{request.Code}' already exists."));
            }
            string? IconFile = null;
            if (request.Icon is not null)
            {
                await using var stream = request.Icon.OpenReadStream();
                var uploadResult = await _fileServiceClient.UploadAsync(
                    stream,
                    request.Icon.FileName,
                    request.Icon.ContentType ?? "application/octet-stream",
                    cancellationToken
                );
                IconFile = uploadResult.FilePath;
            }
            var component = CivilComponent.Update(
                request.Id,
                request.Code,
                request.Name,
                request.Label,
               IconFile,
                request.Tooltip,
                request.Category,
                request.DefaultElement,
                request.IsActive,
                request.updatedBy);
            await repository.UpdateAsync(component, cancellationToken);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(CivilcomponentErrors.CreateFailed(ex.Message));
        }
    }
}

public class DeleteCivilComponentCommandHandler(ICivilComponentRepository repository)
{
    public async Task<Result<bool>> Handle(DeleteCivilComponentCommand request, CancellationToken cancellationToken)
    {
        try
        {
            await repository.DeleteAsync(request.Id, cancellationToken);
            return Result.Success(true);
        }
        catch (Exception ex)
        {
            return Result.Failure<bool>(CivilcomponentErrors.CreateFailed(ex.Message));
        }
    }
}
