# ADR-004: REST API Design Standards

## Status

Accepted

## Context

We need consistent REST API design across all microservices in the GSS Backend to ensure:
- Predictable developer experience
- Easy integration with frontend and external systems
- Clear API contracts
- Maintainability and evolvability
- Alignment with industry best practices

Without standardized API design, we risk:
- Inconsistent endpoint naming across services
- Unclear HTTP method usage
- Poor error handling
- Difficult API versioning
- Confusion for API consumers

## Decision

We will adopt the following REST API design standards for all GSS Backend services:

## Alternatives Considered

### Alternative 1: GraphQL

**Description**: GraphQL is a query language for APIs that allows clients to request exactly the data they need.

**Pros**:
- ✅ Clients can request exactly what they need (no over-fetching)
- ✅ Single endpoint for all queries
- ✅ Strong typing with schema
- ✅ Excellent for complex, nested data requirements
- ✅ Built-in introspection and documentation

**Cons**:
- ❌ **Complexity**: Significant learning curve for team
- ❌ **Caching**: HTTP caching doesn't work well (all POST requests)
- ❌ **Tooling**: Less mature .NET ecosystem compared to REST
- ❌ **Over-engineering**: Our use cases don't require flexible querying
- ❌ **Performance**: N+1 query problems require careful resolver design
- ❌ **Security**: Harder to implement rate limiting and query complexity limits
- ❌ **Monitoring**: More difficult to monitor and debug

**Why Rejected**:

Our services have **well-defined, stable data models** with predictable access patterns. The GSS platform has:
- Simple CRUD operations (users, SKUs, rules)
- Clear resource boundaries
- No need for clients to compose complex queries
- Frontend knows exactly what data it needs

GraphQL's flexibility would add unnecessary complexity without providing significant value. Our BFF (Backend for Frontend) layer already aggregates data from multiple services, eliminating GraphQL's main benefit.

**Use Case Analysis**:
```
Example: Get user with their configuration
- REST: Two endpoints, clear and cacheable
  GET /api/v1/users/{id}
  GET /api/v1/users/{id}/configuration
  
- GraphQL: Single query, but adds complexity
  query {
    user(id: "123") {
      id, email, displayName
      configuration { id, totalbomcost, status }
    }
  }
```

For our use cases, REST's simplicity outweighs GraphQL's flexibility.

---

### Alternative 2: gRPC

**Description**: gRPC is a high-performance RPC framework using Protocol Buffers.

**Pros**:
- ✅ High performance (binary protocol)
- ✅ Strong typing with .proto files
- ✅ Bi-directional streaming
- ✅ Code generation for multiple languages
- ✅ Excellent for service-to-service communication

**Cons**:
- ❌ **Browser Support**: Limited browser support (requires gRPC-Web)
- ❌ **Human Readability**: Binary format not human-readable
- ❌ **Debugging**: Harder to debug than JSON
- ❌ **Tooling**: No equivalent to Swagger UI for easy testing
- ❌ **Learning Curve**: Team unfamiliar with Protocol Buffers
- ❌ **HTTP/2 Required**: More complex infrastructure requirements

**Why Rejected**:

gRPC is excellent for **internal service-to-service** communication, but our primary consumers are:
1. **Web Frontend** (React) - needs browser-friendly APIs
2. **External Partners** - need easy-to-integrate APIs
3. **Testers** - need human-readable, testable APIs

REST with JSON provides:
- Easy browser integration
- Human-readable requests/responses
- Swagger UI for interactive testing
- Familiar patterns for external partners

**Future Consideration**: We may use gRPC for internal service-to-service communication where performance is critical (e.g., BOM generation calling multiple services), while keeping REST for external APIs.

---

### Alternative 3: SOAP

**Description**: SOAP is an XML-based protocol for exchanging structured information.

**Pros**:
- ✅ Mature, well-established
- ✅ Strong typing with WSDL
- ✅ Built-in error handling
- ✅ WS-* standards for security, transactions

**Cons**:
- ❌ **Legacy Technology**: Considered outdated
- ❌ **Verbose**: XML is verbose compared to JSON
- ❌ **Complexity**: Overly complex for modern web applications
- ❌ **Performance**: Slower than REST/JSON
- ❌ **Developer Experience**: Poor compared to modern APIs

**Why Rejected**:

SOAP is a legacy technology that doesn't align with modern development practices. Our team expects:
- Lightweight, JSON-based APIs
- Simple, resource-oriented design
- Easy integration with modern frameworks

SOAP would hinder developer productivity and make integration more difficult.

---

### Alternative 4: REST with JSON:API Specification

