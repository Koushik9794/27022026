using CatalogService.Application.Errors;
using CatalogService.Application.Commands.Taxonomy;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;
using System.Threading;
using System.Threading.Tasks;

namespace CatalogService.Application.Handlers.Taxonomy;

public class CreateComponentTypeHandler
{
    private readonly IComponentTypeRepository _typeRepository;
    private readonly IComponentGroupRepository _groupRepository;

    public CreateComponentTypeHandler(
        IComponentTypeRepository typeRepository,
        IComponentGroupRepository groupRepository)
    {
        _typeRepository = typeRepository;
        _groupRepository = groupRepository;
    }

    public async Task<Result<Guid>> Handle(CreateComponentTypeCommand command, CancellationToken cancellationToken = default)
    {
        // Check for duplicate code
        if (await _typeRepository.ExistsAsync(command.Code))
        {
            return Result.Failure<Guid>(ComponentTypeErrors.DuplicateCode);
        }

        // Resolve component group
        var group = await _groupRepository.GetByCodeAsync(command.ComponentGroupCode);
        if (group == null)
        {
            return Result.Failure<Guid>(Error.NotFound("ComponentGroup.NotFound", $"Component Group with code {command.ComponentGroupCode} not found."));
        }

        // Resolve parent type if specified
        Guid? parentTypeId = null;
        if (!string.IsNullOrEmpty(command.ParentTypeCode))
        {
            var parentType = await _typeRepository.GetByCodeAsync(command.ParentTypeCode);
            if (parentType == null)
            {
                return Result.Failure<Guid>(ComponentTypeErrors.ParentNotFound);
            }
            parentTypeId = parentType.Id;
        }

        var componentType = ComponentType.Create(
            command.Code,
            command.Name,
            group.Id,
            command.Description,
            parentTypeId,
            command.AttributeSchema
        );

        var id = await _typeRepository.CreateAsync(componentType);
        return Result.Success(id);
    }
}

public class UpdateComponentTypeHandler
{
    private readonly IComponentTypeRepository _typeRepository;
    private readonly IComponentGroupRepository _groupRepository;

    public UpdateComponentTypeHandler(
        IComponentTypeRepository typeRepository,
        IComponentGroupRepository groupRepository)
    {
        _typeRepository = typeRepository;
        _groupRepository = groupRepository;
    }

    public async Task<Result<bool>> Handle(UpdateComponentTypeCommand command, CancellationToken cancellationToken = default)
    {
        var componentType = await _typeRepository.GetByIdAsync(command.Id);
        if (componentType == null) return Result.Failure<bool>(ComponentTypeErrors.NotFound);

        // Resolve component group
        var group = await _groupRepository.GetByCodeAsync(command.ComponentGroupCode);
        if (group == null)
        {
            return Result.Failure<bool>(Error.NotFound("ComponentGroup.NotFound", $"Component Group with code {command.ComponentGroupCode} not found."));
        }

        // Resolve parent type if specified
        Guid? parentTypeId = null;
        if (!string.IsNullOrEmpty(command.ParentTypeCode))
        {
            var parentType = await _typeRepository.GetByCodeAsync(command.ParentTypeCode);
            if (parentType == null)
            {
                return Result.Failure<bool>(ComponentTypeErrors.ParentNotFound);
            }
            parentTypeId = parentType.Id;
        }

        componentType.Update(
            command.Name,
            command.Description,
            group.Id,
            parentTypeId,
            command.AttributeSchema
        );

        var success = await _typeRepository.UpdateAsync(componentType);
        return Result.Success(success);
    }
}

public class DeleteComponentTypeHandler
{
    private readonly IComponentTypeRepository _repository;

    public DeleteComponentTypeHandler(IComponentTypeRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(DeleteComponentTypeCommand command, CancellationToken cancellationToken = default)
    {
        var success = await _repository.DeleteAsync(command.Id);
        if (!success) return Result.Failure<bool>(ComponentTypeErrors.NotFound);
        return Result.Success(true);
    }
}
