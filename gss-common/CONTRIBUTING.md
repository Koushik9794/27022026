# Contributing to GSS Common

## What Belongs in gss-common?

This library is intentionally **minimal** and contains only cross-cutting concerns that are:
1. **Truly shared** across multiple services
2. **Stable** and unlikely to change frequently
3. **Interface-driven** (abstractions, not implementations)
4. **Lightweight** (no heavy dependencies)

## ✅ What CAN Be Added

### 1. **Authentication & Authorization Abstractions**
```csharp
// ✅ Good - Interface for user context
public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Email { get; }
}

// ✅ Good - Permission checking abstraction
public interface IPermissionChecker
{
    Task<bool> HasPermissionAsync(string permission);
}
```

**Criteria:**
- Every service needs authentication
- Interface only, no implementation
- No framework dependencies

### 2. **Common Value Objects & Patterns**
```csharp
// ✅ Good - Result pattern for error handling
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public Error Error { get; }
}

// ✅ Good - Money value object
public record Money(decimal Amount, string Currency);

// ✅ Good - Email value object
public record Email(string Value);
```

**Criteria:**
- Immutable value objects
- Used across multiple domains
- No business logic specific to one service

### 3. **Domain Event Contracts**
```csharp
// ✅ Good - Base domain event interface
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}

// ✅ Good - Event dispatcher abstraction
public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> events);
}
```

**Criteria:**
- Enables event-driven architecture
- Interface only
- No specific event implementations

### 4. **Common Interfaces for Infrastructure Concerns**
```csharp
// ✅ Good - Export abstraction
public interface IExcelExporter
{
    Task<ExportResult> ExportAsync<T>(IEnumerable<T> data);
}

// ✅ Good - Email sender abstraction
public interface IEmailSender
{
    Task SendAsync(EmailMessage message);
}

// ✅ Good - File storage abstraction
public interface IFileStorage
{
    Task<string> UploadAsync(Stream stream, string fileName);
}
```

**Criteria:**
- Common infrastructure needs
- Interface only, services provide implementations
- No cloud provider-specific code

### 5. **Common DTOs for Cross-Service Communication**
```csharp
// ✅ Good - Pagination request
public record PagingRequest(int Page, int PageSize);

// ✅ Good - Pagination response
public record PagedResult<T>(List<T> Items, int TotalCount, int Page, int PageSize);

// ✅ Good - API error response
public record ErrorResponse(string Code, string Message, Dictionary<string, string[]>? Errors);
```

**Criteria:**
- Used in API contracts across services
- Immutable records
- No service-specific logic

### 6. **Common Enums & Constants**
```csharp
// ✅ Good - Common status enum
public enum EntityStatus
{
    Active,
    Inactive,
    Deleted
}

// ✅ Good - Common error codes
public static class ErrorCodes
{
    public const string NotFound = "Error.NotFound";
    public const string Validation = "Error.Validation";
    public const string Conflict = "Error.Conflict";
}
```

**Criteria:**
- Shared across multiple services
- Stable and unlikely to change
- No business logic

## ❌ What CANNOT Be Added

### 1. **Business Logic**
```csharp
// ❌ Bad - Business logic belongs in domain layer
public class OrderCalculator
{
    public decimal CalculateTotal(Order order) { ... }
}
```

**Why not:**
- Business logic belongs in service domain layers
- Violates single responsibility
- Creates tight coupling

### 2. **Infrastructure Implementations**
```csharp
// ❌ Bad - Implementation belongs in service infrastructure
public class S3FileStorage : IFileStorage
{
    public async Task<string> UploadAsync(Stream stream, string fileName)
    {
        // AWS S3 specific code
    }
}
```

**Why not:**
- Services should implement their own infrastructure
- Creates dependency on specific cloud providers
- Violates independence principle

