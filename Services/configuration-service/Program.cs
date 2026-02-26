using ConfigurationService.Application.Abstractions;
using ConfigurationService.Application.Handlers;
using ConfigurationService.Infrastructure.Persistence;
using ConfigurationService.Infrastructure.Persistence.Repositories;
using ConfigurationService.Infrastructure.Clients;
using FluentMigrator.Runner;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;
using Wolverine;
using Wolverine.FluentValidation;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Configuration Service API",
        Version = "v1",
        Description = "Warehouse configuration state and versioning for enquiries"
    });
});

// Add MediatR
//builder.Services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(Program).Assembly));
builder.Host.UseWolverine(opts =>
{
// Auto-discover handlers in the application assembly

//Atrribute handlers
opts.Discovery.IncludeAssembly(typeof(CreateEnquiryHandler).Assembly);
    opts.Discovery.IncludeAssembly(typeof(CreateEnquiryHandler).Assembly);

    opts.Discovery.IncludeAssembly(typeof(SaveCivilLayoutHandler).Assembly);
    
    opts.Policies.AutoApplyTransactions();

    opts.UseFluentValidation();
});

builder.Services.AddHttpClient<IFileServiceClient, FileServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:FileServiceBaseUrl"]!);
    
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Database connection
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? "Host=localhost;Database=configuration_service;Username=postgres;Password=postgres;Port=5432";

builder.Services.AddSingleton<IDbConnectionFactory>(_ => new PostgresConnectionFactory(connectionString));

// Repositories - aggregate-based (load full aggregates)
builder.Services.AddScoped<IEnquiryRepository, EnquiryRepository>();
builder.Services.AddScoped<IConfigurationRepository, ConfigurationRepository>();
builder.Services.AddScoped<ICivilLayoutRepository, CivilLayoutRepository>();
builder.Services.AddScoped<IRackConfigurationRepository, RackConfigurationRepository>();

// Orchestration Services
builder.Services.AddScoped<ConfigurationService.Application.Services.ConfiguratorService>();
builder.Services.AddHttpClient<ConfigurationService.Application.Services.IRuleServiceClient, ConfigurationService.Infrastructure.Services.RuleServiceClient>();
builder.Services.AddHttpClient<ConfigurationService.Application.Services.IBomServiceClient, ConfigurationService.Infrastructure.Services.BomServiceClient>();

// FluentMigrator
builder.Services.AddFluentMigratorCore()
    .ConfigureRunner(rb => rb
        .AddPostgres()
        .WithGlobalConnectionString(connectionString)
        .ScanIn(typeof(Program).Assembly).For.Migrations())
    .AddLogging(lb => lb.AddFluentMigratorConsole());
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()    // or .WithOrigins("https://your-frontend.com")
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});
var app = builder.Build();

// Run migrations on startup in development
if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var runner = scope.ServiceProvider.GetRequiredService<IMigrationRunner>();
    runner.MigrateUp();
}

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseCors("AllowAll");
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var feature = context.Features.Get<Microsoft.AspNetCore.Diagnostics.IExceptionHandlerFeature>();
        var ex = feature?.Error;

        if (ex is ValidationException vex)
        {
            // ValidationException exposes the failures in .Errors [2]
            var errors = vex.Errors
                .GroupBy(e => e.PropertyName ?? string.Empty)
                .ToDictionary(g => g.Key, g => g.Select(x => x.ErrorMessage).ToArray());

            // ValidationProblemDetails supports IDictionary<string, string[]> errors [3]
            var problem = new ValidationProblemDetails(errors)
            {
                Title = "Validation failed",
                Status = StatusCodes.Status400BadRequest
            };

            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(problem);
            return;
        }
        else
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            context.Response.ContentType = "application/problem+json";
            await context.Response.WriteAsJsonAsync(ex);
            return;
        }

        // Let other exceptions fall through to default (or map them similarly).
        throw ex!;
    });
});
app.UseAuthorization();
app.MapControllers();

// Health check endpoint
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "configuration-service" }));

app.Run();
