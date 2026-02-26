# Service Design Checklist (AWS-Aligned)

## Rule of Use

**A service is not production-ready unless every applicable item below is explicitly answered Yes, N/A, or has a documented exception (ADR).**

This checklist ensures services are designed for production from day one, following AWS Well-Architected Framework principles.

---

## 1️⃣ Operational Excellence Checklist

**Goal**: Can this service be operated, debugged, and evolved safely?

- [ ] Service has a single, clear responsibility
- [ ] Ownership is clearly defined (team / repo / on-call)
- [ ] Service can be deployed independently
- [ ] No manual steps required to deploy or configure in prod
- [ ] Infrastructure is fully defined as code (IaC)
- [ ] `/health` (or equivalent) endpoint exists
- [ ] Health checks reflect real readiness, not just "process alive"
- [ ] Structured logging is implemented (JSON, not plain text)
- [ ] Logs explain why failures occur, not just stack traces
- [ ] Correlation / request IDs are generated or propagated
- [ ] Service can be restarted without data loss or manual intervention

❌ **Red flag**: "We'll debug it using logs after it fails"

### Implementation Guidance

**Health Checks**:
```csharp
app.MapHealthChecks("/health", new HealthCheckOptions
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
                description = e.Value.Description
            })
        });
        await context.Response.WriteAsync(result);
    }
});
```

**Structured Logging**:
```csharp
_logger.LogInformation(
    "User {UserId} created successfully with email {Email}",
    user.Id,
    user.Email.Value
);
```

**Correlation IDs**:
```csharp
public class CorrelationIdMiddleware
{
    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers["X-Correlation-ID"].FirstOrDefault()
            ?? Guid.NewGuid().ToString();
        
        context.Items["CorrelationId"] = correlationId;
        context.Response.Headers.Add("X-Correlation-ID", correlationId);
        
        using (_logger.BeginScope(new Dictionary<string, object>
        {
            ["CorrelationId"] = correlationId
        }))
        {
            await _next(context);
        }
    }
}
```

---

## 2️⃣ Security Checklist

**Goal**: Assume compromise; minimize blast radius.

- [ ] Service authenticates every inbound request
- [ ] Authorization is explicit (not assumed)
- [ ] No trust based on network location alone
- [ ] IAM role is service-specific, not shared
- [ ] IAM permissions follow least privilege
- [ ] No secrets stored in code, images, or environment files
- [ ] Secrets are sourced from Secrets Manager / Parameter Store
- [ ] Secrets can be rotated without redeploying code
- [ ] External inputs are validated and sanitized
- [ ] Error messages do not leak internal details
- [ ] Service is reachable only from intended callers (SG rules)

❌ **Red flag**: "It's internal, so we didn't secure it"

### Implementation Guidance

**JWT Authentication**:
```csharp
services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true
        };
    });
```

**Input Validation**:
```csharp
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(255);
            
        RuleFor(x => x.DisplayName)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(100)
            .Matches("^[a-zA-Z0-9 ]+$"); // Prevent injection
    }
}
```

**Secrets Management**:
```csharp
// AWS Secrets Manager
var secret = await _secretsManager.GetSecretValueAsync(new GetSecretValueRequest
{
    SecretId = "gss/admin-service/db-connection"
});

var connectionString = secret.SecretString;
```

