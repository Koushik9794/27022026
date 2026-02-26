using FluentMigrator.Runner;
using FluentValidation;
using Wolverine;
using AdminService.Application.Commands;
using AdminService.Application.Handlers;
using AdminService.Application.Validators;
using AdminService.Infrastructure.Persistence;
using AdminService.Infrastructure.Dapper;
using AdminService.Infrastructure.Migrations;
using AdminService.Domain.Services;

var builder = WebApplication.CreateBuilder(args);

// Get connection string
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Database connections
DatabaseHelper.EnsureDatabase(connectionString);
builder.Services.AddScoped<IDbConnectionFactory>(_ => new PostgreSqlConnectionFactory(connectionString));

// FluentMigrator
builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(cfg => cfg
        .AddPostgres()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(InitialMigration).Assembly)
        .For.Migrations());

// Wolverine for CQRS
builder.Host.UseWolverine(opts =>
{
    // Auto-discover handlers in the application assembly
    opts.Discovery.IncludeAssembly(typeof(RegisterUserCommandHandler).Assembly);
    
    // Configure validation middleware
    opts.Policies.AutoApplyTransactions();
});

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<RegisterUserCommandValidator>();

// Repositories
builder.Services.AddScoped<IUserRepository, DapperUserRepository>();
builder.Services.AddScoped<IRoleRepository, DapperRoleRepository>();
builder.Services.AddScoped<IPermissionRepository, DapperPermissionRepository>();
builder.Services.AddScoped<IRolePermissionRepository, DapperRolePermissionRepository>();
builder.Services.AddScoped<IDealerRepository, DapperDealerRepository>();
builder.Services.AddScoped<IEntityRepository, DapperEntityRepository>();

// Controllers
builder.Services.AddControllers(options =>
{
    options.Filters.Add<AdminService.Api.GlobalExceptionFilter>();
});

// Swagger/OpenAPI with detailed configuration
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo
    {
        Title = "GSS Admin Service API",
        Version = "1.0.0",
        Description = "User and Admin Management for GSS Warehouse Configurator",
        Contact = new Microsoft.OpenApi.Models.OpenApiContact
        {
            Name = "GSS Support",
            Email = "support@gss.com"
        }
    });

    // Enable XML comments for Swagger documentation
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath))
    {
        c.IncludeXmlComments(xmlPath);
    }
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
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Admin Service API v1");
        c.RoutePrefix = "swagger"; // Swagger UI at /swagger
        c.DocumentTitle = "Admin Service API Documentation";
    });
}

// Health check
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    service = "admin-service",
    version = "1.0.0"
}))
.WithName("Health")
.WithTags("Health");

app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();
