using CatalogService.Application.Dtos;
using CatalogService.Application.Queries.Sku;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.Sku;

public class GetAllSkusQueryHandler(ISkuRepository repository)
{
    private readonly ISkuRepository _repository = repository;


    public async Task<Result<List<SkuDto>>> Handle(GetAllSkusQuery query)
    {
        var skus = await _repository.GetAllAsync();

        return skus.Select(sku => new SkuDto(
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
        )).ToList();
    }
}

