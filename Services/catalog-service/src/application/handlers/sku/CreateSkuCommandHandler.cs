using CatalogService.Application.Commands.Sku;
using GssCommon.Common;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Export;

namespace CatalogService.Application.Handlers.Sku;

public class CreateSkuCommandHandler(ISkuRepository repository, IFileServiceClient fileServiceClient)
{


    private readonly ISkuRepository _repository = repository;
    private readonly IFileServiceClient _fileServiceClient = fileServiceClient;


    public async Task<Result<Guid>> Handle(CreateSkuCommand request)
    {
        // Check for duplicate code
        if (await _repository.ExistsAsync(request.Code))
        {
            return Result.Failure<Guid>(SkuErrors.CodeExists(request.Code));
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

        var skus = CatalogService.Domain.Aggregates.Sku.Create(

            request.Code,
            request.Name,
            request.Description,
                        glbFilePath,
            request.AttributeSchema,

            request.IsActive,
            request.CreatedBy
        );

        await _repository.CreateAsync(skus);
        return Result<Guid>.Success(skus.Id);
    }
}

