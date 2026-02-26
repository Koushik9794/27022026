using Serilog;
using GssWebApi.Dto;
using GssWebApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using NSwag;
using NSwag.Generation.Processors.Security;
using GssWebApi.src.Services;
using FluentValidation;
using FluentValidation.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateLogger();

builder.Host.UseSerilog();

// Add services to the container
builder.Services.AddControllers(options =>
{
    options.Filters.Add<GssWebApi.Api.Filters.BffExceptionFilter>();
});

// Validation
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssemblyContaining<Program>();

//CatalogServiceClient
builder.Services.AddScoped<ICatalogServiceClient, CatalogServiceClient>();


// Authentication
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        var authConfig = builder.Configuration.GetSection("Authentication");
        var secret = authConfig["JwtSecret"] ?? "a_very_long_and_secure_secret_key_for_development_123456";
        
        options.Authority = authConfig["Authority"];
        options.Audience = authConfig["Audience"];
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = authConfig.GetValue<bool>("ValidateIssuer"),
            ValidIssuer = authConfig["Issuer"],
            ValidateAudience = authConfig.GetValue<bool>("ValidateAudience"),
            ValidAudience = authConfig["Audience"],
            ValidateLifetime = authConfig.GetValue<bool>("ValidateLifetime"),
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secret)),
            RequireExpirationTime = true
        };

        if (builder.Environment.IsDevelopment())
        {
            options.RequireHttpsMetadata = false;
        }
    });

// Admin Service Integration
builder.Services.AddHttpClient<IAdminServiceClient, AdminServiceClient>(client =>
{
    var adminServiceUrl = builder.Configuration["ServiceEndpoints:AdminService"] ?? "http://localhost:5000";
    client.BaseAddress = new Uri(adminServiceUrl);
});
// Admin Service Integration
builder.Services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>(client =>
{
    var CatalogServiceeUrl = builder.Configuration["ServiceEndpoints:CatalogService"] ?? "http://localhost:5003";
    client.BaseAddress = new Uri(CatalogServiceeUrl);
});

// Configuration Service Integration
builder.Services.AddHttpClient<IConfigurationService, ConfigurationService>(client =>
{
    var CatalogServiceeUrl = builder.Configuration["ServiceEndpoints:ConfigService"] ?? "http://localhost:5003";
    client.BaseAddress = new Uri(CatalogServiceeUrl);
});

// NSwag OpenAPI
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddOpenApiDocument(document =>
{
    document.Title = "GSS Web API - Backend for Frontend";
    document.Version = "1.0.0";
    document.Description = "Backend-for-Frontend (BFF) service for Godrej Storage Solutions warehouse configurator platform";
    
    // Add JWT Bearer authentication
    document.AddSecurity("JWT", new OpenApiSecurityScheme
    {
        Type = OpenApiSecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Name = "Authorization",
        In = OpenApiSecurityApiKeyLocation.Header,
        Description = "JWT Authorization header using the Bearer scheme. Enter 'Bearer' [space] and then your token in the text input below."
    });

    document.OperationProcessors.Add(
        new AspNetCoreOperationSecurityScopeProcessor("JWT"));
});

// CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
            "http://localhost:3000", //for local frontend
            "http://localhost:5173", //vite dev server
            "https://app.gss.com"    //production frontend URL
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
        .AllowCredentials();
    });
});

// Health checks
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseOpenApi();
    app.UseSwaggerUi();
}

// Middleware
app.UseCors("AllowFrontend");
//p.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Health check endpoint
app.MapHealthChecks("/health");

// Enhanced health endpoint with details
app.MapGet("/health/detailed", () =>
{
    var response = new HealthResponse
    {
        Status = "healthy",
        Timestamp = DateTime.UtcNow,
        Service = "gss-web-api",
        Version = "1.0.0",
        Dependencies = new Dictionary<string, string>
        {
            { "adminService", "healthy" },
            { "catalogService", "healthy" },
            { "ruleService", "healthy" }
        }
    };
    return Results.Ok(response);
})
.WithName("DetailedHealth")
.WithTags("Health")
.AllowAnonymous();

app.MapControllers();

app.Run();
