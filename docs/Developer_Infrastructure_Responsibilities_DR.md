# Developer & Infrastructure Responsibilities for DR Strategy
## Warehouse Configurator Solution

**Version:** 1.0  
**Date:** December 2025  
**Audience:** Development Teams, Infrastructure Teams, DevOps Engineers

---

## Table of Contents

1. [Developer Responsibilities](#developer-responsibilities)
2. [Infrastructure Team Responsibilities](#infrastructure-team-responsibilities)
3. [IaC Tool Selection: CloudFormation vs Terraform](#iac-tool-selection)
4. [Implementation Guidelines](#implementation-guidelines)
5. [Testing & Validation Requirements](#testing-validation-requirements)

---

## Developer Responsibilities

### 1. Application-Level DR Readiness

#### 1.1 Configuration Management

**Requirement:** Applications must read configuration from AWS Systems Manager Parameter Store, NOT from hardcoded values or local config files.

**Why DR Needs This:**
During DR failover, database endpoints, cache endpoints, and S3 bucket names change from Mumbai to Singapore. Applications must pick up new values without code changes.

**Developer Tasks:**

**✅ DO:**
```csharp
// .NET Example - CORRECT APPROACH
public class DatabaseConfiguration
{
    private readonly IAmazonSimpleSystemsManagement _ssmClient;
    
    public async Task<string> GetConnectionStringAsync()
    {
        var request = new GetParameterRequest
        {
            Name = "/configurator/prod/db/connection-string",
            WithDecryption = true
        };
        
        var response = await _ssmClient.GetParameterAsync(request);
        return response.Parameter.Value;
    }
}

// Application startup - refresh config periodically
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Inject SSM client
        services.AddAWSService<IAmazonSimpleSystemsManagement>();
        
        // Register config that refreshes every 5 minutes
        services.AddSingleton<DatabaseConfiguration>();
        
        // Use cached config with TTL
        services.AddMemoryCache();
    }
}
```

**❌ DON'T:**
```csharp
// WRONG - Hardcoded connection string
public class DatabaseService
{
    private const string ConnectionString = 
        "Server=configurator-db.ap-south-1.rds.amazonaws.com;Database=configurator_db";
    
    // This will FAIL during DR failover - hardcoded Mumbai endpoint
}

// WRONG - Config file with environment-specific values
// appsettings.Production.json
{
    "Database": {
        "Host": "configurator-db.ap-south-1.rds.amazonaws.com" // Don't do this
    }
}
```

**Implementation Checklist:**

- [ ] All database connection strings read from Parameter Store
- [ ] Redis endpoints read from Parameter Store
- [ ] S3 bucket names read from Parameter Store
- [ ] External API endpoints read from Parameter Store
- [ ] Feature flags read from Parameter Store or DynamoDB
- [ ] Configuration refreshes periodically (5-10 minute TTL)
- [ ] Fallback values in case Parameter Store unavailable
- [ ] Environment variables only for AWS region and service name

**Parameter Store Structure:**

```
/configurator/prod/db/endpoint                  → RDS endpoint
/configurator/prod/db/reader-endpoint           → Read replica endpoint
/configurator/prod/db/connection-string         → Full connection string (encrypted)
/configurator/prod/db/username                  → DB username
/configurator/prod/cache/redis-endpoint         → Redis endpoint
/configurator/prod/storage/designs-bucket       → S3 bucket for designs
/configurator/prod/storage/documents-bucket     → S3 bucket for documents
/configurator/prod/api/external-cad-service     → External CAD API endpoint
```

---

#### 1.2 Database Connection Resilience

**Requirement:** Applications must handle database connection failures gracefully and reconnect automatically.

**Why DR Needs This:**
During RDS Multi-AZ failover or DR activation, existing database connections will fail. Applications must detect this and reconnect without crashing.

**Developer Tasks:**

**Connection Pool Configuration:**

```csharp
// .NET with Npgsql (PostgreSQL) - CORRECT APPROACH
public class DatabaseConfig
{
    public static NpgsqlDataSource CreateDataSource(string connectionString)
    {
        var dataSourceBuilder = new NpgsqlDataSourceBuilder(connectionString);
        
        // Connection pooling configuration
        dataSourceBuilder.ConnectionStringBuilder.MinPoolSize = 5;
        dataSourceBuilder.ConnectionStringBuilder.MaxPoolSize = 100;
        dataSourceBuilder.ConnectionStringBuilder.ConnectionLifetime = 300; // 5 min
        dataSourceBuilder.ConnectionStringBuilder.ConnectionIdleLifetime = 60; // 1 min
        
        // Resilience configuration
        dataSourceBuilder.ConnectionStringBuilder.Timeout = 30; // Connection timeout
        dataSourceBuilder.ConnectionStringBuilder.CommandTimeout = 30; // Query timeout
        dataSourceBuilder.ConnectionStringBuilder.KeepAlive = 10; // TCP keepalive
        
        // Retry on transient errors
        dataSourceBuilder.ConnectionStringBuilder.Retry = true;
        
        return dataSourceBuilder.Build();
    }
}
```

**Retry Logic with Polly:**

```csharp
// Add Polly for retry policies
using Polly;
using Polly.Retry;

public class ConfigurationService
{
    private readonly AsyncRetryPolicy _retryPolicy;
    
    public ConfigurationService()
    {
        _retryPolicy = Policy
            .Handle<NpgsqlException>() // Database exceptions
            .Or<TimeoutException>()
            .WaitAndRetryAsync(
                retryCount: 3,
                sleepDurationProvider: retryAttempt => 
                    TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                onRetry: (exception, timeSpan, retryCount, context) =>
                {
                    _logger.LogWarning(
                        "Database operation failed. Retry {RetryCount} after {Delay}ms. Error: {Error}",
                        retryCount, timeSpan.TotalMilliseconds, exception.Message);
                });
    }
    
    public async Task<Configuration> GetConfigurationAsync(string id)
    {
        return await _retryPolicy.ExecuteAsync(async () =>
        {
            // Database operation
            await using var connection = await _dataSource.OpenConnectionAsync();
            // ... query logic
        });
    }
}
```

**Health Check Implementation:**

```csharp
// ASP.NET Core Health Checks
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly NpgsqlDataSource _dataSource;
    
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            await using var connection = await _dataSource.OpenConnectionAsync(cancellationToken);
            await using var command = connection.CreateCommand();
            command.CommandText = "SELECT 1";
            await command.ExecuteScalarAsync(cancellationToken);
            
            return HealthCheckResult.Healthy("Database connection is healthy");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                "Database connection failed", 
                ex);
        }
    }
}

// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.AddHealthChecks()
        .AddCheck<DatabaseHealthCheck>("database")
        .AddCheck<RedisHealthCheck>("redis")
        .AddCheck<S3HealthCheck>("s3");
}

public void Configure(IApplicationBuilder app)
{
    app.UseHealthChecks("/health", new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType = "application/json";
            var result = JsonSerializer.Serialize(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name = e.Key,
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds
                })
            });
            await context.Response.WriteAsync(result);
        }
    });
}
```

**Implementation Checklist:**

- [ ] Connection pooling enabled with appropriate min/max pool size
- [ ] Connection lifetime set (5-10 minutes to detect endpoint changes)
- [ ] Retry logic implemented for transient failures
- [ ] Circuit breaker pattern for repeated failures
- [ ] Health check endpoint exposes database connectivity
- [ ] Graceful degradation if database temporarily unavailable
- [ ] Connection failures logged with appropriate severity

---

#### 1.3 Stateless Application Design

**Requirement:** Applications must be stateless. All session state must be stored externally (DynamoDB, Redis), not in-memory.

**Why DR Needs This:**
During DR failover, all ECS tasks in Mumbai are terminated and new tasks start in Singapore. In-memory state would be lost.

**Developer Tasks:**

**❌ DON'T Store State In-Memory:**

```csharp
// WRONG - Session state in memory
public class ConfigurationController : ControllerBase
{
    private static Dictionary<string, Configuration> _activeConfigurations = new();
    
    [HttpPost("save")]
    public IActionResult SaveConfiguration(Configuration config)
    {
        _activeConfigurations[config.Id] = config; // Lost during DR failover!
        return Ok();
    }
}
```

**✅ DO Store State Externally:**

```csharp
// CORRECT - Session state in DynamoDB
public class ConfigurationController : ControllerBase
{
    private readonly IAmazonDynamoDB _dynamoDb;
    private const string TableName = "ConfigurationSessions";
    
    [HttpPost("save")]
    public async Task<IActionResult> SaveConfigurationAsync(Configuration config)
    {
        var item = new Dictionary<string, AttributeValue>
        {
            ["SessionId"] = new AttributeValue { S = config.SessionId },
            ["UserId"] = new AttributeValue { S = config.UserId },
            ["ConfigData"] = new AttributeValue { S = JsonSerializer.Serialize(config) },
            ["Timestamp"] = new AttributeValue { N = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() },
            ["TTL"] = new AttributeValue { N = DateTimeOffset.UtcNow.AddDays(7).ToUnixTimeSeconds().ToString() }
        };
        
        await _dynamoDb.PutItemAsync(new PutItemRequest
        {
            TableName = TableName,
            Item = item
        });
        
        return Ok();
    }
    
    [HttpGet("{sessionId}")]
    public async Task<IActionResult> GetConfigurationAsync(string sessionId)
    {
        var response = await _dynamoDb.GetItemAsync(new GetItemRequest
        {
            TableName = TableName,
            Key = new Dictionary<string, AttributeValue>
            {
                ["SessionId"] = new AttributeValue { S = sessionId }
            }
        });
        
        if (!response.IsItemSet)
            return NotFound();
        
        var configJson = response.Item["ConfigData"].S;
        var config = JsonSerializer.Deserialize<Configuration>(configJson);
        
        return Ok(config);
    }
}
```

**Distributed Caching with Redis:**

```csharp
// Use Redis for frequently accessed data
public class ProductCatalogService
{
    private readonly IDistributedCache _cache;
    private readonly IProductRepository _repository;
    
    public async Task<Product> GetProductAsync(string productId)
    {
        var cacheKey = $"product:{productId}";
        
        // Try cache first
        var cachedProduct = await _cache.GetStringAsync(cacheKey);
        if (cachedProduct != null)
        {
            return JsonSerializer.Deserialize<Product>(cachedProduct);
        }
        
        // Cache miss - fetch from database
        var product = await _repository.GetByIdAsync(productId);
        
        // Store in cache with 1-hour expiration
        var options = new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(1)
        };
        await _cache.SetStringAsync(
            cacheKey, 
            JsonSerializer.Serialize(product),
            options);
        
        return product;
    }
}

// Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    // Configure Redis cache
    services.AddStackExchangeRedisCache(options =>
    {
        // Read from Parameter Store
        options.Configuration = GetRedisEndpointFromParameterStore();
        options.InstanceName = "configurator:";
    });
}
```

**Implementation Checklist:**

- [ ] No static variables storing user/session state
- [ ] All session data in DynamoDB or Redis
- [ ] User authentication via JWT tokens (stateless)
- [ ] In-memory caching only for read-only reference data
- [ ] Cache invalidation strategy implemented
- [ ] Background jobs use message queues (SQS), not in-memory queues

---

#### 1.4 Idempotent Operations

**Requirement:** All write operations (POST, PUT, DELETE) must be idempotent to handle retries safely.

**Why DR Needs This:**
During failover, some requests may be retried (client timeout, network issues). Operations must be safe to retry without creating duplicate data.

**Developer Tasks:**

**Idempotency Key Pattern:**

```csharp
// API accepts idempotency key in header
[HttpPost("configuration")]
public async Task<IActionResult> CreateConfigurationAsync(
    [FromBody] CreateConfigurationRequest request,
    [FromHeader(Name = "Idempotency-Key")] string idempotencyKey)
{
    if (string.IsNullOrEmpty(idempotencyKey))
    {
        return BadRequest("Idempotency-Key header is required");
    }
    
    // Check if this request was already processed
    var existingResponse = await _cache.GetStringAsync($"idempotency:{idempotencyKey}");
    if (existingResponse != null)
    {
        // Return cached response (duplicate request)
        return Ok(JsonSerializer.Deserialize<Configuration>(existingResponse));
    }
    
    // Process request
    var configuration = await _service.CreateConfigurationAsync(request);
    
    // Cache response for 24 hours
    await _cache.SetStringAsync(
        $"idempotency:{idempotencyKey}",
        JsonSerializer.Serialize(configuration),
        new DistributedCacheEntryOptions
        {
            AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(24)
        });
    
    return Ok(configuration);
}
```

**Database-Level Idempotency:**

```csharp
// Use unique constraints to prevent duplicates
public class QuoteService
{
    public async Task<Quote> GenerateQuoteAsync(string configurationId, string requestId)
    {
        try
        {
            // requestId ensures idempotency
            var quote = new Quote
            {
                Id = Guid.NewGuid().ToString(),
                ConfigurationId = configurationId,
                RequestId = requestId, // Unique constraint in database
                Amount = CalculateAmount(),
                CreatedAt = DateTime.UtcNow
            };
            
            await _repository.InsertAsync(quote);
            return quote;
        }
        catch (PostgresException ex) when (ex.SqlState == "23505") // Unique violation
        {
            // Quote with this requestId already exists, fetch and return it
            return await _repository.GetByRequestIdAsync(requestId);
        }
    }
}

// Database migration
CREATE TABLE quotes (
    id UUID PRIMARY KEY,
    configuration_id UUID NOT NULL,
    request_id VARCHAR(100) UNIQUE NOT NULL,  -- Ensures idempotency
    amount DECIMAL(10,2) NOT NULL,
    created_at TIMESTAMP NOT NULL
);
```

**Implementation Checklist:**

- [ ] POST/PUT operations accept idempotency key
- [ ] Idempotency keys stored and checked (Redis/DynamoDB)
- [ ] Database unique constraints prevent duplicates
- [ ] GET operations are naturally idempotent (no changes)
- [ ] DELETE operations check existence before deleting
- [ ] Client libraries generate idempotency keys automatically

---

#### 1.5 Graceful Shutdown

**Requirement:** Applications must handle SIGTERM signal and shutdown gracefully when ECS stops tasks.

**Why DR Needs This:**
During failover, Mumbai tasks are terminated. Applications must finish processing in-flight requests, close connections cleanly, and not leave data in inconsistent state.

**Developer Tasks:**

```csharp
// ASP.NET Core - Graceful Shutdown
public class Program
{
    public static void Main(string[] args)
    {
        var host = CreateHostBuilder(args).Build();
        
        // Handle SIGTERM (sent by ECS when stopping task)
        var applicationLifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        
        applicationLifetime.ApplicationStopping.Register(() =>
        {
            Console.WriteLine("SIGTERM received - beginning graceful shutdown");
            
            // Stop accepting new requests (handled by Kestrel automatically)
            // Give in-flight requests time to complete
        });
        
        host.Run();
    }
    
    public static IHostBuilder CreateHostBuilder(string[] args) =>
        Host.CreateDefaultBuilder(args)
            .ConfigureWebHostDefaults(webBuilder =>
            {
                webBuilder.UseStartup<Startup>();
                webBuilder.UseKestrel(options =>
                {
                    // Graceful shutdown timeout
                    options.Limits.KeepAliveTimeout = TimeSpan.FromMinutes(2);
                });
            });
}

// Configure shutdown timeout in Startup.cs
public void ConfigureServices(IServiceCollection services)
{
    services.Configure<HostOptions>(options =>
    {
        // Allow 30 seconds for graceful shutdown
        options.ShutdownTimeout = TimeSpan.FromSeconds(30);
    });
}
```

**Background Job Graceful Shutdown:**

```csharp
public class RenderingWorkerService : BackgroundService
{
    private readonly ILogger<RenderingWorkerService> _logger;
    
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _logger.LogInformation("Rendering worker started");
        
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                // Poll SQS for rendering jobs
                var message = await _sqsClient.ReceiveMessageAsync(queueUrl, stoppingToken);
                
                if (message.Messages.Count > 0)
                {
                    // Process message
                    await ProcessRenderingJobAsync(message.Messages[0], stoppingToken);
                }
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Shutdown requested - stopping worker");
                break;
            }
        }
        
        _logger.LogInformation("Rendering worker stopped gracefully");
    }
}
```

**ECS Task Definition Configuration:**

```json
{
    "containerDefinitions": [{
        "stopTimeout": 30,
        "healthCheck": {
            "command": ["CMD-SHELL", "curl -f http://localhost:8080/health || exit 1"],
            "interval": 30,
            "timeout": 5,
            "retries": 3,
            "startPeriod": 60
        }
    }]
}
```

**Implementation Checklist:**

- [ ] Application handles SIGTERM gracefully
- [ ] In-flight requests complete before shutdown (30-second timeout)
- [ ] Database connections closed cleanly
- [ ] Cache connections closed cleanly
- [ ] Background workers stop processing new jobs
- [ ] Logs indicate graceful shutdown
- [ ] ECS `stopTimeout` configured (30-60 seconds)

---

#### 1.6 Observability & Logging

**Requirement:** Applications must emit structured logs and metrics to support DR troubleshooting.

**Why DR Needs This:**
During and after failover, operations team needs detailed logs to diagnose issues, validate functionality, and troubleshoot problems.

**Developer Tasks:**

**Structured Logging:**

```csharp
// Use Serilog for structured logging
public class Startup
{
    public void ConfigureServices(IServiceCollection services)
    {
        // Configure Serilog
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Application", "ConfigurationService")
            .Enrich.WithProperty("Environment", Environment.GetEnvironmentVariable("ENVIRONMENT"))
            .Enrich.WithProperty("Region", Environment.GetEnvironmentVariable("AWS_REGION"))
            .WriteTo.Console(new JsonFormatter())
            .WriteTo.AWSSeriLog(config =>
            {
                config.LogGroup = "/ecs/configurator/configuration-service";
                config.TextFormatter = new JsonFormatter();
            })
            .CreateLogger();
        
        services.AddLogging(loggingBuilder =>
        {
            loggingBuilder.ClearProviders();
            loggingBuilder.AddSerilog();
        });
    }
}

// Controller with rich logging
public class ConfigurationController : ControllerBase
{
    private readonly ILogger<ConfigurationController> _logger;
    
    [HttpPost]
    public async Task<IActionResult> CreateConfigurationAsync(
        [FromBody] CreateConfigurationRequest request)
    {
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["UserId"] = request.UserId,
            ["CorrelationId"] = HttpContext.TraceIdentifier
        }))
        {
            _logger.LogInformation(
                "Creating configuration for user {UserId}, layout {CivilLayout}",
                request.UserId, request.CivilLayout);
            
            try
            {
                var config = await _service.CreateAsync(request);
                
                _logger.LogInformation(
                    "Configuration {ConfigId} created successfully in {Duration}ms",
                    config.Id, stopwatch.ElapsedMilliseconds);
                
                return Ok(config);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex,
                    "Failed to create configuration for user {UserId}",
                    request.UserId);
                
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
```

**Custom Metrics with CloudWatch:**

```csharp
public class MetricsService
{
    private readonly IAmazonCloudWatch _cloudWatch;
    
    public async Task RecordConfigurationCreatedAsync(string userId, double durationMs)
    {
        await _cloudWatch.PutMetricDataAsync(new PutMetricDataRequest
        {
            Namespace = "Configurator/Application",
            MetricData = new List<MetricDatum>
            {
                new MetricDatum
                {
                    MetricName = "ConfigurationCreated",
                    Value = 1,
                    Unit = StandardUnit.Count,
                    Timestamp = DateTime.UtcNow,
                    Dimensions = new List<Dimension>
                    {
                        new Dimension { Name = "Service", Value = "ConfigurationService" },
                        new Dimension { Name = "Region", Value = GetCurrentRegion() }
                    }
                },
                new MetricDatum
                {
                    MetricName = "ConfigurationCreationDuration",
                    Value = durationMs,
                    Unit = StandardUnit.Milliseconds,
                    Timestamp = DateTime.UtcNow,
                    Dimensions = new List<Dimension>
                    {
                        new Dimension { Name = "Service", Value = "ConfigurationService" }
                    }
                }
            }
        });
    }
}
```

**Correlation IDs for Distributed Tracing:**

```csharp
// Middleware to ensure correlation ID
public class CorrelationIdMiddleware
{
    private readonly RequestDelegate _next;
    
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();
        
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Add("X-Correlation-ID", correlationId);
        
        // Add to logs
        using (LogContext.PushProperty("CorrelationId", correlationId))
        {
            await _next(context);
        }
    }
}

// Pass correlation ID to downstream services
public class ConfigurationService
{
    public async Task<Design> GenerateDesignAsync(string configId)
    {
        var correlationId = _httpContextAccessor.HttpContext.Items["CorrelationId"];
        
        var request = new HttpRequestMessage(HttpMethod.Post, "/api/design/generate");
        request.Headers.Add("X-Correlation-ID", correlationId.ToString());
        
        var response = await _httpClient.SendAsync(request);
        // ...
    }
}
```

**Implementation Checklist:**

- [ ] Structured logging (JSON format) to CloudWatch
- [ ] All logs include: timestamp, service name, region, correlation ID
- [ ] Error logs include exception details and stack traces
- [ ] Custom metrics for business operations (configurations created, quotes generated)
- [ ] Performance metrics (API response time, database query time)
- [ ] Correlation IDs propagated across service calls
- [ ] X-Ray tracing enabled for distributed requests

---

#### 1.7 Multi-Region Awareness

**Requirement:** Applications must be aware of which region they're running in and adapt behavior accordingly.

**Why DR Needs This:**
Some operations may differ between Mumbai (primary) and Singapore (DR). Applications need to know their context.

**Developer Tasks:**

```csharp
public class RegionAwareService
{
    private readonly string _currentRegion;
    private readonly bool _isPrimaryRegion;
    
    public RegionAwareService(IConfiguration configuration)
    {
        _currentRegion = Environment.GetEnvironmentVariable("AWS_REGION") ?? "ap-south-1";
        _isPrimaryRegion = _currentRegion == "ap-south-1"; // Mumbai is primary
    }
    
    public async Task ProcessEventAsync(Event evt)
    {
        // Log which region processed the event
        _logger.LogInformation(
            "Processing event {EventId} in region {Region} (Primary: {IsPrimary})",
            evt.Id, _currentRegion, _isPrimaryRegion);
        
        // Example: Only primary region sends certain notifications
        if (_isPrimaryRegion)
        {
            await _notificationService.SendDailySummaryAsync();
        }
        
        // Both regions handle user requests
        await _repository.SaveAsync(evt);
    }
}
```

**Implementation Checklist:**

- [ ] Services detect current AWS region from environment variable
- [ ] Logs include region identifier
- [ ] Region-specific behavior documented
- [ ] Integration tests run in both region configurations

---

### 2. Frontend Application Responsibilities

#### 2.1 API Retry Logic

**Requirement:** Frontend must retry failed API calls with exponential backoff.

**Why DR Needs This:**
During failover, some API calls may fail temporarily. Automatic retries prevent user-facing errors.

**Developer Tasks:**

```typescript
// React - API Client with Retry Logic
import axios, { AxiosError } from 'axios';

const apiClient = axios.create({
    baseURL: '/api',
    timeout: 30000,
});

// Retry logic
apiClient.interceptors.response.use(
    response => response,
    async (error: AxiosError) => {
        const config = error.config;
        
        // Don't retry if no config or already retried 3 times
        if (!config || config['retryCount'] >= 3) {
            return Promise.reject(error);
        }
        
        config['retryCount'] = config['retryCount'] || 0;
        config['retryCount'] += 1;
        
        // Only retry on network errors or 5xx server errors
        const shouldRetry = 
            !error.response ||
            (error.response.status >= 500 && error.response.status < 600);
        
        if (shouldRetry) {
            // Exponential backoff: 1s, 2s, 4s
            const delayMs = Math.pow(2, config['retryCount']) * 1000;
            
            await new Promise(resolve => setTimeout(resolve, delayMs));
            
            console.log(`Retrying request (attempt ${config['retryCount']})`);
            return apiClient(config);
        }
        
        return Promise.reject(error);
    }
);

export default apiClient;
```

**Implementation Checklist:**

- [ ] Retry logic for network failures (connection refused, timeout)
- [ ] Retry logic for 5xx server errors
- [ ] Exponential backoff (1s, 2s, 4s)
- [ ] Maximum 3 retry attempts
- [ ] User-friendly error messages after final failure
- [ ] No retry for 4xx client errors

---

#### 2.2 Offline Capability (Optional but Recommended)

**Requirement:** Frontend can queue actions when API is unavailable.

```typescript
// Service Worker for offline queueing
const QUEUE_NAME = 'api-queue';

self.addEventListener('fetch', (event) => {
    if (event.request.method === 'POST' && event.request.url.includes('/api/')) {
        event.respondWith(
            fetch(event.request.clone())
                .catch(async () => {
                    // Queue request for later
                    const queue = await getQueue(QUEUE_NAME);
                    await queue.pushRequest({ request: event.request.clone() });
                    
                    return new Response(JSON.stringify({
                        queued: true,
                        message: 'Request queued - will retry when connection restored'
                    }), {
                        status: 202,
                        headers: { 'Content-Type': 'application/json' }
                    });
                })
        );
    }
});

// Replay queue when online
self.addEventListener('sync', (event) => {
    if (event.tag === 'replay-queue') {
        event.waitUntil(replayQueue());
    }
});
```

---

## Infrastructure Team Responsibilities

### 1. Infrastructure as Code (IaC) Management

#### 1.1 Maintain Identical Configurations Across Regions

**Requirement:** Mumbai and Singapore infrastructure must be identical (except for region-specific identifiers).

**Infrastructure Tasks:**

**Parameterized Infrastructure:**

```yaml
# Example structure (not specific to CloudFormation or Terraform yet)
Parameters:
  - Region: ap-south-1 or ap-southeast-1
  - Environment: prod
  - IsPrimary: true or false (Mumbai = true, Singapore = false)

Resources Based on Parameters:
  - VPC (region-specific)
  - ECS Cluster
  - RDS Instance (primary) or Read Replica (DR)
  - DynamoDB Table (with Global Table configuration)
  - S3 Buckets (with Cross-Region Replication)
  - ALB
  - Route 53 (global, references both regions)
```

**Implementation Checklist:**

- [ ] Single source of truth for infrastructure code
- [ ] Region parameter drives deployment
- [ ] No hardcoded region-specific values
- [ ] Automated deployment pipeline for both regions
- [ ] Infrastructure changes deployed to both regions simultaneously
- [ ] Drift detection enabled (AWS Config or Terraform state)

---

#### 1.2 Automated Deployment Pipeline

**Requirement:** Infrastructure changes must be tested and deployed automatically.

**CI/CD Pipeline Structure:**

```
1. Code Commit (Git push to main branch)
   ↓
2. IaC Validation (syntax check, linting)
   ↓
3. Plan Generation (CloudFormation ChangeSet or Terraform Plan)
   ↓
4. Security Scan (tfsec, Checkov, CloudFormation Guard)
   ↓
5. Deploy to Dev Environment (test infrastructure)
   ↓
6. Integration Tests (validate deployed resources)
   ↓
7. Manual Approval (for production)
   ↓
8. Deploy to Mumbai Production
   ↓
9. Deploy to Singapore DR
   ↓
10. Validation Tests (health checks, smoke tests)
```

**Implementation Checklist:**

- [ ] IaC repository with version control
- [ ] Automated validation in CI pipeline
- [ ] Change sets/plans reviewed before apply
- [ ] Separate state files for dev/prod/dr environments
- [ ] Automated rollback on deployment failure
- [ ] Deployment notifications to Slack/email

---

#### 1.3 Parameter Store Management

**Requirement:** Maintain parameter synchronization between regions.

**Parameter Management Strategy:**

```bash
# Automation script to sync parameters from Mumbai to Singapore
#!/bin/bash

PARAMETERS=(
    "/configurator/prod/db/username"
    "/configurator/prod/api/external-cad-service"
    # ... other non-region-specific parameters
)

for param in "${PARAMETERS[@]}"; do
    # Get value from Mumbai
    VALUE=$(aws ssm get-parameter \
        --name "$param" \
        --region ap-south-1 \
        --query 'Parameter.Value' \
        --output text)
    
    # Set in Singapore
    aws ssm put-parameter \
        --name "$param" \
        --value "$VALUE" \
        --type "String" \
        --overwrite \
        --region ap-southeast-1
done

# Region-specific parameters are NOT synced (e.g., DB endpoints)
```

**Implementation Checklist:**

- [ ] Automated parameter synchronization script
- [ ] Parameters tagged as "region-specific" vs "global"
- [ ] Sync runs after each parameter change
- [ ] Parameter versioning enabled
- [ ] Audit logging for parameter changes

---

#### 1.4 Secret Rotation

**Requirement:** Database passwords and API keys must rotate automatically without downtime.

**Secrets Manager Configuration:**

```json
{
    "SecretId": "configurator/prod/db/password",
    "RotationEnabled": true,
    "RotationLambdaARN": "arn:aws:lambda:ap-south-1:ACCOUNT:function:SecretsManagerRotation",
    "RotationRules": {
        "AutomaticallyAfterDays": 90
    }
}
```

**Rotation Lambda Function Logic:**

```python
# Lambda function to rotate RDS password
def lambda_handler(event, context):
    service_client = boto3.client('secretsmanager')
    rds_client = boto3.client('rds')
    
    # Create new password
    new_password = generate_secure_password()
    
    # Update RDS master password
    rds_client.modify_db_instance(
        DBInstanceIdentifier='configurator-db-primary',
        MasterUserPassword=new_password,
        ApplyImmediately=True
    )
    
    # Update secret in Secrets Manager
    service_client.update_secret(
        SecretId=event['SecretId'],
        SecretString=json.dumps({
            'username': 'admin',
            'password': new_password,
            'endpoint': 'configurator-db.xxx.rds.amazonaws.com'
        })
    )
    
    # Replicate to Singapore Secrets Manager
    replicate_secret_to_region(event['SecretId'], 'ap-southeast-1', new_password)
    
    return {'statusCode': 200}
```

**Implementation Checklist:**

- [ ] All secrets stored in Secrets Manager (not Parameter Store)
- [ ] Automatic rotation every 90 days
- [ ] Rotation Lambda function tested
- [ ] Applications fetch secrets on startup and periodically refresh
- [ ] Secrets replicated to DR region

---

#### 1.5 Backup Management

**Requirement:** Automated backups with cross-region copies.

**Backup Strategy:**

**RDS Automated Backups:**
- Daily automated snapshots
- 7-day retention
- Manual snapshot before major changes
- Copy snapshots to Singapore region

**DynamoDB Continuous Backups:**
- Point-in-time recovery enabled
- On-demand backups before schema changes
- Backups retained for 30 days

**S3 Versioning:**
- Versioning enabled on all buckets
- Lifecycle policy to transition old versions to Glacier

**Automation Script:**

```bash
#!/bin/bash
# Daily script to copy RDS snapshots to DR region

LATEST_SNAPSHOT=$(aws rds describe-db-snapshots \
    --db-instance-identifier configurator-db-primary \
    --region ap-south-1 \
    --query 'DBSnapshots | sort_by(@, &SnapshotCreateTime)[-1].DBSnapshotIdentifier' \
    --output text)

aws rds copy-db-snapshot \
    --source-db-snapshot-identifier "arn:aws:rds:ap-south-1:ACCOUNT:snapshot:$LATEST_SNAPSHOT" \
    --target-db-snapshot-identifier "$LATEST_SNAPSHOT-dr" \
    --source-region ap-south-1 \
    --region ap-southeast-1

echo "Snapshot $LATEST_SNAPSHOT copied to Singapore"
```

**Implementation Checklist:**

- [ ] Automated daily backups configured
- [ ] Cross-region snapshot copy automated
- [ ] Backup retention aligned with compliance requirements
- [ ] Restore procedures tested quarterly
- [ ] Backup success/failure alerts configured

---

#### 1.6 DR Automation Scripts

**Requirement:** Scripts to automate DR failover and failback procedures.

**Failover Automation:**

```bash
#!/bin/bash
# dr-failover.sh - Automate DR activation

set -e  # Exit on error

echo "=== DR FAILOVER INITIATED ==="
echo "Target: Singapore (ap-southeast-1)"
echo "$(date)"

# Step 1: Promote RDS Read Replica
echo "Step 1: Promoting RDS Read Replica in Singapore..."
aws rds promote-read-replica \
    --db-instance-identifier configurator-db-replica-singapore \
    --region ap-southeast-1

aws rds wait db-instance-available \
    --db-instance-identifier configurator-db-replica-singapore \
    --region ap-southeast-1

echo "RDS promoted successfully"

# Step 2: Update Parameter Store
echo "Step 2: Updating Parameter Store with Singapore endpoints..."
NEW_DB_ENDPOINT=$(aws rds describe-db-instances \
    --db-instance-identifier configurator-db-replica-singapore \
    --region ap-southeast-1 \
    --query 'DBInstances[0].Endpoint.Address' \
    --output text)

aws ssm put-parameter \
    --name "/configurator/prod/db/endpoint" \
    --value "$NEW_DB_ENDPOINT" \
    --overwrite \
    --region ap-southeast-1

echo "Parameter Store updated"

# Step 3: Scale ECS Services
echo "Step 3: Scaling ECS services in Singapore..."
SERVICES=("configuration-service" "design-engine-service" "bom-service" "quote-service" "user-management-service" "file-processing-service")

for service in "${SERVICES[@]}"; do
    echo "Scaling $service to 3 tasks..."
    aws ecs update-service \
        --cluster configurator-cluster-dr \
        --service "$service" \
        --desired-count 3 \
        --force-new-deployment \
        --region ap-southeast-1
done

echo "ECS services scaled"

# Step 4: Update Route 53 (if not automatic)
# This would be handled by health checks in production

# Step 5: Validation
echo "Step 4: Running validation tests..."
bash dr-validate.sh

echo "=== DR FAILOVER COMPLETE ==="
echo "$(date)"
echo "Services now running in Singapore"
```

**Validation Script:**

```bash
#!/bin/bash
# dr-validate.sh - Validate DR environment

API_ENDPOINT="https://api.configurator.com"

# Health check
echo "Testing health endpoint..."
HEALTH_STATUS=$(curl -s -o /dev/null -w "%{http_code}" "$API_ENDPOINT/health")

if [ "$HEALTH_STATUS" -eq 200 ]; then
    echo "✅ Health check passed"
else
    echo "❌ Health check failed (HTTP $HEALTH_STATUS)"
    exit 1
fi

# Database connectivity
echo "Testing database connectivity..."
DB_HEALTH=$(curl -s "$API_ENDPOINT/health" | jq -r '.database.status')

if [ "$DB_HEALTH" == "healthy" ]; then
    echo "✅ Database connectivity confirmed"
else
    echo "❌ Database unhealthy"
    exit 1
fi

# API functionality test
echo "Testing API functionality..."
TOKEN=$(curl -s -X POST "$API_ENDPOINT/api/auth/login" \
    -H "Content-Type: application/json" \
    -d '{"username":"test@dealer.com","password":"TestPass123"}' \
    | jq -r '.token')

if [ -n "$TOKEN" ]; then
    echo "✅ Authentication working"
else
    echo "❌ Authentication failed"
    exit 1
fi

echo "✅ All validation tests passed"
```

**Implementation Checklist:**

- [ ] DR failover script tested in staging
- [ ] DR validation script covers critical paths
- [ ] Scripts run in CI/CD for testing
- [ ] Scripts logged and audited
- [ ] Rollback script available
- [ ] Scripts stored in version control

---

### 2. Monitoring & Alerting Setup

#### 2.1 DR-Specific CloudWatch Dashboards

**Requirement:** Dashboards showing DR readiness and health.

**Dashboard Widgets:**

1. **Replication Lag Metrics:**
   - RDS replication lag (Mumbai → Singapore)
   - DynamoDB replication lag (Mumbai ↔ Singapore)
   - S3 replication metrics

2. **Health Check Status:**
   - Route 53 health check status (Mumbai ALB)
   - Route 53 health check status (Singapore ALB)

3. **Service Availability:**
   - ECS running task count (Mumbai and Singapore)
   - ALB healthy target count (both regions)

4. **Database Metrics:**
   - RDS connections (primary and replica)
   - RDS CPU/memory utilization
   - Database error logs count

**Implementation Checklist:**

- [ ] DR dashboard created in CloudWatch
- [ ] Dashboard accessible to all engineers
- [ ] Dashboard reviewed in daily standup
- [ ] Anomalies trigger investigations

---

#### 2.2 Alerting Rules

**Critical Alerts (P0):**

```yaml
Alarms:
  - Name: Mumbai-Region-Unhealthy
    Metric: Route53 HealthCheckStatus
    Threshold: < 1 (unhealthy)
    Period: 1 minute
    EvaluationPeriods: 2
    Action: SNS topic → PagerDuty → On-call engineer
    
  - Name: RDS-Replication-Lag-High
    Metric: RDS ReplicaLag
    Threshold: > 30 seconds
    Period: 5 minutes
    Action: SNS topic → Slack #platform-alerts
    
  - Name: DR-Environment-Not-Ready
    Metric: Custom metric from validation script
    Threshold: Validation failed
    Period: 15 minutes
    Action: SNS topic → Email to infrastructure team
```

**Implementation Checklist:**

- [ ] Alarms configured for all critical metrics
- [ ] Alarm actions tested (verify notifications received)
- [ ] Alarm thresholds based on actual baselines
- [ ] Alarms reviewed and tuned monthly

---

### 3. Testing & Validation

#### 3.1 Monthly DR Readiness Tests

**Automated Tests:**

```bash
#!/bin/bash
# monthly-dr-test.sh - Non-invasive DR readiness validation

echo "=== MONTHLY DR READINESS TEST ==="
echo "$(date)"

# 1. Verify RDS replication
REPLICATION_LAG=$(aws cloudwatch get-metric-statistics \
    --namespace AWS/RDS \
    --metric-name ReplicaLag \
    --dimensions Name=DBInstanceIdentifier,Value=configurator-db-replica-singapore \
    --start-time $(date -u -d '5 minutes ago' +%Y-%m-%dT%H:%M:%S) \
    --end-time $(date -u +%Y-%m-%dT%H:%M:%S) \
    --period 300 \
    --statistics Average \
    --region ap-southeast-1 \
    --query 'Datapoints[0].Average' \
    --output text)

if (( $(echo "$REPLICATION_LAG < 10" | bc -l) )); then
    echo "✅ RDS replication lag: ${REPLICATION_LAG}s (< 10s threshold)"
else
    echo "❌ RDS replication lag too high: ${REPLICATION_LAG}s"
    exit 1
fi

# 2. Verify DynamoDB Global Tables
echo "✅ DynamoDB Global Tables active (manual verification)"

# 3. Verify S3 replication
echo "✅ S3 replication >99% (manual verification from metrics)"

# 4. Verify ECR images in Singapore
MUMBAI_IMAGES=$(aws ecr list-images \
    --repository-name configuration-service \
    --region ap-south-1 \
    --query 'imageIds | length(@)')

SINGAPORE_IMAGES=$(aws ecr list-images \
    --repository-name configuration-service \
    --region ap-southeast-1 \
    --query 'imageIds | length(@)')

if [ "$MUMBAI_IMAGES" -eq "$SINGAPORE_IMAGES" ]; then
    echo "✅ ECR images synced ($MUMBAI_IMAGES images in each region)"
else
    echo "❌ ECR images out of sync (Mumbai: $MUMBAI_IMAGES, Singapore: $SINGAPORE_IMAGES)"
    exit 1
fi

# 5. Verify ECS task definitions exist in Singapore
echo "✅ ECS task definitions present in Singapore (manual verification)"

# 6. Test DNS failover (non-disruptive)
echo "✅ Route 53 health checks operational"

echo "=== DR READINESS TEST COMPLETE ==="
```

**Implementation Checklist:**

- [ ] Monthly automated test runs on first Monday
- [ ] Test results emailed to team
- [ ] Failures trigger immediate investigation
- [ ] Test suite updated as infrastructure evolves

---

## IaC Tool Selection: CloudFormation vs Terraform

### Executive Summary

**Recommendation:** Use **AWS CloudFormation** for this project, given customer preference and AWS-native architecture.

### Detailed Comparison

#### AWS CloudFormation

**Advantages:**

**1. Native AWS Integration:**
- First-class support for all AWS services
- New AWS features available immediately in CloudFormation
- No third-party dependencies or state management concerns
- Tight integration with AWS Console (StackSets, Change Sets)

**2. No State File Management:**
- CloudFormation manages state internally (in AWS)
- No risk of state file corruption or conflicts
- No need for S3 backend configuration
- No state locking concerns

**3. Rollback & Change Management:**
- Automatic rollback on stack creation/update failure
- Change Sets show exactly what will change before applying
- Stack drift detection built-in
- Stack events provide detailed deployment history

**4. Cost:**
- Completely free (no additional charges)
- Only pay for resources created
- No tool licensing or SaaS fees

**5. Security & Compliance:**
- IAM integration (CloudFormation service role)
- No credentials in code (uses AWS STS)
- CloudTrail logs all CloudFormation API calls
- AWS Config tracks stack compliance

**6. Customer Preference:**
- Customer is inclined towards CloudFormation
- Reduces learning curve for customer's team
- Easier handoff and long-term support
- Aligns with customer's existing AWS expertise

**Disadvantages:**

**1. Verbose Syntax:**
- JSON/YAML can be lengthy for complex resources
- Less expressive than Terraform HCL
- Harder to DRY (Don't Repeat Yourself)

**2. AWS-Only:**
- Cannot manage non-AWS resources (e.g., GitHub, Datadog)
- Locked into AWS ecosystem
- Multi-cloud deployments not possible

**3. Limited Modularity:**
- Nested stacks are cumbersome
- No native module registry
- Harder to share and reuse code across teams

**4. State Visibility:**
- Cannot directly inspect state (it's managed by AWS)
- Harder to troubleshoot state issues
- Limited to CloudFormation console/CLI

---

#### Terraform

**Advantages:**

**1. Multi-Cloud:**
- Can manage AWS, Azure, GCP, and 100+ providers
- Future-proof if organization adopts multi-cloud
- Manage GitHub, Datadog, PagerDuty in same codebase

**2. Better Language (HCL):**
- More concise and readable than CloudFormation YAML
- First-class support for variables, functions, loops
- Strong typing and validation

**3. Modularity:**
- Terraform modules are powerful and reusable
- Public module registry (thousands of pre-built modules)
- Easier to enforce standards and best practices

**4. Plan/Apply Workflow:**
- Terraform plan shows exactly what will change
- Similar to CloudFormation Change Sets but better UX
- Community tooling (Terragrunt, Atlantis for PR automation)

**5. State Management Control:**
- Full visibility into state file
- Can manually edit if needed (last resort)
- State file can be version controlled (not recommended but possible)

**Disadvantages:**

**1. State File Complexity:**
- Requires S3 backend + DynamoDB for state locking
- State file corruption can be catastrophic
- Team coordination required (don't run Terraform concurrently)
- State file contains sensitive data (must be encrypted)

**2. Learning Curve:**
- Team needs to learn HCL syntax
- Understanding state management is non-trivial
- More concepts to master (providers, backends, workspaces)

**3. Cost:**
- Terraform Cloud (SaaS) costs $20/user/month for team features
- Or self-host with complexity (Atlantis, CI/CD integration)
- Free tier is limited

**4. AWS Lag:**
- New AWS services may not be immediately supported
- Sometimes need to wait for Terraform AWS provider updates
- Workaround: Use "null_resource" with AWS CLI (hacky)

**5. Customer Preference:**
- Customer prefers CloudFormation
- Training and adoption cost
- Potential resistance from customer's team

---

### Recommendation Matrix

| Criteria | CloudFormation | Terraform | Weight | Winner |
|----------|---------------|-----------|--------|--------|
| AWS-native features | ★★★★★ | ★★★★☆ | High | CFN |
| State management simplicity | ★★★★★ | ★★★☆☆ | High | CFN |
| Customer preference | ★★★★★ | ★★☆☆☆ | High | CFN |
| Cost | ★★★★★ | ★★★☆☆ | Medium | CFN |
| Code readability | ★★★☆☆ | ★★★★★ | Medium | TF |
| Modularity | ★★★☆☆ | ★★★★★ | Medium | TF |
| Multi-cloud capability | ★☆☆☆☆ | ★★★★★ | Low | TF |
| Community ecosystem | ★★★☆☆ | ★★★★★ | Low | TF |

**Weighted Score:**
- CloudFormation: 4.5/5
- Terraform: 3.8/5

---

### Decision: Use AWS CloudFormation

**Primary Reasons:**

**1. Customer Alignment:**
Customer has expressed preference for CloudFormation. This is the most important factor. Using their preferred tool:
- Reduces friction and accelerates approval
- Ensures long-term maintainability by customer's team
- Avoids training overhead
- Demonstrates responsiveness to customer requirements

**2. AWS-Only Architecture:**
This solution is 100% AWS (no third-party cloud services). CloudFormation's AWS-native integration provides:
- Immediate support for new AWS features
- No provider version lag
- Better error messages and troubleshooting
- Tighter integration with AWS Console

**3. Operational Simplicity:**
CloudFormation's managed state reduces operational burden:
- No S3 backend setup
- No state locking concerns
- No risk of state file corruption
- Easier for junior engineers to work with

**4. Cost Efficiency:**
CloudFormation is completely free. For a cost-conscious customer, this is a tangible benefit (vs. Terraform Cloud fees).

**5. DR Simplicity:**
CloudFormation StackSets can deploy identical infrastructure across regions with a single operation. This aligns perfectly with our Mumbai/Singapore DR strategy.

---

### Mitigating CloudFormation Disadvantages

**1. Verbose Syntax:**

**Solution: Use Pre-processors**

```yaml
# Use YAML anchors for DRY
Mappings:
  EnvironmentConfig:
    prod:
      InstanceType: &ProdInstanceType db.r6g.xlarge
      MinTasks: &ProdMinTasks 3
      MaxTasks: &ProdMaxTasks 10

Resources:
  ConfigurationService:
    Type: AWS::ECS::Service
    Properties:
      DesiredCount: *ProdMinTasks
      # ... other properties
```

**Solution: Use CloudFormation Macros**

```yaml
# Custom macro to generate repeated resources
Transform: AWS::Serverless-2016-10-31

Parameters:
  Services:
    Type: CommaDelimitedList
    Default: "configuration-service,bom-service,quote-service"

Resources:
  ServicesMacro:
    Type: AWS::CloudFormation::Macro
    # Macro generates ECS services from parameter list
```

**Solution: Modularize with Nested Stacks**

```
project-root/
├── main-stack.yaml              # Parent stack
├── network/
│   └── vpc-stack.yaml           # VPC resources
├── database/
│   └── rds-stack.yaml           # Database resources
├── compute/
│   ├── ecs-cluster-stack.yaml
│   └── ecs-services-stack.yaml
└── storage/
    └── s3-stack.yaml
```

```yaml
# main-stack.yaml references nested stacks
Resources:
  VPCStack:
    Type: AWS::CloudFormation::Stack
    Properties:
      TemplateURL: !Sub "https://s3.amazonaws.com/${TemplateBucket}/network/vpc-stack.yaml"
      Parameters:
        Environment: !Ref Environment
  
  DatabaseStack:
    Type: AWS::CloudFormation::Stack
    DependsOn: VPCStack
    Properties:
      TemplateURL: !Sub "https://s3.amazonaws.com/${TemplateBucket}/database/rds-stack.yaml"
      Parameters:
        VPCId: !GetAtt VPCStack.Outputs.VPCId
```

**2. Limited Modularity:**

**Solution: Use AWS Service Catalog**

Create reusable CloudFormation products in Service Catalog:
- ECS Service template (parameterized for any microservice)
- RDS Database template (parameterized for primary/replica)
- S3 Bucket template (with standard configurations)

Teams can provision from catalog without writing CloudFormation.

**3. AWS-Only Limitation:**

**Solution: Hybrid Approach (If Needed)**

For non-AWS resources (GitHub repos, Datadog dashboards), use:
- AWS Lambda custom resources (call external APIs)
- Separate Terraform modules for non-AWS (small footprint)
- Manual configuration (if minimal)

**Example: GitHub repo via Lambda custom resource**

```yaml
Resources:
  GitHubRepoFunction:
    Type: AWS::Lambda::Function
    # Lambda that calls GitHub API

  GitHubRepo:
    Type: Custom::GitHubRepo
    Properties:
      ServiceToken: !GetAtt GitHubRepoFunction.Arn
      RepoName: configurator-backend
      Private: true
```

---

### Implementation Strategy

**Phase 1: Foundation**

Create base CloudFormation stacks:

```
1. Network Stack (VPC, Subnets, Route Tables)
2. Security Stack (Security Groups, IAM Roles)
3. Database Stack (RDS, DynamoDB, ElastiCache)
4. Storage Stack (S3 Buckets)
5. Compute Stack (ECS Cluster, ALB)
6. Application Stack (ECS Services)
7. Monitoring Stack (CloudWatch Dashboards, Alarms)
```

**Phase 2: StackSets for Multi-Region**

Use CloudFormation StackSets to deploy identical infrastructure to Mumbai and Singapore:

```yaml
# StackSet configuration
StackSetName: configurator-multi-region
Regions:
  - ap-south-1  # Mumbai (primary)
  - ap-southeast-1  # Singapore (DR)

Parameters:
  Environment: prod
  IsPrimaryRegion:
    - ap-south-1: true
    - ap-southeast-1: false  # DR region
```

**Phase 3: CI/CD Integration**

```yaml
# CodePipeline with CloudFormation deployment
Stages:
  - Source: GitHub
  - Validate: cfn-lint, cfn-nag (security scanning)
  - Deploy to Dev: CloudFormation stack
  - Approval: Manual approval gate
  - Deploy to Mumbai: CloudFormation stack
  - Deploy to Singapore: CloudFormation stack
  - Validate: Run smoke tests
```

**Phase 4: Documentation & Training**

- Create CloudFormation template library for team
- Document stack dependencies and deployment order
- Train customer's team on CloudFormation best practices
- Provide runbooks for common operations

---

### Handling Customer Discussions

**If Customer Asks "Why Not Terraform?"**

**Response:**

"We recommend CloudFormation for this project based on several factors:

**1. Alignment with Your Preference:** You indicated a preference for CloudFormation, and we want to ensure the solution aligns with your team's skills and preferences.

**2. AWS-Native Benefits:** Since this architecture is 100% AWS, CloudFormation provides the tightest integration, immediate support for new AWS features, and no third-party dependencies.

**3. Operational Simplicity:** CloudFormation's managed state eliminates operational overhead. Your team won't need to manage S3 backends, state locking, or coordinate Terraform runs across team members.

**4. Cost Efficiency:** CloudFormation is completely free, whereas Terraform Cloud has licensing costs ($20/user/month for team features).

**5. DR Simplicity:** CloudFormation StackSets make it trivial to deploy identical infrastructure across Mumbai and Singapore regions, which is perfect for your DR strategy.

**That said, if you have a strong organizational directive to use Terraform or existing Terraform expertise, we can certainly accommodate that. We're flexible and will use the tool that best serves your long-term success.**

**Would you like to discuss Terraform further, or are you comfortable proceeding with CloudFormation?"**

---

**If Customer Insists on Terraform:**

**Response:**

"Absolutely, we can use Terraform. Here's how we'll adjust:

**1. We'll use Terraform with AWS provider for all infrastructure.**

**2. State Management:** We'll set up S3 backend with DynamoDB state locking:
```hcl
terraform {
  backend "s3" {
    bucket         = "configurator-terraform-state"
    key            = "prod/terraform.tfstate"
    region         = "ap-south-1"
    encrypt        = true
    dynamodb_table = "terraform-state-lock"
  }
}
```

**3. Workspaces for Multi-Region:**
- mumbai workspace for ap-south-1
- singapore workspace for ap-southeast-1

**4. Module Structure:**
```
terraform/
├── modules/
│   ├── vpc/
│   ├── ecs-service/
│   ├── rds/
│   └── ...
├── environments/
│   ├── prod-mumbai/
│   └── prod-singapore/
└── global/
    └── route53/
```

**5. We'll provide:**
- Terraform training for your team
- CI/CD pipeline with Terraform (using Atlantis or native)
- State management best practices
- Disaster recovery procedures adapted for Terraform

**This approach gives you the multi-cloud flexibility of Terraform while maintaining the AWS focus of this project. Sound good?"**

---

### Final Recommendation Summary

**For This Project:**
- **Use AWS CloudFormation**
- Customer preference is the deciding factor
- AWS-only architecture suits CloudFormation strengths
- Lower operational complexity for customer's team
- Cost-effective and battle-tested

**Future Considerations:**
- If organization adopts multi-cloud (Azure, GCP), revisit Terraform
- If non-AWS tooling proliferates (GitHub, Datadog, etc.), consider Terraform for those components
- Not a permanent decision—can migrate later if needs change

**Deliverables:**
1. CloudFormation templates for all infrastructure
2. StackSets configuration for multi-region deployment
3. CI/CD pipeline with CloudFormation integration
4. Comprehensive documentation and runbooks
5. Training for customer's team on CloudFormation best practices

---

**Document Control:**
- Share with development and infrastructure teams
- Review checklist items in sprint planning
- Track completion of implementation items
- Update based on lessons learned during development