### 3. **Framework-Specific Code**
```csharp
// ❌ Bad - Use standard packages instead
public class CustomMediatRBehavior : IPipelineBehavior<TRequest, TResponse>
{
    // Custom MediatR behavior
}
```

**Why not:**
- Use standard NuGet packages (MediatR, FluentValidation, etc.)
- Creates unnecessary abstraction
- Harder to maintain

### 4. **Service-Specific Models**
```csharp
// ❌ Bad - Service-specific domain model
public class Sku
{
    public string Code { get; set; }
    public string Name { get; set; }
}
```

**Why not:**
- Domain models belong in service domain layers
- Each service should own its models
- Violates bounded context

### 5. **Heavy Dependencies**
```csharp
// ❌ Bad - Heavy external dependencies
// Don't add packages like:
// - Entity Framework Core
// - Dapper
// - Serilog
// - AWS SDK
```

**Why not:**
- Keep gss-common lightweight
- Services choose their own infrastructure
- Avoids version conflicts

### 6. **Database Schemas or Migrations**
```csharp
// ❌ Bad - Database-specific code
public class DatabaseSchema
{
    public static string CreateUsersTable = "CREATE TABLE users...";
}
```

**Why not:**
- Each service owns its database
- Violates database-per-service pattern
- Creates tight coupling

## Decision Flowchart

```
Is it needed by multiple services?
├─ No → Don't add to gss-common
└─ Yes
    └─ Is it an interface/abstraction?
        ├─ No → Don't add to gss-common
        └─ Yes
            └─ Does it have heavy dependencies?
                ├─ Yes → Don't add to gss-common
                └─ No
                    └─ Is it stable and unlikely to change?
                        ├─ No → Don't add to gss-common
                        └─ Yes → ✅ ADD TO gss-common
```

## Examples of Good Additions

### Example 1: Adding Audit Fields Interface
```csharp
// ✅ Good - Common audit pattern
namespace GssCommon.Common;

public interface IAuditable
{
    DateTime CreatedAt { get; }
    string? CreatedBy { get; }
    DateTime? UpdatedAt { get; }
    string? UpdatedBy { get; }
}
```

**Why it's good:**
- Used across all services
- Interface only
- No dependencies
- Stable pattern

### Example 2: Adding Soft Delete Interface
```csharp
// ✅ Good - Common soft delete pattern
namespace GssCommon.Common;

public interface ISoftDeletable
{
    bool IsDeleted { get; }
    DateTime? DeletedAt { get; }
    string? DeletedBy { get; }
}
```

**Why it's good:**
- Common pattern across services
- Interface only
- No business logic

### Example 3: Adding Multi-Tenancy Support
```csharp
// ✅ Good - Multi-tenant abstraction
namespace GssCommon.Auth;

public interface ITenantContext
{
    string? TenantId { get; }
    string? TenantName { get; }
}
```

**Why it's good:**
- Cross-cutting concern
- Interface only
- Enables multi-tenancy across services

## Review Process

Before adding anything to gss-common:

1. **Ask yourself:**
   - Is this truly needed by 2+ services?
   - Can I make it an interface instead of implementation?
   - Does it have zero/minimal dependencies?
   - Will it remain stable?

2. **Get team approval:**
   - Discuss in architecture review
   - Get consensus from service teams
   - Document the decision

3. **Follow the pattern:**
   - Add to appropriate namespace
   - Write XML documentation
   - Add usage example to README
   - Update version number

## When in Doubt

**Default to NOT adding it to gss-common.**

It's better to:
- Copy code to individual services
- Use standard NuGet packages
- Keep services independent

Only add to gss-common when there's a **clear, compelling reason** that it's truly cross-cutting and stable.

## Questions?

If you're unsure whether something belongs in gss-common, ask:
- "Would removing this break multiple services?"
- "Is this an interface or an implementation?"
- "Will this change frequently?"

If the answer to the first question is "No", or the second is "Implementation", or the third is "Yes", then **don't add it**.
