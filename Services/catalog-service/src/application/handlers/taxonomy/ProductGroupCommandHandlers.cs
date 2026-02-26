using CatalogService.Application.Errors;
using CatalogService.Application.Commands.Taxonomy;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;

namespace CatalogService.Application.Handlers.Taxonomy;

public class CreateProductGroupHandler
{
    private readonly IProductGroupRepository _repository;

    public CreateProductGroupHandler(IProductGroupRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<Guid>> Handle(CreateProductGroupCommand command)
    {
        // Check for duplicate code
        if (await _repository.ExistsAsync(command.Code))
        {
            return Result.Failure<Guid>(ProductGroupErrors.DuplicateCode);
        }

        // Resolve parent group if specified
        Guid? parentGroupId = null;
        if (!string.IsNullOrEmpty(command.ParentGroupCode))
        {
            var parentGroup = await _repository.GetByCodeAsync(command.ParentGroupCode);
            if (parentGroup == null)
            {
                return Result.Failure<Guid>(ProductGroupErrors.ParentNotFound);
            }
            parentGroupId = parentGroup.Id;
        }

        var productGroup = ProductGroup.Create(
            command.Code,
            command.Name,
            command.Description,
            parentGroupId
        );

        var id = await _repository.CreateAsync(productGroup);
        return Result.Success(id);
    }
}

public class UpdateProductGroupHandler
{
    private readonly IProductGroupRepository _repository;

    public UpdateProductGroupHandler(IProductGroupRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(UpdateProductGroupCommand command)
    {
        var productGroup = await _repository.GetByIdAsync(command.Id);
        if (productGroup == null) return Result.Failure<bool>(ProductGroupErrors.NotFound);

        // Resolve parent group if specified
        Guid? parentGroupId = null;
        if (!string.IsNullOrEmpty(command.ParentGroupCode))
        {
            var parentGroup = await _repository.GetByCodeAsync(command.ParentGroupCode);
            if (parentGroup == null)
            {
                return Result.Failure<bool>(ProductGroupErrors.ParentNotFound);
            }
            parentGroupId = parentGroup.Id;
        }

        productGroup.Update(command.Name, command.Description, parentGroupId);

        var success = await _repository.UpdateAsync(productGroup);
        return Result.Success(success);
    }
}

public class DeleteProductGroupHandler
{
    private readonly IProductGroupRepository _repository;

    public DeleteProductGroupHandler(IProductGroupRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<bool>> Handle(DeleteProductGroupCommand command)
    {
        var success = await _repository.DeleteAsync(command.Id);
        if (!success) return Result.Failure<bool>(ProductGroupErrors.NotFound);
        return Result.Success(true);
    }
}
