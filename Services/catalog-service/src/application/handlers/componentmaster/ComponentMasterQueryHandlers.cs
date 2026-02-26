using CatalogService.Application.Queries;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.ComponentMaster;

public class ComponentMasterQueryHandlers
{
    private readonly IComponentMasterRepository _repository;

    public ComponentMasterQueryHandlers(IComponentMasterRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<CatalogService.Domain.Aggregates.ComponentMaster>> Handle(GetAllComponentMastersQuery query)
    {
        return await _repository.GetAllAsync(
            query.CountryCode,
            query.ComponentGroupId,
            query.ComponentTypeId,
            query.IsActive,
            query.IncludeDeleted,
            query.Page,
            query.PageSize
        );
    }

    public async Task<Result<CatalogService.Domain.Aggregates.ComponentMaster>> Handle(GetComponentMasterByIdQuery query)
    {
        var cm = await _repository.GetByIdAsync(query.Id);
        if (cm == null)
        {
            return Result.Failure<CatalogService.Domain.Aggregates.ComponentMaster>(Error.NotFound("ComponentMaster.NotFound", $"Component Master with ID {query.Id} not found."));
        }
        return Result.Success(cm);
    }

    public async Task<Result<CatalogService.Domain.Aggregates.ComponentMaster>> Handle(GetComponentMasterByCodeAndCountryQuery query)
    {
        var cm = await _repository.GetByCodeAndCountryAsync(query.ComponentMasterCode, query.CountryCode);
        if (cm == null)
        {
            return Result.Failure<CatalogService.Domain.Aggregates.ComponentMaster>(Error.NotFound("ComponentMaster.NotFound", $"Component Master with Code {query.ComponentMasterCode} and Country {query.CountryCode} not found."));
        }
        return Result.Success(cm);
    }
}