**Description**: JSON:API is a specification for building APIs in JSON with standardized structure.

**Pros**:
- ✅ Standardized response format
- ✅ Built-in pagination, filtering, sorting
- ✅ Relationship handling
- ✅ Reduces bikeshedding on API design

**Cons**:
- ❌ **Rigid Structure**: Less flexibility in response format
- ❌ **Verbosity**: More verbose than simple REST
- ❌ **Learning Curve**: Team needs to learn specification
- ❌ **Over-engineering**: Too complex for our simple use cases

**Why Rejected**:

JSON:API is excellent for complex, relationship-heavy APIs, but our services are relatively simple. The specification's rigid structure would add unnecessary complexity without significant benefit.

We prefer **pragmatic REST** with our own conventions, documented in this ADR.

---

## Why REST?

After evaluating alternatives, we chose **REST with JSON** because:

### 1. **Simplicity**
- Easy to understand and implement
- Familiar to all team members
- Low learning curve for new developers

### 2. **Tooling**
- Excellent .NET support (ASP.NET Core)
- Swagger/OpenAPI for documentation
- Easy testing with Postman, curl, browser

### 3. **Caching**
- HTTP caching works out of the box
- CDN support for GET requests
- Browser caching for static resources

### 4. **Flexibility**
- Can evolve incrementally
- Easy to version (URL path versioning)
- No vendor lock-in

### 5. **Integration**
- Browser-friendly (JavaScript fetch)
- Easy for external partners
- Human-readable for debugging

### 6. **Performance**
- Good enough for our use cases
- Can optimize with caching, compression
- BFF layer handles aggregation

### 7. **Monitoring**
- Standard HTTP status codes
- Easy to monitor with APM tools
- Clear request/response patterns

---

### When to Reconsider

We should reconsider this decision if:

- ❓ Frontend needs highly flexible data querying → Consider GraphQL
- ❓ Service-to-service calls become performance bottleneck → Consider gRPC internally
- ❓ Clients need real-time bidirectional communication → Consider WebSockets or gRPC streaming
- ❓ Mobile apps need bandwidth optimization → Consider GraphQL or gRPC

For now, **REST meets our needs** and provides the best balance of simplicity, performance, and developer experience.

---

### 1. Resource-Oriented URLs

**Use nouns, not verbs** for resource names:

```
✅ Good:
GET    /api/v1/users
POST   /api/v1/users
GET    /api/v1/users/{id}
PUT    /api/v1/users/{id}
DELETE /api/v1/users/{id}

❌ Bad:
POST   /api/v1/createUser
GET    /api/v1/getUser/{id}
POST   /api/v1/deleteUser/{id}
```

**Use plural nouns** for collections:
- `/users` not `/user`
- `/products` not `/product`

**Use hierarchical relationships** for nested resources:
```
GET /api/v1/users/{userId}/orders
GET /api/v1/configurations/{configId}/snapshots
```

### 2. HTTP Methods (Verbs)

Use HTTP methods according to their semantic meaning:

| Method | Purpose | Idempotent | Safe |
|--------|---------|------------|------|
| GET | Retrieve resource(s) | Yes | Yes |
| POST | Create new resource | No | No |
| PUT | Update/Replace entire resource | Yes | No |
| PATCH | Partial update | No | No |
| DELETE | Remove resource | Yes | No |

**Examples**:
```csharp
// GET - Retrieve
[HttpGet]
public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()

[HttpGet("{id}")]
public async Task<ActionResult<UserDto>> GetUser(Guid id)

// POST - Create
[HttpPost]
public async Task<ActionResult<Guid>> CreateUser([FromBody] CreateUserRequest request)

// PUT - Full update
[HttpPut("{id}")]
public async Task<IActionResult> UpdateUser(Guid id, [FromBody] UpdateUserRequest request)

// DELETE - Remove
[HttpDelete("{id}")]
public async Task<IActionResult> DeleteUser(Guid id)
```

### 3. HTTP Status Codes

Use appropriate status codes:

**Success (2xx)**:
- `200 OK` - Successful GET, PUT, PATCH, DELETE
- `201 Created` - Successful POST (resource created)
- `202 Accepted` - Request accepted for async processing
- `204 No Content` - Successful DELETE or PUT with no response body

**Client Errors (4xx)**:
- `400 Bad Request` - Invalid request data (validation errors)
- `401 Unauthorized` - Authentication required
- `403 Forbidden` - Authenticated but not authorized
- `404 Not Found` - Resource doesn't exist
- `409 Conflict` - Resource conflict (e.g., duplicate email)
- `422 Unprocessable Entity` - Semantic validation errors

