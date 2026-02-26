# Contributing to GSS Web API (BFF)

Thank you for contributing to the GSS Web API! This document provides guidelines for developing the Backend-for-Frontend service.

## Table of Contents

- [BFF Architecture Principles](#bff-architecture-principles)
- [Development Setup](#development-setup)
- [Code Standards](#code-standards)
- [API Design Guidelines](#api-design-guidelines)
- [Testing Requirements](#testing-requirements)
- [Pull Request Process](#pull-request-process)
- [Architecture Decision Records](#architecture-decision-records)

---

## BFF Architecture Principles

### What is a BFF?

A **Backend-for-Frontend (BFF)** is a service layer that sits between the frontend and internal microservices. It is designed specifically for the needs of a particular frontend application.

### Core Principles

1. **Intent-Based APIs**
   - Design endpoints around **user journeys** and **frontend screens**, not internal microservice structures
   - Example: `POST /configurations` (intent) vs `POST /catalog/skus` (CRUD)
   - Each endpoint should represent a meaningful user action

2. **Orchestration, Not Pass-Through**
   - BFF should **aggregate** data from multiple microservices
   - BFF should **transform** data to match frontend needs
   - Avoid simple proxy endpoints that just forward requests

3. **Stateless Design**
   - No session state stored in BFF
   - All state managed by downstream services or client
   - Enables horizontal scaling on AWS ECS Fargate

4. **Resilience First**
   - Implement circuit breakers for all downstream calls
   - Use retries with exponential backoff
   - Set appropriate timeouts
   - Fail gracefully with meaningful error messages

5. **Security Gateway**
   - Validate JWT tokens at BFF ingress
   - Resolve user context (permissions, dealer info)
   - Never expose internal service URLs or secrets to frontend
   - Propagate correlation IDs for tracing

### What NOT to Do

❌ **Don't create CRUD wrappers** - BFF is not a simple proxy  
❌ **Don't store state** - BFF must be stateless  
❌ **Don't expose internal models** - Transform to frontend-specific DTOs  
❌ **Don't skip resilience patterns** - Always use Polly policies  
❌ **Don't hardcode URLs** - Use configuration for service endpoints  

---

## Development Setup

### Prerequisites

- .NET 10 SDK
- Docker Desktop
- VS Code with C# extension
- Git

### Initial Setup

1. **Clone and navigate to BFF**
   ```bash
   git clone <repository-url>
   cd gss-backend/gss-web-api
   ```

2. **Install dependencies**
   ```bash
   dotnet restore
   ```

3. **Start dependent services**
   ```bash
   cd ..
   docker-compose up -d admin-service catalog-service rule-service
   ```

4. **Run BFF locally**
   ```bash
   cd gss-web-api
   dotnet run
   ```

5. **Verify health**
   ```bash
   curl http://localhost:5003/health
   ```

### Development Workflow

1. Create a feature branch: `git checkout -b feature/add-configuration-history`
2. Make changes following code standards (see below)
3. Write tests (unit, contract, integration)
4. Run tests: `dotnet test`
5. Update OpenAPI spec if adding/modifying endpoints
6. Commit with descriptive message
7. Push and create pull request

---

## Code Standards

### Naming Conventions

- **Controllers**: `{Intent}Controller.cs` (e.g., `ConfigurationController.cs`)
- **DTOs**: `{Entity}{Type}.cs` (e.g., `CreateConfigurationRequest.cs`)
- **Handlers**: `{Command/Query}Handler.cs` (e.g., `CreateConfigurationHandler.cs`)
- **Clients**: `I{Service}Client.cs` (e.g., `IAdminServiceClient.cs`)

### File Organization

```
src/
├── api/controllers/          # Thin controllers, delegate to handlers
├── orchestration/handlers/   # Business logic, orchestration
├── clients/                  # Typed HTTP clients for microservices
├── dto/                      # Request/response models
├── middleware/               # Custom middleware
└── infrastructure/           # Cross-cutting concerns
```

### Controller Guidelines

**✅ Good Controller (Thin, Intent-Based)**

```csharp
/// <summary>
/// Create new warehouse configuration
/// </summary>
/// <remarks>
/// User Journey: 02_Create_Configuration_From_Enquiry
/// 
/// Orchestrates:
/// 1. Fetch enquiry details from admin-service
/// 2. Validate site constraints via rule-service
/// 3. Create configuration in catalog-service
/// 4. Return aggregated response
/// </remarks>
[HttpPost]
[ProducesResponseType(typeof(ConfigurationResponse), 201)]
public async Task<ActionResult<ConfigurationResponse>> CreateConfiguration(
    [FromBody] CreateConfigurationRequest request)
{
    var command = new CreateConfigurationCommand(request);
    var result = await _mediator.Send(command);
    return CreatedAtAction(nameof(GetConfiguration), new { id = result.ConfigurationId }, result);
}
```

**❌ Bad Controller (CRUD Pass-Through)**

```csharp
// Don't do this - just proxying to catalog-service
[HttpPost("skus")]
public async Task<IActionResult> CreateSku([FromBody] SkuDto sku)
{
    var response = await _catalogClient.CreateSku(sku);
    return Ok(response);
}
```

### DTO Guidelines

- Use `record` types for immutability
- Add XML documentation for Swagger
- Use descriptive property names
- Include validation attributes

```csharp
/// <summary>
/// Request to create a new configuration
/// </summary>
public record CreateConfigurationRequest
{
    /// <summary>
    /// Optional enquiry ID to link this configuration to
    /// </summary>
    public Guid? EnquiryId { get; init; }
    
    /// <summary>
    /// Configuration name
    /// </summary>
    [Required]
    [StringLength(200)]
    public string Name { get; init; } = string.Empty;
    
    /// <summary>
    /// Site details including building footprint
    /// </summary>
    [Required]
    public SiteDetails SiteDetails { get; init; } = new();
}
```

### Handler Guidelines

Handlers contain orchestration logic:

```csharp
public class CreateConfigurationHandler : ICommandHandler<CreateConfigurationCommand, ConfigurationResponse>
{
    private readonly IAdminServiceClient _adminClient;
    private readonly ICatalogServiceClient _catalogClient;
    private readonly IRuleServiceClient _ruleClient;
    private readonly ILogger<CreateConfigurationHandler> _logger;

    public async Task<ConfigurationResponse> Handle(CreateConfigurationCommand command, CancellationToken ct)
    {
        // 1. Fetch enquiry if provided
        EnquiryDto? enquiry = null;
        if (command.Request.EnquiryId.HasValue)
        {
            enquiry = await _adminClient.GetEnquiryAsync(command.Request.EnquiryId.Value, ct);
        }

        // 2. Validate site constraints
        var validationRequest = MapToValidationRequest(command.Request, enquiry);
        var validationResult = await _ruleClient.ValidateSiteAsync(validationRequest, ct);
        
        if (!validationResult.IsValid)
        {
            throw new ValidationException(validationResult.Errors);
        }

        // 3. Create configuration
        var createRequest = MapToCreateRequest(command.Request, enquiry);
        var configuration = await _catalogClient.CreateConfigurationAsync(createRequest, ct);

        // 4. Return aggregated response
        return MapToResponse(configuration, validationResult);
    }
}
```

### Resilience Patterns

Always wrap downstream calls with Polly policies:

```csharp
// In service registration (Program.cs)
builder.Services.AddHttpClient<IAdminServiceClient, AdminServiceClient>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy())
    .AddPolicyHandler(GetTimeoutPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(3, retryAttempt => 
            TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}

static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));
}
```

---

## API Design Guidelines

### 1. Map to User Journeys

Every endpoint must map to a user journey documented in `docs/User_Journeys/`.

**Example:**
- User Journey: `02_Create_Configuration_From_Enquiry`
- Endpoint: `POST /api/v1/configurations`

### 2. Use HTTP Methods Correctly

- `POST` - Create resources or trigger actions
- `GET` - Retrieve resources
- `PUT` - Full replacement update
- `PATCH` - Partial update
- `DELETE` - Remove resources

### 3. Versioning

All endpoints must include version in URL: `/api/v1/...`

### 4. Response Codes

Use appropriate HTTP status codes:
- `200 OK` - Successful GET/PUT/PATCH
- `201 Created` - Successful POST (include `Location` header)
- `202 Accepted` - Async operation started
- `400 Bad Request` - Validation error
- `401 Unauthorized` - Invalid/expired token
- `403 Forbidden` - Insufficient permissions
- `404 Not Found` - Resource not found
- `409 Conflict` - Concurrent modification
- `500 Internal Server Error` - Unexpected error

### 5. Error Response Format

All errors must follow standard format:

```json
{
  "correlationId": "abc-123-def",
  "errorCode": "VALIDATION_ERROR",
  "message": "Configuration validation failed",
  "details": [
    {
      "field": "siteDetails.buildingFootprint.length",
      "message": "Length must be greater than 0"
    }
  ],
  "timestamp": "2026-01-08T04:00:00Z",
  "path": "/api/v1/configurations"
}
```

### 6. HATEOAS Links

Include `_links` in responses for discoverability:

```json
{
  "configurationId": "...",
  "_links": {
    "self": "/api/v1/configurations/...",
    "validate": "/api/v1/configurations/.../validate",
    "generateBom": "/api/v1/jobs/generate-bom"
  }
}
```

### 7. Pagination

For list endpoints, use query parameters:
- `page` - Page number (1-indexed)
- `pageSize` - Items per page (default: 20, max: 100)
- `sort` - Sort field and direction (e.g., `createdAt:desc`)

---

## Testing Requirements

### Test Coverage Targets

- **Unit Tests**: 80%+ coverage
- **Contract Tests**: 100% of API endpoints
- **Integration Tests**: All orchestration flows
- **E2E Tests**: Critical user journeys

### Unit Tests

Test handlers in isolation with mocked dependencies:

```csharp
[Fact]
public async Task Handle_ValidRequest_CreatesConfiguration()
{
    // Arrange
    var mockAdminClient = new Mock<IAdminServiceClient>();
    var mockCatalogClient = new Mock<ICatalogServiceClient>();
    var handler = new CreateConfigurationHandler(mockAdminClient.Object, mockCatalogClient.Object);
    
    var command = new CreateConfigurationCommand(new CreateConfigurationRequest { ... });

    // Act
    var result = await handler.Handle(command, CancellationToken.None);

    // Assert
    result.Should().NotBeNull();
    result.ConfigurationId.Should().NotBeEmpty();
    mockCatalogClient.Verify(x => x.CreateConfigurationAsync(It.IsAny<CreateConfigRequest>(), It.IsAny<CancellationToken>()), Times.Once);
}
```

### Contract Tests

Validate API responses against OpenAPI spec:

```csharp
[Fact]
public async Task POST_Configurations_ReturnsValidSchema()
{
    // Arrange
    var request = new CreateConfigurationRequest { ... };
    
    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/configurations", request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    var content = await response.Content.ReadAsStringAsync();
    var schema = await LoadOpenApiSchema("ConfigurationResponse");
    schema.Validate(content).Should().BeTrue();
}
```

### Integration Tests

Test with WireMock for microservices:

```csharp
[Fact]
public async Task CreateConfiguration_WithEnquiry_OrchestatesCorrectly()
{
    // Arrange - Setup WireMock stubs
    _adminServiceMock.Given(Request.Create().WithPath("/enquiries/*"))
        .RespondWith(Response.Create().WithStatusCode(200).WithBodyAsJson(enquiryDto));
    
    // Act
    var response = await _client.PostAsJsonAsync("/api/v1/configurations", request);
    
    // Assert
    response.StatusCode.Should().Be(HttpStatusCode.Created);
    _adminServiceMock.LogEntries.Should().ContainSingle(x => x.RequestMessage.Path.Contains("/enquiries"));
}
```

---

## Pull Request Process

### Before Submitting

1. ✅ All tests pass locally
2. ✅ Code follows standards
3. ✅ OpenAPI spec updated (if API changed)
4. ✅ XML documentation added to new endpoints
5. ✅ User journey mapping documented
6. ✅ No hardcoded values or secrets

### PR Description Template

```markdown
## Description
Brief description of changes

## User Journey
Which user journey does this relate to? (e.g., 02_Create_Configuration_From_Enquiry)

## Changes
- Added `POST /configurations/{id}/history` endpoint
- Implemented `GetConfigurationHistoryHandler`
- Added integration tests

## Testing
- [ ] Unit tests added/updated
- [ ] Contract tests added/updated
- [ ] Integration tests added/updated
- [ ] Manually tested in Swagger UI

## Breaking Changes
None / List any breaking changes

## Checklist
- [ ] OpenAPI spec updated
- [ ] XML documentation added
- [ ] Tests passing
- [ ] No secrets committed
```

### Code Review Criteria

Reviewers will check:
- Intent-based API design
- Proper orchestration (not pass-through)
- Resilience patterns applied
- Error handling
- Test coverage
- Documentation quality

---

## Architecture Decision Records

For significant architectural decisions, create an ADR in `docs/adr/`:

**Template: `docs/adr/001-use-wolverine-for-cqrs.md`**

```markdown
# ADR 001: Use Wolverine for CQRS

## Status
Accepted

## Context
Need to implement CQRS pattern for BFF orchestration handlers.

## Decision
Use WolverineFx instead of MediatR for consistency with other services.

## Consequences
- Consistent with admin-service and catalog-service
- Better performance than MediatR
- Requires learning Wolverine conventions
```

---

## Questions?

For questions or clarifications:
- **Slack**: #gss-backend-dev
- **Email**: backend-team@gss.com
- **Wiki**: [Internal BFF Development Guide](https://wiki.gss.com/bff)

---

**Thank you for contributing to GSS Web API!** 🚀
