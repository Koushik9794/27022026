using CatalogService.Application.Errors;
using CatalogService.Application.Dtos;
using CatalogService.Application.Queries.Pallets;
using CatalogService.Application.Queries.Taxonomy;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;



namespace CatalogService.Application.Handlers.Pallets;
/// <summary>
/// Handles pallet type query operations.
/// </summary>
public class GetAllPalletsHandler
{
    private readonly IPalletRepository _repository;

    public GetAllPalletsHandler(IPalletRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<List<PalletDto>>> Handle(GetAllPalletsQuery query)
    {
        var pallets = await _repository.GetAllAsync(query.IncludeInactive);
        return pallets.Select(MapToDto).ToList();
    }

    private static PalletDto MapToDto(Pallet pallet)
    {
        return new PalletDto(
            pallet.Id,
            pallet.Code,
            pallet.Name,
            pallet.Description,
            pallet.GetAttributeSchemaDictionary(),
            pallet.GlbFilePath,
            pallet.IsActive,
            pallet.CreatedAt,
            pallet.CreatedBy,
            pallet.UpdatedAt,
            pallet.UpdatedBy
        );
    }
}

public class GetPalletsByIdHandler
{
    private readonly IPalletRepository _repository;

    public GetPalletsByIdHandler(IPalletRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PalletDto?>> Handle(GetPalletByIdQuery query)
    {
        var category = await _repository.GetByIdAsync(query.Id);
        if (category == null) { return Result.Failure<PalletDto?>(PalletErrors.NotFound(query.Id)); }

        return new PalletDto(
            category.Id,
            category.Code,
            category.Name,
            category.Description,
            category.GetAttributeSchemaDictionary(),
            category.GlbFilePath,
            category.IsActive,
            category.CreatedAt,
            category.CreatedBy,
            category.UpdatedAt,
            category.UpdatedBy
        );
    }
}

public class GetPalletsByCodeHandler
{
    private readonly IPalletRepository _repository;

    public GetPalletsByCodeHandler(IPalletRepository repository)
    {
        _repository = repository;
    }

    public async Task<Result<PalletDto?>> Handle(GetPalletByCodeQuery query)
    {
        var category = await _repository.GetByCodeAsync(query.Code);
        if (category == null) { return Result.Failure<PalletDto?>(Error.Failure("Pallet.codeexits", $"Pallet with code '{query.Code}' already exists.")); }

        return new PalletDto(
            category.Id,
            category.Code,
            category.Name,
            category.Description,
            category.GetAttributeSchemaDictionary(),
            category.GlbFilePath,
            category.IsActive,
            category.CreatedAt,
            category.CreatedBy,
            category.UpdatedAt,
            category.UpdatedBy
        );
    }
}