**Server Errors (5xx)**:
- `500 Internal Server Error` - Unexpected server error
- `503 Service Unavailable` - Service temporarily unavailable

**Implementation**:
```csharp
[HttpPost]
[ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
{
    var userId = await _mediator.Send(new CreateUserCommand(request));
    return CreatedAtAction(nameof(GetUser), new { id = userId }, userId);
}
```

### 4. API Versioning

Use **URL path versioning** for clarity and simplicity:

```
/api/v1/users
/api/v2/users
```

**Rationale**:
- Clear and explicit
- Easy to route in API Gateway
- Simple for clients to understand
- Supports multiple versions simultaneously

**Implementation**:
```csharp
[ApiController]
[Route("api/v{version:apiVersion}/users")]
[ApiVersion("1.0")]
public class UsersV1Controller : ControllerBase
{
    // v1 endpoints
}

[ApiController]
[Route("api/v{version:apiVersion}/users")]
[ApiVersion("2.0")]
public class UsersV2Controller : ControllerBase
{
    // v2 endpoints with breaking changes
}
```

**Version Strategy**:
- Increment major version (v1 → v2) for breaking changes
- Maintain backward compatibility within a version
- Deprecate old versions with 6-month notice
- Support maximum 2 versions simultaneously

### 5. Request/Response Format

**Use JSON** for all request and response bodies:

```json
// Request
{
  "email": "user@example.com",
  "displayName": "John Doe",
  "role": "DESIGNER"
}

// Response (Success)
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "displayName": "John Doe",
  "role": "DESIGNER",
  "status": "PENDING",
  "createdAt": "2026-01-08T00:00:00Z"
}
```

**Naming Conventions**:
- Use `camelCase` for JSON properties
- Use descriptive names
- Avoid abbreviations

### 6. Error Responses

Use **RFC 7807 Problem Details** for error responses:

```json
{
  "type": "https://api.gss.com/errors/validation-error",
  "title": "Validation Failed",
  "status": 400,
  "detail": "One or more validation errors occurred",
  "instance": "/api/v1/users",
  "errors": {
    "email": ["Email is required", "Invalid email format"],
    "displayName": ["Display name must be between 2 and 100 characters"]
  },
  "traceId": "00-4bf92f3577b34da6a3ce929d0e0e4736-00"
}
```

**Implementation**:
```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;
        
        var problemDetails = exception switch
        {
            ValidationException validationEx => new ValidationProblemDetails
            {
                Type = "https://api.gss.com/errors/validation-error",
                Title = "Validation Failed",
                Status = StatusCodes.Status400BadRequest,
                Detail = validationEx.Message,
                Instance = context.Request.Path,
                Errors = validationEx.Errors
            },
            UserNotFoundException notFoundEx => new ProblemDetails
            {
                Type = "https://api.gss.com/errors/not-found",
                Title = "Resource Not Found",
                Status = StatusCodes.Status404NotFound,
                Detail = notFoundEx.Message,
                Instance = context.Request.Path
            },
            _ => new ProblemDetails
            {
                Type = "https://api.gss.com/errors/internal-error",
                Title = "Internal Server Error",
                Status = StatusCodes.Status500InternalServerError,
                Detail = "An unexpected error occurred",
                Instance = context.Request.Path
            }
        };
        
        problemDetails.Extensions["traceId"] = context.TraceIdentifier;
        
        context.Response.StatusCode = problemDetails.Status ?? 500;
        await context.Response.WriteAsJsonAsync(problemDetails);
    });
});
```

### 7. Pagination

Use **query parameters** for pagination:

```
GET /api/v1/users?page=1&pageSize=20
GET /api/v1/users?offset=0&limit=20
```

**Response format**:
```json
{
  "items": [...],
  "totalCount": 150,
  "page": 1,
  "pageSize": 20,
  "totalPages": 8,
  "hasNextPage": true,
  "hasPreviousPage": false
}
```

**Implementation**:
```csharp
[HttpGet]
public async Task<ActionResult<PagedResult<UserDto>>> GetUsers(
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
{
    var result = await _mediator.Send(new ListUsersQuery(page, pageSize));
    return Ok(result);
}
```

### 8. Filtering, Sorting, Searching

Use **query parameters**:

```
// Filtering
GET /api/v1/users?status=ACTIVE&role=DESIGNER

// Sorting
GET /api/v1/users?sortBy=createdAt&sortOrder=desc

// Searching
GET /api/v1/users?search=john

// Combined
GET /api/v1/users?status=ACTIVE&sortBy=email&search=john&page=1&pageSize=20
```

