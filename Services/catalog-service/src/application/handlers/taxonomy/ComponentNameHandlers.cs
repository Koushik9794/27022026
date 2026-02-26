using CatalogService.Application.Dtos;
using CatalogService.Application.Errors;
using CatalogService.Application.Commands;
using CatalogService.Application.Queries;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;
using Wolverine;

namespace CatalogService.Application.Handlers.Taxonomy;

public class ComponentNameHandlers
{
    private readonly IComponentNameRepository _repository;
    // Assuming we need to validate ComponentType existence, we might inject IComponentTypeRepository here too, 
    // or rely on frontend/database FK constraint. Best practice: validate in domain or handler.
    // Let's assume database constraint is enough for now, or just trust the ID. 
    // Actually, good to validate. But I don't have IComponentTypeRepository interface handy in my context, 
    // so I'll skip it for now and rely on DB constraint failing if invalid.

    public ComponentNameHandlers(IComponentNameRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<ComponentNameDto>>> Handle(GetAllComponentNamesQuery query)
    {
        var names = await _repository.GetAllAsync(query.IncludeInactive);
        return Result.Success(names.Select(MapToDto));
    }

    public async Task<Result<IEnumerable<ComponentNameDto>>> Handle(GetComponentNamesByTypeQuery query)
    {
        var names = await _repository.GetByTypeIdAsync(query.ComponentTypeId, query.IncludeInactive);
        return Result.Success(names.Select(MapToDto));
    }

    private static ComponentNameDto MapToDto(ComponentName x) =>
        new(x.Id, x.Code, x.Name, x.Description, x.ComponentTypeId, x.ComponentTypeCode, x.ComponentTypeName, x.IsActive, x.CreatedAt, x.UpdatedAt);

    public async Task<Result<ComponentNameDto>> Handle(GetComponentNameByIdQuery query)
    {
        var name = await _repository.GetByIdAsync(query.Id);
        if (name == null)
        {
            return Result.Failure<ComponentNameDto>(ComponentNameErrors.NotFound);
        }
        return Result.Success(MapToDto(name));
    }

    public async Task<Result<ComponentNameDto>> Handle(GetComponentNameByCodeQuery query)
    {
        var name = await _repository.GetByCodeAsync(query.Code);
        if (name == null)
        {
            return Result.Failure<ComponentNameDto>(ComponentNameErrors.NotFound);
        }
        return Result.Success(MapToDto(name));
    }

    public async Task<Result<Guid>> Handle(CreateComponentNameCommand command)
    {
        if (await _repository.ExistsByCodeAsync(command.Code))
        {
            return Result.Failure<Guid>(ComponentNameErrors.DuplicateCode);
        }

        var name = ComponentName.Create(
            command.Code,
            command.Name,
            command.ComponentTypeId,
            command.Description
        );

        await _repository.CreateAsync(name);

        return Result.Success(name.Id);
    }

    public async Task<Result<bool>> Handle(UpdateComponentNameCommand command)
    {
        var name = await _repository.GetByIdAsync(command.Id);
        if (name == null)
        {
            return Result.Failure<bool>(ComponentNameErrors.NotFound);
        }

        name.Update(command.Name, command.Description, command.ComponentTypeId);

        await _repository.UpdateAsync(name);

        return Result.Success(true);
    }

    public async Task<Result<bool>> Handle(DeleteComponentNameCommand command)
    {
        var name = await _repository.GetByIdAsync(command.Id);
        if (name == null)
        {
            return Result.Failure<bool>(ComponentNameErrors.NotFound);
        }

        await _repository.DeleteAsync(command.Id);

        return Result.Success(true);
    }
}
