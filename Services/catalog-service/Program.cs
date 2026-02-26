﻿using CatalogService.Application.Handlers;
using CatalogService.Infrastructure.Persistence;
using FluentMigrator.Runner;
using FluentValidation;
using GssCommon.Export;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi.Models;


var builder = WebApplication.CreateBuilder(args);


//db connection and migrations
builder.Services.AddPersistence(builder.Configuration);

// Application Services
builder.Services.AddScoped<CatalogService.Application.Services.ICatalogService, CatalogService.Application.Services.CatalogService>();

// Wolverine for CQRS
builder.AddHandler();



// Controllers
builder.Services.AddControllers();
builder.Services.AddProblemDetails();

builder.Services.AddHttpClient<IFileServiceClient, FileServiceClient>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["Services:FileServiceBaseUrl"]!);

});

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
Title = "GSS Catalog Service API",
Version = "1.0.0",
Description = "Catalog management for SKUs, Pallets, and MHEs in GSS Warehouse Configurator",
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
c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog Service API v1");
c.RoutePrefix = "swagger";
c.DocumentTitle = "Catalog Service API Documentation";
});
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
            await context.Response.WriteAsJsonAsync(ex?.Message ?? "An unknown error occurred.");
            return;
        }

// Let other exceptions fall through to default (or map them similarly).
throw ex!;
});
});

// Health check
app.MapGet("/health", () => Results.Ok(new
{
    status = "healthy",
    timestamp = DateTime.UtcNow,
    service = "catalog-service",
    version = "1.0.0"
}));

//app.UseHttpsRedirection();
app.UseAuthorization();
app.MapControllers();

app.Run();

// Make Program class accessible for integration tests
public partial class Program { }