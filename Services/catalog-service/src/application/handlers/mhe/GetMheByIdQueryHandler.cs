using CatalogService.Application.dtos;
using CatalogService.Application.Errors;
using CatalogService.Application.queries.Mhe;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.Mhe;

public class GetMheByIdQueryHandler(IMheRepository repository)
{
    private readonly IMheRepository _repository = repository;



    public async Task<Result<MheDto?>> Handle(GetMheByIdQuery request, CancellationToken cancellationToken)
    {
        var mhe = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (mhe == null)
        {
            return Result.Failure<MheDto?>(MheErrors.NotFound(request.Id));
        }

        return new MheDto(
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
        );
    }
}

