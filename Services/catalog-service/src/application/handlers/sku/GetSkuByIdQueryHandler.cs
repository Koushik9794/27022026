using CatalogService.Application.Dtos;
using GssCommon.Common;
using CatalogService.Application.Queries.Sku;
using CatalogService.Infrastructure.Persistence;

namespace CatalogService.Application.Handlers.Sku;

public class GetSkuByIdQueryHandler
{
    private readonly ISkuRepository _repository;

    public GetSkuByIdQueryHandler(ISkuRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<SkuDto?>> Handle(GetSkuByIdQuery query)
    {
        if (query.Id == Guid.Empty)
        {
            return Result.Failure<SkuDto?>(SkuErrors.InvalidId());
        }

        var sku = await _repository.GetByIdAsync(query.Id);
        if (sku == null)
        {
            return Result.Failure<SkuDto?>(SkuErrors.NotFound(query.Id));
        }

        return Result.Success<SkuDto?>(new SkuDto(
            sku.Id,
            sku.Code,
            sku.Name,
            sku.Description,
            sku.GetAttributeSchemaDictionary(),
            sku.GlbFilePath,
            sku.IsActive,
            sku.CreatedAt,
            sku.CreatedBy,
            sku.UpdatedAt,
            sku.UpdatedBy
        ));
    }
}

