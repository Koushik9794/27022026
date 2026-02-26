using CatalogService.Application.dtos;
using CatalogService.Application.queries.Mhe;
using CatalogService.Application.queries.palette;
using CatalogService.Application.Dtos;
using CatalogService.Application.Queries.Pallets;
using CatalogService.Application.Queries.Sku;
using CatalogService.Application.Queries.Taxonomy;
using CatalogService.Infrastructure.Persistence;
using GssCommon.Common;
using Microsoft.AspNetCore.Mvc;
using Wolverine;
namespace CatalogService.Application.Handlers.paletteHandler;

public class CivilDesignerHandler(IMessageBus bus)
{
    private readonly IMessageBus _bus = bus;
    public async Task<CivilDesignerResponseDto> Handle(GetCivilDesignerDataQuery query)
    {
        var warehouseTypesResult = await _bus.InvokeAsync<Result<List<WarehouseTypeDto>>>(new GetAllWarehouseTypesQuery(true));
        var CivileTypesResult = await _bus.InvokeAsync<Result<List<CivilComponentDto>>>(new GetAllCivileComponentQuery(true));
        var SkuResult = await _bus.InvokeAsync<Result<List<SkuDto>>>(new GetAllSkusQuery());
        var PalletResult = await _bus.InvokeAsync<Result<List<PalletDto>>>(new GetAllPalletsQuery(true));
        var MheResult = await _bus.InvokeAsync<Result<List<MheDto>>>(new GetAllMheQuery(true));

        

        var response = new CivilDesignerResponseDto
        {
            WarehouseTypes = warehouseTypesResult.IsSuccess ? warehouseTypesResult.Value : [],
            CivilComponents = CivileTypesResult.IsSuccess ? CivileTypesResult.Value : [],
            Skus = SkuResult.IsSuccess ? SkuResult.Value : [],
            Pallets = PalletResult.IsSuccess ? PalletResult.Value : [],
            Mhe = MheResult.IsSuccess ? MheResult.Value : []


        };
        return response;
    }
}

