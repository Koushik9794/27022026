using CatalogService.Application.Commands.Sku;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;
using GssCommon.Export;

namespace CatalogService.Application.Handlers.Sku;

public class UpdateSkuCommandHandler
{
    private readonly ISkuRepository _repository;
    private readonly IFileServiceClient _fileServiceClient;

    public UpdateSkuCommandHandler(ISkuRepository repository, IFileServiceClient fileServiceClient)
    {
        _repository = repository;
        _fileServiceClient = fileServiceClient;
    }

    public async Task<Result<bool>> Handle(UpdateSkuCommand request, CancellationToken cancellationToken)
    {
        if (request.Id == Guid.Empty)
        {
            return Result.Failure<bool>(SkuErrors.InvalidId());
        }

        var sku = await _repository.GetByIdAsync(request.Id);
        if (sku == null)
        {
            return Result.Failure<bool>(SkuErrors.NotFound(request.Id));
        }

        string? glbFilePath = null;
        if (request.GlbFile is not null)
        {
            await using var stream = request.GlbFile.OpenReadStream();
            var uploadResult = await _fileServiceClient.UploadAsync(
                stream,
                request.GlbFile.FileName,
                request.GlbFile.ContentType ?? "application/octet-stream"
            );
            glbFilePath = uploadResult.FilePath;
        }

        sku.Update(
            request.Name,
            request.Description,
            request.AttributeSchema,
            glbFilePath,
            request.IsActive,
            request.UpdatedBy
        );

        await _repository.UpdateAsync(sku);
        return Result.Success(true);
    }
}
