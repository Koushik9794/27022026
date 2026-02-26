using FluentValidation;
using FluentMigrator.Runner;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "BOM Service API",
        Version = "v1",
        Description = "Bill of Materials generation and management"
    });
});

// Add MediatR
builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Database connections
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
builder.Services.AddScoped<System.Data.IDbConnection>(_ => new Npgsql.NpgsqlConnection(connectionString));

// Persistence
builder.Services.AddScoped<BomService.Infrastructure.Persistence.IBomRepository, BomService.Infrastructure.Persistence.DapperBomRepository>();

// FluentMigrator
builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(cfg => cfg
        .AddPostgres()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(BomService.Infrastructure.Migrations.InitialMigration).Assembly)
        .For.Migrations());

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "bom-service" }));

app.Run();
