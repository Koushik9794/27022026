using CatalogService.Application.dtos;
using CatalogService.Application.queries.Mhe;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.Mhe;

public class GetAllMhesQueryHandler(IMheRepository repository)
{
    private readonly IMheRepository _repository = repository;



    public async Task<Result<List<MheDto>>> Handle(GetAllMheQuery request, CancellationToken cancellationToken)
    {
        var mhe = await _repository.GetAllAsync(request.IsActive, cancellationToken);

        return mhe.Select(mhe => new MheDto(
            mhe.Id,
            mhe.Code,
            mhe.Name,
            mhe.Manufacturer,
            mhe.Brand,
            mhe.Model,
            mhe.MheType,
            mhe.MheCategory,
            mhe.GlbFilePath,
            mhe.Attributes,
            mhe.IsActive,
            mhe.IsDeleted,
            mhe.CreatedAt,
            mhe.CreatedBy,
            mhe.UpdatedAt,
            mhe.UpdatedBy
        )).ToList();
    }
}

