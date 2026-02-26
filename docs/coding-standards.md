# Coding Standards and Guidelines

## Purpose

This document defines coding standards for the GSS Backend project. All code must adhere to these standards before merging to `develop` or `main`.

---

## Table of Contents

- [C# Language Standards](#c-language-standards)
- [Naming Conventions](#naming-conventions)
- [Code Organization](#code-organization)
- [DDD Patterns](#ddd-patterns)
- [Error Handling](#error-handling)
- [Async/Await](#asyncawait)
- [Dependency Injection](#dependency-injection)
- [Logging](#logging)
- [Comments and Documentation](#comments-and-documentation)
- [Testing Standards](#testing-standards)
- [Security Standards](#security-standards)
- [Performance Guidelines](#performance-guidelines)

---

## C# Language Standards

### Language Version

- **Use C# 12** features where appropriate
- **Target .NET 10**
- **Enable nullable reference types** in all projects

```xml
<PropertyGroup>
  <TargetFramework>net10.0</TargetFramework>
  <Nullable>enable</Nullable>
  <LangVersion>12.0</LangVersion>
</PropertyGroup>
```

### Modern C# Features

✅ **Use**:
- File-scoped namespaces
- Record types for DTOs and value objects
- Pattern matching
- Expression-bodied members
- Primary constructors (C# 12)
- Collection expressions (C# 12)

```csharp
// ✅ Good: File-scoped namespace
namespace GSS.AdminService.Domain.ValueObjects;

// ✅ Good: Record for value object
public record Email
{
    private Email(string value) => Value = value;
    
    public string Value { get; }
    
    public static Email Create(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        if (!IsValidEmail(email))
            throw new ArgumentException("Invalid email format");
        return new Email(email);
    }
    
    private static bool IsValidEmail(string email) =>
        Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$");
}

// ✅ Good: Primary constructor (C# 12)
public class UserRepository(IDbConnectionFactory connectionFactory) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var connection = connectionFactory.CreateConnection();
        // ...
    }
}

// ❌ Bad: Old-style namespace
namespace GSS.AdminService.Domain.ValueObjects
{
    public class Email { }
}
```

---

## Naming Conventions

### General Rules

| Type | Convention | Example |
|------|------------|---------|
| **Namespace** | PascalCase | `GSS.AdminService.Domain` |
| **Class** | PascalCase | `UserRepository` |
| **Interface** | IPascalCase | `IUserRepository` |
| **Method** | PascalCase | `GetUserById` |
| **Property** | PascalCase | `DisplayName` |
| **Parameter** | camelCase | `userId` |
| **Local Variable** | camelCase | `userName` |
| **Private Field** | _camelCase | `_repository` |
| **Constant** | PascalCase | `MaxRetryCount` |
| **Enum** | PascalCase | `UserStatus` |
| **Enum Value** | PascalCase | `Active`, `Pending` |

### Specific Naming Patterns

**Commands**: `{Verb}{Entity}Command`
```csharp
public record CreateUserCommand(string Email, string DisplayName) : IRequest<Guid>;
public record UpdateUserCommand(Guid Id, string DisplayName) : IRequest;
public record DeleteUserCommand(Guid Id) : IRequest;
```

**Queries**: `Get{Entity}[By{Criteria}]Query`
```csharp
public record GetUserByIdQuery(Guid Id) : IRequest<UserDto>;
public record GetUserByEmailQuery(string Email) : IRequest<UserDto>;
public record ListUsersQuery(int Page, int PageSize) : IRequest<List<UserDto>>;
```

**Handlers**: `{Command/Query}Handler`
```csharp
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
public class GetUserByIdQueryHandler : IRequestHandler<GetUserByIdQuery, UserDto>
```

**Repositories**: `I{Entity}Repository` / `{Entity}Repository`
```csharp
public interface IUserRepository
public class UserRepository : IUserRepository
```

**Value Objects**: Descriptive noun
```csharp
public record Email
public record DisplayName
public record UserRole
```

---

## Code Organization

### File Structure

**One class per file** (except nested classes)

```
src/
├── api/
│   └── UsersController.cs
├── application/
│   ├── commands/
│   │   ├── CreateUserCommand.cs
│   │   └── UpdateUserCommand.cs
│   ├── queries/
│   │   └── GetUserByIdQuery.cs
│   ├── handlers/
│   │   ├── CreateUserCommandHandler.cs
│   │   └── GetUserByIdQueryHandler.cs
│   └── dtos/
│       └── UserDto.cs
├── domain/
│   ├── aggregates/
│   │   └── User.cs
│   ├── valueobjects/
│   │   ├── Email.cs
│   │   └── DisplayName.cs
│   └── repositories/
│       └── IUserRepository.cs
└── infrastructure/
    ├── persistence/
    │   └── UserRepository.cs
    └── migrations/
        └── M20260108001_CreateUsersTable.cs
```

### Class Member Order

```csharp
public class User
{
    // 1. Constants
    private const int MaxDisplayNameLength = 100;
    
    // 2. Private fields
    private readonly List<DomainEvent> _domainEvents = [];
    
    // 3. Constructors (private for aggregates)
    private User() { }
    
    // 4. Public properties
    public Guid Id { get; private set; }
    public Email Email { get; private set; }
    
    // 5. Factory methods
    public static User Create(Email email, DisplayName displayName)
    {
        // ...
    }
    
    // 6. Public methods
    public void Activate()
    {
        // ...
    }
    
    // 7. Private methods
    private void AddDomainEvent(DomainEvent domainEvent)
    {
        // ...
    }
}
```

---

## DDD Patterns

### Aggregates

✅ **Do**:
- Use private constructors
- Use factory methods for creation
- Encapsulate all state changes
- Use private setters
- Validate invariants

```csharp
public class User
{
    private User() { } // Private constructor
    
    public Guid Id { get; private set; }
    public Email Email { get; private set; }
    public UserStatus Status { get; private set; }
    
    // Factory method
    public static User Create(Email email, DisplayName displayName)
    {
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Status = UserStatus.Pending
        };
        
        user.Validate();
        return user;
    }
    
    // Behavior, not setters
    public void Activate()
    {
        if (Status == UserStatus.Active)
            throw new InvalidOperationException("User is already active");
            
        Status = UserStatus.Active;
    }
    
    private void Validate()
    {
        if (Email == null)
            throw new InvalidOperationException("Email is required");
    }
}
```

❌ **Don't**:
```csharp
// Bad: Anemic model with public setters
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Status { get; set; }
}
```

### Value Objects

```csharp
public record Email
{
    private Email(string value) => Value = value;
    
    public string Value { get; }
    
    public static Email Create(string email)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(email);
        
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ArgumentException("Invalid email format");
            
        return new Email(email.ToLowerInvariant());
    }
    
    // Value equality is automatic with records
}
```

### Repository Pattern

```csharp
// Interface in domain layer
public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(Email email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
}

// Implementation in infrastructure layer
public class UserRepository(IDbConnectionFactory connectionFactory) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var connection = connectionFactory.CreateConnection();
        
        var sql = "SELECT * FROM users WHERE id = @Id AND is_deleted = false";
        var row = await connection.QuerySingleOrDefaultAsync(sql, new { Id = id });
        
        return row == null ? null : MapToUser(row);
    }
    
    private static User MapToUser(dynamic row)
    {
        // Reconstitute aggregate from database
        return User.Reconstitute(
            row.id,
            Email.Create(row.email),
            DisplayName.Create(row.display_name),
            Enum.Parse<UserStatus>(row.status)
        );
    }
}
```

---

## Error Handling

### Exception Guidelines

✅ **Do**:
- Use specific exception types
- Create domain-specific exceptions
- Include meaningful messages
- Log exceptions with context

```csharp
// Custom domain exception
public class UserNotFoundException : Exception
{
    public UserNotFoundException(Guid userId) 
        : base($"User with ID {userId} not found")
    {
        UserId = userId;
    }
    
    public Guid UserId { get; }
}

// Usage
public async Task<User> GetUserAsync(Guid id)
{
    var user = await _repository.GetByIdAsync(id);
    if (user == null)
        throw new UserNotFoundException(id);
    
    return user;
}
```

### Global Exception Handling

```csharp
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        var exceptionFeature = context.Features.Get<IExceptionHandlerFeature>();
        var exception = exceptionFeature?.Error;
        
        // Log with full details
        logger.LogError(exception, "Unhandled exception occurred");
        
        // Return sanitized error
        var (statusCode, message) = exception switch
        {
            UserNotFoundException => (404, "User not found"),
            ValidationException => (400, "Validation failed"),
            _ => (500, "An internal error occurred")
        };
        
        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new
        {
            error = message,
            requestId = context.TraceIdentifier
        });
    });
});
```

---

## Async/Await

### Rules

✅ **Do**:
- Use async/await for all I/O operations
- Use `Task` for void methods, not `async void` (except event handlers)
- Use `ConfigureAwait(false)` in library code (not needed in ASP.NET Core)
- Suffix async methods with `Async`

```csharp
// ✅ Good
public async Task<User> GetUserAsync(Guid id)
{
    var user = await _repository.GetByIdAsync(id);
    return user;
}

// ✅ Good: Parallel execution
public async Task<ConfigurationSummary> GetSummaryAsync(Guid id)
{
    var configTask = _configService.GetAsync(id);
    var skusTask = _catalogService.GetSkusAsync(id);
    var rulesTask = _ruleService.GetRulesAsync(id);
    
    await Task.WhenAll(configTask, skusTask, rulesTask);
    
    return new ConfigurationSummary
    {
        Configuration = configTask.Result,
        Skus = skusTask.Result,
        Rules = rulesTask.Result
    };
}

// ❌ Bad: async void
public async void ProcessUser(Guid id) // Should be Task
{
    await _service.ProcessAsync(id);
}

// ❌ Bad: Blocking on async
public User GetUser(Guid id)
{
    return _repository.GetByIdAsync(id).Result; // Deadlock risk!
}
```

---

## Dependency Injection

### Constructor Injection

✅ **Do**:
- Use constructor injection
- Use primary constructors (C# 12)
- Inject interfaces, not implementations
- Keep constructors simple

```csharp
// ✅ Good: Primary constructor
public class CreateUserCommandHandler(
    IUserRepository repository,
    ILogger<CreateUserCommandHandler> logger) 
    : IRequestHandler<CreateUserCommand, Guid>
{
    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken ct)
    {
        logger.LogInformation("Creating user with email {Email}", request.Email);
        
        var email = Email.Create(request.Email);
        var user = User.Create(email, DisplayName.Create(request.DisplayName));
        
        await repository.AddAsync(user);
        
        return user.Id;
    }
}
```

### Service Registration

```csharp
// Repositories
services.AddScoped<IUserRepository, UserRepository>();

// MediatR handlers (auto-registered)
services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly()));

// Validators
services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

// HTTP clients with Polly
services.AddHttpClient<ICatalogServiceClient, CatalogServiceClient>()
    .AddPolicyHandler(GetRetryPolicy())
    .AddPolicyHandler(GetCircuitBreakerPolicy());
```

---

## Logging

### Structured Logging

✅ **Always use structured logging**:

```csharp
// ✅ Good: Structured
_logger.LogInformation(
    "User {UserId} created with email {Email}",
    user.Id,
    user.Email.Value
);

// ❌ Bad: String interpolation
_logger.LogInformation($"User {user.Id} created with email {user.Email.Value}");
```

### Log Levels

- **Trace**: Very detailed, development only
- **Debug**: Detailed flow, development/staging
- **Information**: General flow, production
- **Warning**: Unexpected but handled
- **Error**: Errors and exceptions
- **Critical**: Critical failures

```csharp
_logger.LogTrace("Entering GetUserAsync with id {UserId}", id);
_logger.LogDebug("Retrieved user from cache: {UserId}", id);
_logger.LogInformation("User {UserId} created successfully", user.Id);
_logger.LogWarning("User {UserId} not found in cache, querying database", id);
_logger.LogError(ex, "Failed to create user with email {Email}", email);
_logger.LogCritical(ex, "Database connection failed");
```

### Don't Log Sensitive Data

```csharp
// ✅ Good
_logger.LogInformation("User created with ID {UserId}", user.Id);

// ❌ Bad: Logs password
_logger.LogInformation("User created: {@User}", user);
```

---

## Comments and Documentation

### XML Documentation

**Required for**:
- All public APIs
- All controllers
- All public methods

```csharp
/// <summary>
/// Creates a new user in the system
/// </summary>
/// <param name="request">User creation details including email and display name</param>
/// <returns>The ID of the created user</returns>
/// <exception cref="ValidationException">Thrown when request data is invalid</exception>
/// <response code="201">User created successfully</response>
/// <response code="400">Invalid request data</response>
/// <response code="409">User with email already exists</response>
[HttpPost]
[ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
[ProducesResponseType(typeof(ValidationProblemDetails), StatusCodes.Status400BadRequest)]
[ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status409Conflict)]
public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
{
    // Implementation
}
```

### Code Comments

✅ **Good comments** (explain WHY):
```csharp
// Use exponential backoff to avoid overwhelming the downstream service
await Task.Delay(TimeSpan.FromSeconds(Math.Pow(2, retryCount)));

// Soft delete to maintain audit trail
user.IsDeleted = true;
```

❌ **Bad comments** (explain WHAT - code already does this):
```csharp
// Set user to deleted
user.IsDeleted = true;

// Loop through users
foreach (var user in users)
```

---

## Testing Standards

### Test Naming

```csharp
// Pattern: MethodName_Scenario_ExpectedResult
[Fact]
public void Create_WithValidEmail_ReturnsUser()

[Fact]
public void Activate_WhenAlreadyActive_ThrowsException()

[Fact]
public async Task GetByIdAsync_WhenUserExists_ReturnsUser()

[Fact]
public async Task GetByIdAsync_WhenUserNotFound_ReturnsNull()
```

### AAA Pattern

```csharp
[Fact]
public void Activate_WhenPending_ActivatesUser()
{
    // Arrange
    var user = UserFixtures.CreatePendingUser();
    
    // Act
    user.Activate();
    
    // Assert
    user.Status.Should().Be(UserStatus.Active);
}
```

### Use FluentAssertions

```csharp
// ✅ Good
user.Should().NotBeNull();
user.Email.Value.Should().Be("test@example.com");
user.Status.Should().Be(UserStatus.Active);

// ❌ Avoid
Assert.NotNull(user);
Assert.Equal("test@example.com", user.Email.Value);
```

---

## Security Standards

### Input Validation

**Always validate external inputs**:

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

### SQL Injection Prevention

**Always use parameterized queries**:

```csharp
// ✅ Good: Parameterized
var sql = "SELECT * FROM users WHERE email = @Email";
var user = await connection.QuerySingleOrDefaultAsync(sql, new { Email = email });

// ❌ Bad: String concatenation
var sql = $"SELECT * FROM users WHERE email = '{email}'"; // SQL INJECTION!
```

### Secrets Management

```csharp
// ✅ Good: From configuration/secrets manager
var connectionString = configuration["ConnectionStrings:DefaultConnection"];

// ❌ Bad: Hardcoded
var connectionString = "Server=prod;Password=secret123"; // NEVER!
```

---

## Performance Guidelines

### Database Queries

```csharp
// ✅ Good: Single query with join
var sql = @"
    SELECT u.*, r.name as role_name
    FROM users u
    JOIN roles r ON u.role_id = r.id
    WHERE u.id = @Id";

// ❌ Bad: N+1 queries
var user = await GetUserAsync(id);
var role = await GetRoleAsync(user.RoleId); // Separate query!
```

### Avoid Premature Optimization

```csharp
// ✅ Good: Clear and simple
var activeUsers = users.Where(u => u.Status == UserStatus.Active).ToList();

// ❌ Bad: Premature optimization (unless proven bottleneck)
var activeUsers = new List<User>(users.Count);
for (int i = 0; i < users.Count; i++)
{
    if (users[i].Status == UserStatus.Active)
        activeUsers.Add(users[i]);
}
```

---

## Code Review Checklist

Before submitting a PR, verify:

- [ ] Code follows naming conventions
- [ ] DDD patterns correctly applied
- [ ] All public APIs have XML documentation
- [ ] Input validation implemented
- [ ] Structured logging used
- [ ] Async/await used correctly
- [ ] No hardcoded secrets
- [ ] Tests written and passing
- [ ] No compiler warnings
- [ ] Code is self-documenting

---

## Tools and Enforcement

### EditorConfig

Create `.editorconfig` in repository root:

```ini
root = true

[*.cs]
indent_style = space
indent_size = 4
end_of_line = crlf
charset = utf-8
trim_trailing_whitespace = true
insert_final_newline = true

# Naming conventions
dotnet_naming_rule.private_fields_should_be_camelcase.severity = warning
dotnet_naming_rule.private_fields_should_be_camelcase.symbols = private_fields
dotnet_naming_rule.private_fields_should_be_camelcase.style = camelcase_with_underscore

dotnet_naming_symbols.private_fields.applicable_kinds = field
dotnet_naming_symbols.private_fields.applicable_accessibilities = private

dotnet_naming_style.camelcase_with_underscore.capitalization = camel_case
dotnet_naming_style.camelcase_with_underscore.required_prefix = _
```

### Code Analysis

Enable in `.csproj`:

```xml
<PropertyGroup>
  <TreatWarningsAsErrors>true</TreatWarningsAsErrors>
  <EnableNETAnalyzers>true</EnableNETAnalyzers>
  <AnalysisLevel>latest</AnalysisLevel>
  <EnforceCodeStyleInBuild>true</EnforceCodeStyleInBuild>
</PropertyGroup>
```

---

## References

- [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- [.NET Framework Design Guidelines](https://docs.microsoft.com/en-us/dotnet/standard/design-guidelines/)
- [CONTRIBUTING.md](../CONTRIBUTING.md) - Full contribution guide
- [SERVICE_DESIGN_CHECKLIST.md](SERVICE_DESIGN_CHECKLIST.md) - Production readiness
