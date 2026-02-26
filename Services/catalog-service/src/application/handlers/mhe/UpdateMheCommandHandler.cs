using System.Text.Json;
using CatalogService.Application.commands.Mhe;
using CatalogService.Application.Errors;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.Mhe;

public class UpdateMheCommandHandler(IMheRepository repository, GssCommon.Export.IFileServiceClient fileServiceClient)
{
    private readonly IMheRepository _repository = repository;
    private readonly GssCommon.Export.IFileServiceClient _fileServiceClient = fileServiceClient;

    public async Task<Result<Guid>> Handle(UpdateMheCommand request, CancellationToken cancellationToken)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(request.Code) || string.IsNullOrWhiteSpace(request.Name))
            {
                return Result.Failure<Guid>(MheErrors.InvalidKey());
            }
            bool exists = await _repository.CodeExistsAsync(request.Id, request.Code, cancellationToken);
            if (exists)
            {
                return Result.Failure<Guid>(MheErrors.CodeExists(request.Name));
            }
            CatalogService.Domain.Aggregates.Mhe? mhe = await _repository.GetByIdAsync(request.Id, cancellationToken);
            if (mhe == null)
            {
                return Result.Failure<Guid>(MheErrors.NotFound(request.Id));
            }

            string? glbFilePath = mhe.GlbFilePath;
            if (request.GlbFile is not null)
            {
                await using var stream = request.GlbFile.OpenReadStream();
                var uploadResult = await _fileServiceClient.UploadAsync(
                    stream,
                    request.GlbFile.FileName,
                    request.GlbFile.ContentType ?? "application/octet-stream",
                    cancellationToken
                );
                glbFilePath = uploadResult.FilePath;
            }

            Dictionary<string, JsonElement>? attributes = string.IsNullOrWhiteSpace(request.Attributes) ? null
                : JsonSerializer.Deserialize<Dictionary<string, JsonElement>>(request.Attributes) ?? null;

            mhe.Update(request.Code, request.Name, request.Manufacturer, request.Brand, request.Model, request.MheType, request.MheCategory, glbFilePath, attributes ?? [], request.IsActive, request.UpdatedBy);

            await _repository.UpdateAsync(mhe, cancellationToken);
            return Result<Guid>.Success(mhe.Id);
        }
        catch (Exception ex)
        {
            return Result.Failure<Guid>(MheErrors.UpdateFailed(ex.Message));
        }
    }
}

