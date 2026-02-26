using System.Text.Json;
using CatalogService.Application.Dtos;
using CatalogService.Application.Errors;
using CatalogService.Application.Queries;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.Parts;

public class PartQueryHandlers
{
    private readonly IPartRepository _repository;
    private readonly IComponentTypeRepository _componentTypeRepository;
    private readonly IAttributeDefinitionRepository _attributeDefinitionRepository;

    public PartQueryHandlers(
        IPartRepository repository,
        IComponentTypeRepository componentTypeRepository,
        IAttributeDefinitionRepository attributeDefinitionRepository)
    {
        _repository = repository;
        _componentTypeRepository = componentTypeRepository;
        _attributeDefinitionRepository = attributeDefinitionRepository;
    }

    public async Task<IEnumerable<Part>> Handle(GetAllPartsQuery query)
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

    public async Task<Result<Part>> Handle(GetPartByIdQuery query)
    {
        var part = await _repository.GetByIdAsync(query.Id);
        if (part == null)
        {
            return Result.Failure<Part>(PartErrors.NotFound);
        }
        return Result.Success(part);
    }

    public async Task<Result<Part>> Handle(GetPartByCodeAndCountryQuery query)
    {
        var part = await _repository.GetByCodeAndCountryAsync(query.PartCode, query.CountryCode);
        if (part == null)
        {
            return Result.Failure<Part>(Error.NotFound("Part.NotFound", $"Part with Code {query.PartCode} and Country {query.CountryCode} not found."));
        }
        return Result.Success(part);
    }
}
