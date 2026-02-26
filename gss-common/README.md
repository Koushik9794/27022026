# GSS Common Library

Lightweight shared library containing cross-cutting concerns for GSS microservices.

## Philosophy

This library follows the **minimal shared code** principle from ADR-003. It contains ONLY:
- **Interfaces and abstractions** (no implementations)
- **Value objects and patterns** (Result, Error)
- **Contracts** (domain events, export interfaces)

**What this library does NOT contain:**
- ❌ Business logic
- ❌ Infrastructure implementations
- ❌ Framework-specific code
- ❌ Heavy dependencies

Each service remains independent and can implement these abstractions as needed.

## Contents

### 1. Authentication (`GssCommon.Auth`)

```csharp
public interface ICurrentUser
{
    Guid? UserId { get; }
    string? Email { get; }
    string? DisplayName { get; }
    string? TenantId { get; }
    IReadOnlyCollection<string> Roles { get; }
    IReadOnlyCollection<Claim> Claims { get; }
    
    bool IsInRole(string role);
    bool HasPermission(string permission);
}
```

**Usage:**
- Provides user context across services
- Supports multi-tenancy
- Role-based and permission-based authorization

### 2. Result Pattern (`GssCommon.Common`)

```csharp
// Success
var result = Result.Success();
var resultWithValue = Result.Success(user);

// Failure
var error = Error.NotFound("User.NotFound", "User not found");
var result = Result.Failure(error);

// Usage
if (result.IsSuccess)
{
    var value = result.Value;
}
else
{
    var errorMessage = result.Error.Message;
}
```

**Benefits:**
- Railway-oriented programming
- Explicit error handling
- Type-safe results
- No exceptions for business logic failures

**Error Types:**
- `Failure` - General failures
- `Validation` - Validation errors
- `NotFound` - Resource not found
- `Conflict` - Conflict errors

### 3. Domain Events (`GssCommon.Events`)

```csharp
public interface IDomainEvent
{
    Guid EventId { get; }
    DateTime OccurredOn { get; }
}

public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
{
    Task HandleAsync(TEvent domainEvent, CancellationToken cancellationToken = default);
}

public interface IDomainEventDispatcher
{
    Task DispatchAsync(IEnumerable<IDomainEvent> domainEvents, CancellationToken cancellationToken = default);
}
```

**Usage:**
```csharp
// Define event
public record UserRegisteredEvent(Guid UserId, string Email) : IDomainEvent
{
    public Guid EventId { get; } = Guid.NewGuid();
    public DateTime OccurredOn { get; } = DateTime.UtcNow;
}

// Handle event
public class SendWelcomeEmailHandler : IDomainEventHandler<UserRegisteredEvent>
{
    public async Task HandleAsync(UserRegisteredEvent domainEvent, CancellationToken cancellationToken)
    {
        // Send welcome email
    }
}
```

### 5. Configurator Models (`GssCommon.Common.Models.Configurator`)

Shared models for the GSS Configurator orchestration flow.

- **`GenericComponent`**: Hierarchical model representing a physical or logical component (Uprights, Beams, Bays).
- **`BomItem`**: Represents a physical part to be included in the Bill of Materials.
- **`PartMetadata`**: Metadata for parts resolved from the Catalog.
- **`Blueprint`**: Models for hierarchical and flattened rule sets.

**Usage:**
- Used by `configuration-service` for layout expansion.
- Used by `rule-service` for evaluation context.
- Used by `catalog-service` and `bom-service` for data exchange.

## Installation

### Option 1: NuGet Package (Recommended for Production)

```bash
dotnet add package GssCommon
```

### Option 2: Project Reference (For Development)

```xml
<ItemGroup>
  <ProjectReference Include="..\..\gss-common\GssCommon.csproj" />
</ItemGroup>
```

## Usage in Services

### catalog-service Example

```csharp
using GssCommon.Common;
using GssCommon.Auth;

// In handler
public class CreateSkuCommandHandler : IRequestHandler<CreateSkuCommand, Result<Guid>>
{
    private readonly ICurrentUser _currentUser;
    
    public async Task<Result<Guid>> Handle(CreateSkuCommand request, CancellationToken cancellationToken)
    {
        // Validation
        if (string.IsNullOrEmpty(request.Code))
        {
            return Result.Failure<Guid>(Error.Validation("Sku.CodeRequired", "SKU code is required"));
        }
        
        // Create SKU
        var sku = Sku.Create(request.Code, request.Name, createdBy: _currentUser.Email);
        
        // Success
        return Result.Success(sku.Id);
    }
}
```

## Design Principles

### 1. **Minimal Dependencies**
- Zero external NuGet packages
- Only uses .NET BCL

### 2. **Interface-Driven**
- Provides contracts, not implementations
- Services implement as needed

### 3. **Immutable Value Objects**
- `Error`, `ExportOptions`, `ExportResult` are records
- Thread-safe by design

### 4. **Opt-In Usage**
- Services use what they need
- No forced dependencies

## When to Use

✅ **Use gss-common for:**
- Authentication/authorization abstractions
- Result pattern for error handling
- Domain event contracts
- Export interfaces

❌ **Don't use gss-common for:**
- Business logic (belongs in domain layer)
- Infrastructure code (belongs in infrastructure layer)
- Framework-specific code (use standard packages)
- Heavy dependencies (keep it lightweight)

## Versioning

- **Version 1.0.0** - Initial release
- Follows Semantic Versioning (SemVer)
- Breaking changes increment major version

## Migration from Shared-Libs

If migrating from `GSSDesingConfigurator.Shared.*`:

| Old | New | Notes |
|-----|-----|-------|
| `GSSDesingConfigurator.Shared.Auth.ICurrentUser` | `GssCommon.Auth.ICurrentUser` | Same interface |
| `GSSDesingConfigurator.Shared.Contracts.Result` | `GssCommon.Common.Result` | Simplified |
| `GSSDesingConfigurator.Shared.Contracts.IDomainEvent` | `GssCommon.Events.IDomainEvent` | Same interface |
| Custom CQRS abstractions | Use MediatR | Standard package |
| Custom validation | Use FluentValidation | Standard package |
| Custom logging | Use Serilog/NLog | Standard package |

## Contributing

When adding to gss-common:

1. **Ask:** Is this truly cross-cutting?
2. **Keep it minimal:** Interfaces > Implementations
3. **No dependencies:** Keep the package lightweight
4. **Document:** Update this README

## License

Proprietary - GSS