**Error Handling** (Don't leak internals):
```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.StatusCode = 500;
        context.Response.ContentType = "application/json";
        
        var error = context.Features.Get<IExceptionHandlerFeature>();
        
        // Log full details internally
        _logger.LogError(error.Error, "Unhandled exception");
        
        // Return sanitized error to client
        await context.Response.WriteAsJsonAsync(new
        {
            error = "An internal error occurred",
            requestId = context.TraceIdentifier
        });
    });
});
```

---

## 3️⃣ Reliability Checklist

**Goal**: Fail predictably without cascading failures.

- [ ] Service is stateless (no session or local disk dependency)
- [ ] All outbound calls have timeouts
- [ ] Retries are bounded and controlled
- [ ] Circuit breakers or equivalent safeguards exist
- [ ] Service does not assume downstream availability
- [ ] No deep synchronous call chains (A → B → C)
- [ ] APIs are idempotent where retries are possible
- [ ] Failure modes are documented (what happens when X is down)
- [ ] Partial failures are handled gracefully
- [ ] Service can scale horizontally without coordination

❌ **Red flag**: "This service expects the other one to always be up"

### Implementation Guidance

**Timeouts and Retries** (using Polly):
```csharp
services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>()
    .AddPolicyHandler(Policy
        .TimeoutAsync<HttpResponseMessage>(TimeSpan.FromSeconds(10)))
    .AddPolicyHandler(Policy
        .Handle<HttpRequestException>()
        .WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt))));
```

**Circuit Breaker**:
```csharp
services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>()
    .AddPolicyHandler(Policy
        .Handle<HttpRequestException>()
        .CircuitBreakerAsync(
            handledEventsAllowedBeforeBreaking: 5,
            durationOfBreak: TimeSpan.FromSeconds(30),
            onBreak: (exception, duration) =>
            {
                _logger.LogWarning("Circuit breaker opened for {Duration}", duration);
            }));
```

**Graceful Degradation**:
```csharp
public async Task<BOM> GenerateBOMAsync(Guid configurationId)
{
    try
    {
        var pricing = await _catalogService.GetPricingAsync();
        return GenerateBOMWithPricing(configurationId, pricing);
    }
    catch (Exception ex)
    {
        _logger.LogWarning(ex, "Pricing service unavailable, generating BOM without pricing");
        return GenerateBOMWithoutPricing(configurationId);
    }
}
```

**Idempotency**:
```csharp
[HttpPost]
public async Task<IActionResult> CreateUser(
    [FromHeader(Name = "Idempotency-Key")] string idempotencyKey,
    [FromBody] CreateUserRequest request)
{
    // Check if already processed
    var existing = await _idempotencyStore.GetAsync(idempotencyKey);
    if (existing != null)
        return Ok(existing); // Return cached result
    
    var result = await _mediator.Send(new CreateUserCommand(request));
    
    await _idempotencyStore.StoreAsync(idempotencyKey, result);
    return CreatedAtAction(nameof(GetUser), new { id = result }, result);
}
```

---

## 4️⃣ Performance Efficiency Checklist

**Goal**: Use the right resources for the workload.

- [ ] Execution model matches workload (sync, async, event-driven)
- [ ] APIs are coarse-grained, not chatty
- [ ] Parallel calls are used where appropriate
- [ ] No unnecessary synchronous fan-out
- [ ] Payloads contain only what the consumer needs
- [ ] Latency is measured (P95/P99, not just average)
- [ ] Performance assumptions are documented
- [ ] No premature caching without measurement
- [ ] Resource limits (CPU/memory) are explicitly defined

❌ **Red flag**: "We'll optimize performance later"

### Implementation Guidance

**Parallel Calls**:
```csharp
public async Task<ConfigurationSummary> GetConfigurationSummaryAsync(Guid id)
{
    var configTask = _configService.GetAsync(id);
    var skusTask = _catalogService.GetSkusAsync(id);
    var rulesTask = _ruleService.GetActiveRulesAsync(id);
    
    await Task.WhenAll(configTask, skusTask, rulesTask);
    
    return new ConfigurationSummary
    {
        Configuration = configTask.Result,
        Skus = skusTask.Result,
        Rules = rulesTask.Result
    };
}
```

**Coarse-Grained APIs**:
```csharp
// Good: Single call with all needed data
[HttpGet("{id}/summary")]
public async Task<ConfigurationSummary> GetSummary(Guid id);

// Bad: Multiple chatty calls
[HttpGet("{id}")]
public async Task<Configuration> Get(Guid id);

[HttpGet("{id}/skus")]
public async Task<List<Sku>> GetSkus(Guid id);

[HttpGet("{id}/rules")]
public async Task<List<Rule>> GetRules(Guid id);
```

**Performance Metrics**:
```csharp
app.Use(async (context, next) =>
{
    var sw = Stopwatch.StartNew();
    await next();
    sw.Stop();
    
    _metrics.RecordLatency(
        context.Request.Path,
        sw.ElapsedMilliseconds,
        context.Response.StatusCode
    );
});
```

**Resource Limits** (Fargate task definition):
```json
{
  "cpu": "512",
  "memory": "1024",
  "containerDefinitions": [{
    "name": "admin-service",
    "cpu": 512,
    "memory": 1024,
    "memoryReservation": 512
  }]
}
```

---

## 5️⃣ Cost Optimization Checklist

**Goal**: Pay for value, not for assumptions.

- [ ] Service justifies being always-on (if applicable)
- [ ] Task size (CPU/memory) is right-sized
- [ ] Auto-scaling policies are defined and tested
- [ ] No unused resources provisioned "just in case"
- [ ] Managed AWS services preferred over self-managed
- [ ] Cost impact of service is understood
- [ ] Logs, metrics, and traces are right-sized (not excessive)
- [ ] Non-critical paths can scale down or pause
- [ ] No duplication of expensive computation across services

❌ **Red flag**: "Cost wasn't considered in this design"

### Implementation Guidance

**Auto-Scaling** (ECS):
```json
{
  "ServiceName": "admin-service",
  "ScalableTargetAction": {
    "MinCapacity": 2,
    "MaxCapacity": 10
  },
  "TargetTrackingScalingPolicyConfiguration": {
    "TargetValue": 70.0,
    "PredefinedMetricSpecification": {
      "PredefinedMetricType": "ECSServiceAverageCPUUtilization"
    }
  }
}
```

**Right-Sized Logging**:
```csharp
builder.Logging.AddFilter("Microsoft", LogLevel.Warning);
builder.Logging.AddFilter("System", LogLevel.Warning);
builder.Logging.AddFilter("GSS", LogLevel.Information);

// Don't log sensitive data
_logger.LogInformation("User created with ID {UserId}", user.Id);
// NOT: _logger.LogInformation("User created: {@User}", user);
```

---

## 6️⃣ Sustainability Checklist

**Goal**: Minimize waste and enable long-term evolution.

- [ ] Service avoids duplicate processing
- [ ] Async processing is used where real-time is not required
- [ ] Data movement is minimized
- [ ] No unnecessary cross-region or cross-zone calls
- [ ] Service boundaries are clean and understandable
- [ ] APIs are designed for evolution, not convenience
- [ ] Technical debt is documented when introduced
- [ ] Service design considers 2–3 year lifespan, not just MVP
- [ ] Unused features and code paths are removable
- [ ] Architecture avoids irreversible coupling

❌ **Red flag**: "We'll clean this up later"

### Implementation Guidance

**Async Processing**:
```csharp
// BOM generation doesn't need to be real-time
[HttpPost("generate")]
public async Task<IActionResult> GenerateBOM([FromBody] GenerateBOMRequest request)
{
    var jobId = await _queue.EnqueueAsync(new GenerateBOMJob
    {
        ConfigurationId = request.ConfigurationId
    });
    
    return Accepted(new { jobId, status = "processing" });
}

[HttpGet("jobs/{jobId}")]
public async Task<IActionResult> GetJobStatus(Guid jobId)
{
    var status = await _queue.GetStatusAsync(jobId);
    return Ok(status);
}
```

**Technical Debt Documentation** (ADR):
```markdown
# ADR-005: Temporary Synchronous BOM Generation

## Status
Accepted (with debt)

## Context
BOM generation currently blocks the request while calculating costs.

## Decision
Implement synchronously for MVP, migrate to async in Q2 2026.

## Consequences
- Simple implementation for MVP
- May cause timeouts for large configurations (>1000 items)
- **Technical Debt**: Tracked in JIRA-1234
```

---

## 7️⃣ API & Contract Checklist

**Goal**: Critical for BFF & Integration APIs

- [ ] API audience is clearly defined (UI, internal, partner)
- [ ] API intent is explicit (not CRUD leakage)
- [ ] Swagger/OpenAPI exists and is up to date
- [ ] Error contracts are documented
- [ ] Backward compatibility expectations are clear
- [ ] Versioning strategy is defined (or explicitly avoided for BFF)
- [ ] No internal topology or service names leaked
- [ ] Contracts are tested (consumer or contract tests)

❌ **Red flag**: "Frontend figured it out by trial and error"

### Implementation Guidance

**API Documentation**:
```csharp
/// <summary>
/// Creates a new user in the system
/// </summary>
/// <param name="request">User creation details</param>
/// <returns>Created user ID</returns>
/// <response code="201">User created successfully</response>
/// <response code="400">Invalid request data</response>
/// <response code="409">User with email already exists</response>
[HttpPost]
[ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
```

**Error Contracts**:
```csharp
public class ErrorResponse
{
    public string Error { get; set; }
    public string Message { get; set; }
    public string RequestId { get; set; }
    public Dictionary<string, string[]> ValidationErrors { get; set; }
}
```

**API Versioning**:
```csharp
services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

[ApiController]
[Route("api/v{version:apiVersion}/users")]
[ApiVersion("1.0")]
public class UsersV1Controller : ControllerBase
```

---

## 8️⃣ Testing & Quality Checklist

**Goal**: Confidence without test bloat.

- [ ] Unit tests cover orchestration and policy logic
- [ ] Contract tests exist for API consumers
- [ ] Integration tests validate wiring and runtime behavior
- [ ] Service supports E2E testing (but does not own it)
- [ ] Test data is deterministic and reproducible
- [ ] CI blocks merges on test failures
- [ ] Non-prod environments reflect prod topology (scaled-down)

❌ **Red flag**: "We'll add tests once it stabilizes"

### Implementation Guidance

**Contract Tests**:
```csharp
public class UserApiContractTests
{
    [Fact]
    public async Task CreateUser_ReturnsExpectedContract()
    {
        var response = await _client.PostAsJsonAsync("/api/v1/users", new
        {
            email = "test@example.com",
            displayName = "Test User"
        });
        
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var userId = await response.Content.ReadFromJsonAsync<Guid>();
        userId.Should().NotBeEmpty();
        
        response.Headers.Location.Should().NotBeNull();
    }
}
```

**CI Configuration** (.github/workflows/ci.yml):
```yaml
name: CI
on: [push, pull_request]
jobs:
  test:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v3
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
      - name: Run tests
        run: dotnet test --configuration Release
      - name: Block on failure
        if: failure()
        run: exit 1
```

---

## Service Readiness Matrix

Use this matrix to track production readiness:

| Service | Operational | Security | Reliability | Performance | Cost | Sustainability | API | Testing | Status |
|---------|-------------|----------|-------------|-------------|------|----------------|-----|---------|--------|
| admin-service | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | Production |
| catalog-service | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | ✅ | Production |
| rule-service | ✅ | ✅ | ✅ | ⚠️ | ✅ | ✅ | ✅ | ✅ | Production |
| file-service | 🚧 | 🚧 | 🚧 | 🚧 | 🚧 | 🚧 | 🚧 | 🚧 | Development |
| configuration-service | 🚧 | 🚧 | 🚧 | 🚧 | 🚧 | 🚧 | 🚧 | 🚧 | Development |
| bom-service | 🚧 | 🚧 | 🚧 | 🚧 | 🚧 | 🚧 | 🚧 | 🚧 | Development |

**Legend**:
- ✅ Complete and verified
- ⚠️ Partial or has documented exceptions
- 🚧 In development
- ❌ Not compliant

---

## Review Process

Before marking a service as production-ready:

1. **Self-Review**: Team completes this checklist
2. **Peer Review**: Another team reviews the checklist
3. **Architecture Review**: Architecture team validates design
4. **Security Review**: Security team validates security controls
5. **ADR Documentation**: Any N/A or exceptions documented in ADRs

---

## References

- [AWS Well-Architected Framework](https://aws.amazon.com/architecture/well-architected/)
- [12-Factor App](https://12factor.net/)
- [Microservices Patterns](https://microservices.io/patterns/)
- [CONTRIBUTING.md](../CONTRIBUTING.md) - Development guidelines
- [TESTING.md](TESTING.md) - Testing strategy
