using CatalogService.Application.Dtos;
using CatalogService.Application.Errors;
using CatalogService.Application.Commands;
using CatalogService.Application.Queries;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;
using Wolverine;

namespace CatalogService.Application.Handlers.Taxonomy;

public class ComponentGroupHandlers
{
    private readonly IComponentGroupRepository _repository;

    public ComponentGroupHandlers(IComponentGroupRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<IEnumerable<ComponentGroupDto>>> Handle(GetAllComponentGroupsQuery query)
    {
        var groups = await _repository.GetAllAsync(query.IncludeInactive);
        return Result.Success(groups.Select(MapToDto));
    }

    private static ComponentGroupDto MapToDto(ComponentGroup x) =>
        new(x.Id, x.Code, x.Name, x.Description, x.SortOrder, x.IsActive, x.CreatedAt, x.UpdatedAt);

    public async Task<Result<ComponentGroupDto>> Handle(GetComponentGroupByIdQuery query)
    {
        var group = await _repository.GetByIdAsync(query.Id);
        if (group == null)
        {
            return Result.Failure<ComponentGroupDto>(ComponentGroupErrors.NotFound);
        }
        return Result.Success(MapToDto(group));
    }

    public async Task<Result<ComponentGroupDto>> Handle(GetComponentGroupByCodeQuery query)
    {
        var group = await _repository.GetByCodeAsync(query.Code);
        if (group == null)
        {
            return Result.Failure<ComponentGroupDto>(ComponentGroupErrors.NotFound);
        }
        return Result.Success(MapToDto(group));
    }

    public async Task<Result<Guid>> Handle(CreateComponentGroupCommand command)
    {
        if (await _repository.ExistsByCodeAsync(command.Code))
        {
            return Result.Failure<Guid>(ComponentGroupErrors.DuplicateCode);
        }

        var group = ComponentGroup.Create(
            command.Code,
            command.Name,
            command.Description,
            command.SortOrder
        );

        await _repository.CreateAsync(group);

        return Result.Success(group.Id);
    }

    public async Task<Result<bool>> Handle(UpdateComponentGroupCommand command)
    {
        var group = await _repository.GetByIdAsync(command.Id);
        if (group == null)
        {
            return Result.Failure<bool>(ComponentGroupErrors.NotFound);
        }

        group.Update(command.Name, command.Description, command.SortOrder);

        await _repository.UpdateAsync(group);

        return Result.Success(true);
    }

    public async Task<Result<bool>> Handle(DeleteComponentGroupCommand command)
    {
        var group = await _repository.GetByIdAsync(command.Id);
        if (group == null)
        {
            return Result.Failure<bool>(ComponentGroupErrors.NotFound);
        }

        // Hard delete for now as per repository
        await _repository.DeleteAsync(command.Id);

        return Result.Success(true);
    }
}
