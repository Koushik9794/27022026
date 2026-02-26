using CatalogService.Application.Handlers.attributes;
using CatalogService.Application.Handlers.Mhe;
using CatalogService.Application.Handlers.Sku;
using CatalogService.Application.Handlers.Taxonomy;

using Wolverine;
using Wolverine.FluentValidation;
using CatalogService.Application.Handlers.Taxonomy;


namespace CatalogService.Application.Handlers;

public static class DependencyInjection
{
    public static WebApplicationBuilder AddHandler(this WebApplicationBuilder builder)
    {
        // Wolverine for CQRS
        builder.Host.UseWolverine(opts =>
        {
            // Auto-discover handlers in the application assembly

            //Atrribute handlers
            opts.Discovery.IncludeAssembly(typeof(CreateAttributeDefinitionCommandHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(DeleteAttributeDefinitionCommandHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(UpdateAttributeDefinitionCommandHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(GetAllAttributeDefinitionsQueryHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(GetAttributeDefinitionByIdQueryHandler).Assembly);

            //SKUs handlers
            opts.Discovery.IncludeAssembly(typeof(CreateSkuCommandHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(DeleteSkuCommandHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(UpdateSkuCommandHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(GetSkuByIdQueryHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(GetAllSkusQueryHandler).Assembly);

            //MHE handlers
            opts.Discovery.IncludeAssembly(typeof(CreateMheCommandHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(DeleteMheCommandHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(UpdateMheCommandHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(GetAllMhesQueryHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(GetMheByIdQueryHandler).Assembly);






            //Warehouse type handlers
            opts.Discovery.IncludeAssembly(typeof(CreateWarehouseTypeHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(DeleteWarehouseTypeHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(UpdateWarehouseTypeHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(WarehouseTypesQueryHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(WarehouseTypeByIdQueryHandler).Assembly);

            //Component Group handlers
            opts.Discovery.IncludeType<ComponentGroupHandlers>();
            
            //Component Name handlers
            opts.Discovery.IncludeType<ComponentNameHandlers>();
            
            //Component Type handlers
            opts.Discovery.IncludeType<CreateComponentTypeHandler>();
            opts.Discovery.IncludeType<UpdateComponentTypeHandler>();
            opts.Discovery.IncludeType<DeleteComponentTypeHandler>();
            opts.Discovery.IncludeType<GetAllComponentTypesHandler>();
            opts.Discovery.IncludeType<GetComponentTypeByIdHandler>();
            opts.Discovery.IncludeType<GetComponentTypeByCodeHandler>();
            
            //Product Group Handlers
             opts.Discovery.IncludeType<CreateProductGroupHandler>();
             opts.Discovery.IncludeType<UpdateProductGroupHandler>();
             opts.Discovery.IncludeType<DeleteProductGroupHandler>();
             opts.Discovery.IncludeType<GetAllProductGroupsHandler>();
             opts.Discovery.IncludeType<GetProductGroupByIdHandler>();
             opts.Discovery.IncludeType<GetProductGroupByCodeHandler>();
             opts.Discovery.IncludeType<GetProductGroupVariantsHandler>();

            //Part Handlers
            opts.Discovery.IncludeType<CatalogService.Application.Handlers.Parts.PartCommandHandlers>();
            opts.Discovery.IncludeType<CatalogService.Application.Handlers.Parts.PartQueryHandlers>();

            //ComponentMaster Handlers
            opts.Discovery.IncludeType<CatalogService.Application.Handlers.ComponentMaster.ComponentMasterCommandHandlers>();
            opts.Discovery.IncludeType<CatalogService.Application.Handlers.ComponentMaster.ComponentMasterQueryHandlers>();

            // Currency Handlers
            opts.Discovery.IncludeType<CatalogService.Application.Handlers.Currency.CurrencyHandlers>();

            // Country Handlers
            opts.Discovery.IncludeType<CatalogService.Application.Handlers.Country.CountryHandlers>();

            // Exchange Rate Handlers
            opts.Discovery.IncludeType<CatalogService.Application.Handlers.Exchange.ExchangeRateHandlers>();

            // LoadChart Handlers
            opts.Discovery.IncludeType<CatalogService.Application.Handlers.LoadCharts.LoadChartCommandHandlers>();
            opts.Discovery.IncludeType<CatalogService.Application.Handlers.LoadCharts.ImportLoadChartExcelCommandHandler>();
            opts.Discovery.IncludeType<CatalogService.Application.Handlers.LoadCharts.LoadChartQueryHandlers>();

            //category type handlers
            opts.Discovery.IncludeAssembly(typeof(CreateCivilComponentCommandHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(UpdateCivilComponentCommandHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(DeleteCivilComponentCommandHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(CivilComponentQueryHandler).Assembly);
            opts.Discovery.IncludeAssembly(typeof(CivilComponentByIdQueryHandler).Assembly);

            // Configure policies
            opts.Policies.AutoApplyTransactions();

            opts.UseFluentValidation();
        });

        return builder;
    }
}

