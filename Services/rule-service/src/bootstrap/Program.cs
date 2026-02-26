using FluentMigrator.Runner;
using FluentValidation;
using Wolverine;
using Wolverine.Http;
using RuleService.Domain.Services;
using RuleService.Infrastructure.Services;
using RuleService.Infrastructure.Adapters;
using RuleService.Infrastructure.Dapper;
using RuleService.Infrastructure.Migrations;
using RuleService.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Add services to the container

// Database connections
builder.Services.AddScoped<IDbConnectionFactory>(_ => new PostgreSqlConnectionFactory(connectionString));

// FluentMigrator
builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(cfg => cfg
        .AddPostgres()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(InitialMigration).Assembly)
        .For.Migrations());

// Wolverine for messaging and HTTP endpoints
builder.Services.AddWolverineHttp();

builder.Host.UseWolverine(opts =>
{
    // Auto-discover handlers in the application assembly
    opts.Discovery.IncludeAssembly(typeof(Program).Assembly);
    
    // Configure HTTP endpoints
    // opts.Policies.AutoApplyTransactions();
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

// Domain Services
builder.Services.AddScoped<IRuleEvaluationService, RuleEvaluationServiceImpl>();

// Infrastructure
builder.Services.AddScoped<IRuleRepository, DapperRuleRepository>();
builder.Services.AddScoped<ILookupMatrixRepository, DapperLookupMatrixRepository>();
builder.Services.AddScoped<IMatrixEvaluationService, MatrixEvaluationServiceImpl>();

// Expression engine - use DynamicExpresso implementation by default
builder.Services.AddScoped<IExpressionEngineAdapter, DynamicExpressoExpressionEngine>();

// Application Handlers (CQRS)
builder.Services.AddScoped<RuleService.Application.Handlers.ImportLoadChartHandler>();

// Field Metadata Service
builder.Services.AddScoped<IFieldMetadataService, FieldMetadataService>();

// External Service Clients
builder.Services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "GSS Rule Service API",
        Version = "1.0.0",
        Description = "Business Rules Engine for GSS Warehouse Configurator"
    });
});

// Logging
builder.Services.AddLogging(cfg => cfg.AddConsole());

var app = builder.Build();

// Run migrations on startup
using (var scope = app.Services.CreateScope())
{
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Rule Service API v1"));
}

// Health check
app.MapGet("/health", () => Results.Ok(new { status = "healthy", timestamp = DateTime.UtcNow }))
    .WithName("Health");

// Map Wolverine HTTP endpoints
try
{
    Console.WriteLine("Mapping Wolverine endpoints...");
    app.MapWolverineEndpoints();
    Console.WriteLine("Wolverine endpoints mapped successfully.");
}
catch (Exception ex)
{
    Console.WriteLine("FATAL ERROR MAPPING WOLVERINE ENDPOINTS:");
    Console.WriteLine(ex.ToString());
    throw;
}

app.Run();