### 9. Idempotency

For **POST requests** that should be idempotent, use `Idempotency-Key` header:

```http
POST /api/v1/users
Idempotency-Key: a7f8d9e0-1234-5678-90ab-cdef12345678
Content-Type: application/json

{
  "email": "user@example.com",
  "displayName": "John Doe"
}
```

**Implementation**:
```csharp
[HttpPost]
public async Task<IActionResult> CreateUser(
    [FromHeader(Name = "Idempotency-Key")] string? idempotencyKey,
    [FromBody] CreateUserRequest request)
{
    if (!string.IsNullOrEmpty(idempotencyKey))
    {
        var cached = await _idempotencyStore.GetAsync(idempotencyKey);
        if (cached != null)
            return Ok(cached); // Return cached result
    }
    
    var userId = await _mediator.Send(new CreateUserCommand(request));
    
    if (!string.IsNullOrEmpty(idempotencyKey))
        await _idempotencyStore.StoreAsync(idempotencyKey, userId, TimeSpan.FromHours(24));
    
    return CreatedAtAction(nameof(GetUser), new { id = userId }, userId);
}
```

### 10. HATEOAS (Optional)

For complex workflows, include hypermedia links:

```json
{
  "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "email": "user@example.com",
  "status": "PENDING",
  "_links": {
    "self": { "href": "/api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6" },
    "activate": { "href": "/api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6/activate", "method": "POST" },
    "update": { "href": "/api/v1/users/3fa85f64-5717-4562-b3fc-2c963f66afa6", "method": "PUT" }
  }
}
```

### 11. Documentation

**All endpoints must have**:
- XML documentation comments
- Swagger/OpenAPI annotations
- Example requests/responses
- Error response documentation

```csharp
/// <summary>
/// Creates a new user in the system
/// </summary>
/// <param name="request">User creation details</param>
/// <returns>The ID of the created user</returns>
/// <remarks>
/// Sample request:
/// 
///     POST /api/v1/users
///     {
///        "email": "user@example.com",
///        "displayName": "John Doe",
///        "role": "DESIGNER"
///     }
/// 
/// </remarks>
/// <response code="201">User created successfully</response>
/// <response code="400">Invalid request data</response>
/// <response code="409">User with email already exists</response>
[HttpPost]
[ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
```

## Consequences

### Positive

✅ **Consistency**: All services follow the same patterns  
✅ **Predictability**: Developers know what to expect  
✅ **Discoverability**: Clear, self-documenting APIs  
✅ **Maintainability**: Easy to understand and modify  
✅ **Integration**: Simplified frontend and external integration  
✅ **Documentation**: Swagger UI provides interactive docs  
✅ **Versioning**: Clear upgrade path for breaking changes  
✅ **Error Handling**: Standardized error responses  

### Negative

⚠️ **Learning Curve**: Team must learn and follow standards  
⚠️ **Refactoring**: Existing APIs may need updates  
⚠️ **Overhead**: More upfront design work  

### Mitigation

- Provide code examples and templates
- Include in code review checklist
- Use linters and analyzers where possible
- Document in [coding-standards.md](../coding-standards.md)

## Compliance Checklist

Before deploying an API endpoint:

- [ ] Uses resource-oriented URLs (nouns, not verbs)
- [ ] Uses appropriate HTTP methods
- [ ] Returns correct HTTP status codes
- [ ] Includes API versioning (v1, v2, etc.)
- [ ] Uses JSON for request/response
- [ ] Implements RFC 7807 Problem Details for errors
- [ ] Supports pagination for collections
- [ ] Has XML documentation comments
- [ ] Has Swagger/OpenAPI annotations
- [ ] Includes example requests/responses
- [ ] Tested via Swagger UI
- [ ] Documented in service README

## References

- [Microsoft REST API Guidelines](https://github.com/microsoft/api-guidelines/blob/vNext/Guidelines.md)
- [RFC 7807 - Problem Details for HTTP APIs](https://tools.ietf.org/html/rfc7807)
- [REST API Tutorial](https://restfulapi.net/)
- [OpenAPI Specification](https://swagger.io/specification/)
- [Coding Standards](../coding-standards.md)
- [Service Design Checklist](../service-design-checklist.md)

## Related ADRs

- ADR-001: Use PostgreSQL for All Services
- ADR-002: Adopt DDD Architecture
- ADR-003: CQRS with MediatR

---

**Decision Date**: January 8, 2026  
**Decided By**: GSS Architecture Team  
**Supersedes**: None  
**Superseded By**: None
