using FileService.Application.Commands;
using FluentValidation;
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
        Title = "File Service API",
        Version = "v1",
        Description = "File upload/download, GLB models, Excel/CSV import/export"
    });
});

// Add MediatR


builder.Host.UseWolverine(opts =>
{
    // Auto-discover handlers in the application assembly

    //Atrribute handlers
    opts.Discovery.IncludeAssembly(typeof(UploadFileCommand).Assembly);

    opts.Discovery.IncludeAssembly(typeof(UploadFileCommandHandler).Assembly);

    opts.Policies.AutoApplyTransactions();

    opts.UseFluentValidation();
});

// Add FluentValidation
builder.Services.AddValidatorsFromAssembly(typeof(Program).Assembly);

// Configuration
builder.Services.Configure<FileService.Infrastructure.Configuration.FileValidationOptions>(
    builder.Configuration.GetSection(FileService.Infrastructure.Configuration.FileValidationOptions.SectionName));

// Storage Provider
builder.Services.AddSingleton<FileService.Application.Interfaces.IStorageProvider, FileService.Infrastructure.Storage.LocalStorageProvider>();

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
app.MapGet("/health", () => Results.Ok(new { status = "healthy", service = "file-service" }));

app.Run();
