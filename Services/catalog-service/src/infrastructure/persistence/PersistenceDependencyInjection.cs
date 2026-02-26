using CatalogService.Application.Validation.Common;
using CatalogService.Infrastructure.Persistence.Repositories;
using CatalogService.Infrastructure.Migrations;
using CatalogService.Application.Handlers.Pallets;
using CatalogService.Infrastructure.Persistence;
using FluentMigrator.Runner;

namespace CatalogService.Infrastructure.Persistence;

public static class PersistenceDependencyInjection
{
    public static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration config)
    {
        // Get connection string
        var connectionString = config.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

        // Database connections
        services.AddScoped<IDbConnectionFactory>(_ => new PostgreSqlConnectionFactory(connectionString));

        // FluentMigrator
        services.AddFluentMigratorCore()
            .ConfigureRunner(cfg => cfg
                .AddPostgres()
                .WithGlobalConnectionString(connectionString)
                .ScanIn(typeof(InitialMigration).Assembly)
                .For.Migrations());



        // Repositories
        services.AddScoped<ISkuRepository, SkuRepository>();
        services.AddScoped<IProductGroupRepository, ProductGroupRepository>();
        services.AddScoped<IMheRepository, MheRepository>();
        services.AddScoped<IPalletRepository, PalletRepository>();
        services.AddScoped<IAttributeDefinitionRepository, AttributeDefinitionRepository>();
        services.AddScoped<IWarehouseTypeRepository, WarehouseTypeRepository>();
        services.AddScoped<ICivilComponentRepository, CivilComponentRepository>();
        services.AddScoped<IComponentTypeRepository, ComponentTypeRepository>();
        
        services.AddScoped<IComponentGroupRepository, ComponentGroupRepository>();
        services.AddScoped<IComponentNameRepository, ComponentNameRepository>();
        services.AddScoped<IPartRepository, PartRepository>();
        services.AddScoped<IComponentMasterRepository, ComponentMasterRepository>();
        services.AddScoped<ICurrencyRepository, CurrencyRepository>();
        services.AddScoped<ICountryRepository, CountryRepository>();
        services.AddScoped<IExchangeRateRepository, ExchangeRateRepository>();
        services.AddScoped<ILoadChartRepository, LoadChartRepository>();

        services.AddScoped<IAttributesJsonValidator, AttributesJsonValidator>();
        services.AddScoped<IAttributeDefinitionProvider, AttributeDefinitionProviderAdapter>();

        return services;
    }
}

