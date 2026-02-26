# Contributing to GSS Backend

Thank you for contributing to the GSS Warehouse Design Configurator backend! This guide will help you understand our development practices and how to contribute effectively.

## Table of Contents

- [Code of Conduct](#code-of-conduct)
- [Getting Started](#getting-started)
- [Development Workflow](#development-workflow)
- [Architecture Guidelines](#architecture-guidelines)
- [Code Style](#code-style)
- [Testing Requirements](#testing-requirements)
- [Documentation Standards](#documentation-standards)
- [Pull Request Process](#pull-request-process)
- [Commit Guidelines](#commit-guidelines)

## Code of Conduct

- Be respectful and professional
- Provide constructive feedback
- Focus on what is best for the project
- Show empathy towards other contributors

## Getting Started

### 1. Fork and Clone

```powershell
# Clone the repository
git clone <repository-url>
cd gss-backend

# Add upstream remote
git remote add upstream <original-repository-url>
```

### 2. Set Up Development Environment

#### For Windows Developers (Recommended: WSL2)

**Why WSL2?**
- 10x faster Docker performance
- Native Linux tooling
- Consistent with production environment
- Better compatibility with Docker

**Quick Setup**:

```powershell
# 1. Install WSL2 (PowerShell as Administrator)
wsl --install -d Ubuntu-22.04

# 2. Restart computer

# 3. Open Ubuntu terminal and install .NET 10
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0

# Add to ~/.bashrc
echo 'export DOTNET_ROOT=$HOME/.dotnet' >> ~/.bashrc
echo 'export PATH=$PATH:$DOTNET_ROOT:$DOTNET_ROOT/tools' >> ~/.bashrc
source ~/.bashrc

# 4. Install Docker Desktop with WSL2 backend
# Download from: https://www.docker.com/products/docker-desktop
# Enable WSL2 integration in Docker Desktop settings

# 5. Verify installations
dotnet --version
docker --version
```

**VS Code Setup**:
```bash
# Install Remote - WSL extension in VS Code
# Then open project from WSL terminal:
cd ~/projects/gss-backend
code .
```

> [!IMPORTANT]
> Always work inside WSL filesystem (`~/projects/`) for best performance, not in `/mnt/c/`.

#### For macOS/Linux Developers

```bash
# Install .NET 10 SDK
# Download from: https://dotnet.microsoft.com/download/dotnet/10.0

# Install Docker Desktop (macOS) or Docker Engine (Linux)
# macOS: https://www.docker.com/products/docker-desktop
# Linux: https://docs.docker.com/engine/install/

# Verify installations
dotnet --version
docker --version
```

### 3. Run the Project

```powershell
# Using Docker Compose (recommended)
docker-compose up -d

# OR run individual service
cd Services/admin-service
dotnet restore
dotnet run
```

### 4. Review Production Readiness Guidelines

Before starting development, review:
- [Service Design Checklist](docs/service-design-checklist.md) - Production readiness requirements
- [Coding Standards](docs/coding-standards.md) - C# coding standards and best practices

All services must meet these criteria before deployment.

## Development Workflow

### Branch Strategy

We use **Git Flow** with the following branches:

- `main` - Production-ready code
- `develop` - Integration branch for features
- `feature/*` - New features
- `bugfix/*` - Bug fixes
- `hotfix/*` - Critical production fixes
- `release/*` - Release preparation

### Creating a Feature Branch

```powershell
# Update develop branch
git checkout develop
git pull upstream develop

# Create feature branch
git checkout -b feature/add-user-authentication

# Work on your feature...
git add .
git commit -m "feat: add JWT authentication"

# Push to your fork
git push origin feature/add-user-authentication
```

### Keeping Your Branch Updated

```powershell
# Fetch latest changes
git fetch upstream

# Rebase your branch
git rebase upstream/develop

# Resolve conflicts if any, then
git rebase --continue

# Force push to your fork
git push origin feature/add-user-authentication --force
```

## Architecture Guidelines

### Domain-Driven Design (DDD)

All services follow DDD principles with clear layer separation:

#### 1. Domain Layer (`src/domain/`)

**Purpose**: Core business logic, independent of infrastructure

**Components**:
- **Aggregates**: Root entities that ensure consistency
- **Entities**: Objects with identity
- **Value Objects**: Immutable objects without identity
- **Domain Services**: Business logic that doesn't fit in entities
- **Domain Events**: Communicate state changes

**Rules**:
- ✅ No dependencies on other layers
- ✅ Rich domain models with encapsulation
- ✅ Business rules enforced in domain
- ❌ No infrastructure concerns (DB, HTTP, etc.)
- ❌ No anemic models (getters/setters only)

**Example**:

```csharp
// Good: Rich domain model
public class User
{
    private User() { } // Private constructor
    
    public Guid Id { get; private set; }
    public Email Email { get; private set; }
    public UserStatus Status { get; private set; }
    
    public static User Create(Email email, DisplayName displayName)
    {
        // Business rules enforced here
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = email,
            Status = UserStatus.Pending
        };
        return user;
    }
    
    public void Activate()
    {
        if (Status == UserStatus.Active)
            throw new InvalidOperationException("User already active");
            
        Status = UserStatus.Active;
    }
}

// Bad: Anemic model
public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; }
    public string Status { get; set; }
}
```

#### 2. Application Layer (`src/application/`)

**Purpose**: Orchestrate domain logic, implement use cases

**Components**:
- **Commands**: Write operations (Create, Update, Delete)
- **Queries**: Read operations (Get, List, Search)
- **Handlers**: Execute commands and queries
- **DTOs**: Data transfer objects for API
- **Validators**: FluentValidation rules

**Rules**:
- ✅ Use MediatR for CQRS
- ✅ Thin handlers that delegate to domain
- ✅ Validate inputs with FluentValidation
- ❌ No business logic (belongs in domain)
- ❌ No direct database access (use repositories)

**Example**:

```csharp
// Command
public record CreateUserCommand(string Email, string DisplayName) : IRequest<Guid>;

// Validator
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Email).NotEmpty().EmailAddress();
        RuleFor(x => x.DisplayName).NotEmpty().Length(2, 100);
    }
}

// Handler
public class CreateUserCommandHandler : IRequestHandler<CreateUserCommand, Guid>
{
    private readonly IUserRepository _repository;
    
    public async Task<Guid> Handle(CreateUserCommand request, CancellationToken ct)
    {
        var email = Email.Create(request.Email);
        var displayName = DisplayName.Create(request.DisplayName);
        
        var user = User.Create(email, displayName);
        
        await _repository.AddAsync(user);
        
        return user.Id;
    }
}
```

#### 3. Infrastructure Layer (`src/infrastructure/`)

**Purpose**: Technical concerns and external dependencies

**Components**:
- **Repositories**: Data access implementations
- **Migrations**: Database schema versioning
- **External Services**: Third-party API clients
- **File Storage**: S3, Azure Blob, etc.

**Rules**:
- ✅ Implement domain interfaces
- ✅ Use Dapper for data access
- ✅ FluentMigrator for migrations
- ❌ No business logic

**Example**:

```csharp
public class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    
    public async Task<User?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        
        var sql = "SELECT * FROM users WHERE id = @Id";
        var row = await connection.QuerySingleOrDefaultAsync(sql, new { Id = id });
        
        if (row == null) return null;
        
        // Map to domain model
        return User.Reconstitute(row.id, row.email, row.display_name, row.status);
    }
}
```

#### 4. API Layer (`src/api/`)

**Purpose**: HTTP endpoints and request/response handling

**Rules**:
- ✅ Minimal controllers (delegate to MediatR)
- ✅ Use DTOs for requests/responses
- ✅ Document with XML comments for Swagger
- ✅ Return appropriate HTTP status codes
- ❌ No business logic

**Example**:

```csharp
[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    private readonly IMediator _mediator;
    
    /// <summary>
    /// Create a new user
    /// </summary>
    /// <param name="request">User creation details</param>
    /// <returns>Created user ID</returns>
    [HttpPost]
    [ProducesResponseType(typeof(Guid), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest request)
    {
        var command = new CreateUserCommand(request.Email, request.DisplayName);
        var userId = await _mediator.Send(command);
        
        return CreatedAtAction(nameof(GetUser), new { id = userId }, userId);
    }
}
```

### CQRS Pattern

Separate read and write operations:

- **Commands**: Modify state, return void or ID
- **Queries**: Read state, return DTOs
- **No mixing**: A handler should be either command or query, not both

### Value Objects

Use value objects for domain concepts:

```csharp
public record Email
{
    private Email(string value) => Value = value;
    
    public string Value { get; }
    
    public static Email Create(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            throw new ArgumentException("Email cannot be empty");
            
        if (!Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
            throw new ArgumentException("Invalid email format");
            
        return new Email(email);
    }
}
```

## Code Style

### C# Conventions

- Use **C# 12** features (record types, pattern matching, etc.)
- Follow [Microsoft C# Coding Conventions](https://docs.microsoft.com/en-us/dotnet/csharp/fundamentals/coding-style/coding-conventions)
- Use **nullable reference types** (`#nullable enable`)
- Prefer **expression-bodied members** for simple methods
- Use **file-scoped namespaces** (C# 10+)

### Naming Conventions

| Type | Convention | Example |
|------|------------|---------|
| Classes | PascalCase | `UserRepository` |
| Interfaces | IPascalCase | `IUserRepository` |
| Methods | PascalCase | `GetUserById` |
| Parameters | camelCase | `userId` |
| Private fields | _camelCase | `_repository` |
| Constants | PascalCase | `MaxRetryCount` |
| Enums | PascalCase | `UserStatus` |

### File Organization

```
src/
├── api/
│   └── UsersController.cs
├── application/
│   ├── commands/
│   │   ├── CreateUserCommand.cs
│   │   └── CreateUserCommandValidator.cs
│   ├── queries/
│   │   └── GetUserByIdQuery.cs
│   └── handlers/
│       ├── CreateUserCommandHandler.cs
│       └── GetUserByIdQueryHandler.cs
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

## Testing Requirements

### Test Pyramid

1. **Unit Tests** (70%): Fast, isolated, test domain logic
2. **Integration Tests** (20%): Test with real database
3. **Contract Tests** (10%): API contract validation

### Unit Tests

Test domain logic and application handlers:

```csharp
public class UserTests
{
    [Fact]
    public void Create_WithValidData_ReturnsUser()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var displayName = DisplayName.Create("Test User");
        
        // Act
        var user = User.Create(email, displayName);
        
        // Assert
        user.Should().NotBeNull();
        user.Email.Should().Be(email);
        user.Status.Should().Be(UserStatus.Pending);
    }
    
    [Fact]
    public void Activate_WhenAlreadyActive_ThrowsException()
    {
        // Arrange
        var user = CreateActiveUser();
        
        // Act
        var act = () => user.Activate();
        
        // Assert
        act.Should().Throw<InvalidOperationException>();
    }
}
```

### Integration Tests

Test with real database:

```csharp
public class UserRepositoryTests : IClassFixture<DatabaseFixture>
{
    private readonly IDbConnectionFactory _connectionFactory;
    
    [Fact]
    public async Task AddAsync_WithValidUser_PersistsToDatabase()
    {
        // Arrange
        var repository = new UserRepository(_connectionFactory);
        var user = CreateTestUser();
        
        // Act
        await repository.AddAsync(user);
        
        // Assert
        var retrieved = await repository.GetByIdAsync(user.Id);
        retrieved.Should().NotBeNull();
        retrieved!.Email.Should().Be(user.Email);
    }
}
```

### Test Coverage

- Aim for **80%+ code coverage**
- **100% coverage** for domain layer
- Focus on business logic, not infrastructure

### Running Tests

```powershell
# Run all tests
dotnet test

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageReportsDirectory=./coverage

# Run specific category
dotnet test --filter Category=Unit
dotnet test --filter Category=Integration
```

## Documentation Standards

### Code Documentation

Use XML comments for public APIs:

```csharp
/// <summary>
/// Creates a new user with the specified email and display name.
/// </summary>
/// <param name="email">User's email address</param>
/// <param name="displayName">User's display name</param>
/// <returns>The created user instance</returns>
/// <exception cref="ArgumentException">Thrown when email or display name is invalid</exception>
public static User Create(Email email, DisplayName displayName)
{
    // Implementation
}
```

### README Files

Each service must have a README with:

1. Overview and key features
2. Architecture diagram
3. Getting started guide
4. API endpoints
5. Domain models
6. Configuration
7. Testing instructions

### Architecture Decision Records (ADRs)

Document significant decisions in `docs/adr/`:

```markdown
# ADR-001: Use PostgreSQL for All Services

## Status
Accepted

## Context
We need to choose a database for our microservices...

## Decision
We will use PostgreSQL for all services...

## Consequences
- Consistent tooling across services
- Strong ACID guarantees
- JSON support for flexible schemas
```

## Pull Request Process

### 1. Before Creating PR

- ✅ All tests pass locally
- ✅ Code follows style guidelines
- ✅ Documentation updated
- ✅ Commits follow commit guidelines
- ✅ Branch is up-to-date with develop

### 2. PR Title Format

```
<type>(<scope>): <description>

Examples:
feat(admin-service): add user authentication
fix(catalog-service): resolve SKU duplication bug
docs(readme): update quick start guide
```

### 3. PR Description Template

```markdown
## Description
Brief description of changes

## Type of Change
- [ ] Bug fix
- [ ] New feature
- [ ] Breaking change
- [ ] Documentation update

## Testing
- [ ] Unit tests added/updated
- [ ] Integration tests added/updated
- [ ] Manual testing performed

## Checklist
- [ ] Code follows style guidelines
- [ ] Self-review completed
- [ ] Documentation updated
- [ ] No new warnings
- [ ] Tests pass locally
```

### 4. Review Process

- At least **1 approval** required
- All CI checks must pass
- No unresolved comments
- Squash and merge to develop

## Commit Guidelines

### Commit Message Format

```
<type>(<scope>): <subject>

<body>

<footer>
```

### Types

- `feat`: New feature
- `fix`: Bug fix
- `docs`: Documentation changes
- `style`: Code style changes (formatting)
- `refactor`: Code refactoring
- `test`: Adding/updating tests
- `chore`: Maintenance tasks

### Examples

```
feat(admin-service): add JWT authentication

Implement JWT token generation and validation for user authentication.
Includes refresh token support and token expiration handling.

Closes #123

---

fix(catalog-service): resolve SKU duplication issue

Fixed bug where duplicate SKUs could be created due to race condition.
Added unique constraint on SKU code field.

Fixes #456

---

docs(contributing): add testing guidelines

Added comprehensive testing guidelines including unit test examples,
integration test setup, and coverage requirements.
```

### Commit Best Practices

- **Atomic commits**: One logical change per commit
- **Descriptive messages**: Explain what and why, not how
- **Present tense**: "Add feature" not "Added feature"
- **Reference issues**: Include issue numbers when applicable

## Additional Guidelines

### Performance

- Use async/await for I/O operations
- Avoid N+1 queries (use joins or batch queries)
- Use pagination for large result sets
- Cache frequently accessed data

### Security

- Never commit secrets or credentials
- Use environment variables for configuration
- Validate all user inputs
- Use parameterized queries (prevent SQL injection)
- Implement proper authentication and authorization

### Error Handling

```csharp
// Good: Specific exceptions
public class UserNotFoundException : Exception
{
    public UserNotFoundException(Guid userId) 
        : base($"User with ID {userId} not found") { }
}

// Use in code
var user = await _repository.GetByIdAsync(userId) 
    ?? throw new UserNotFoundException(userId);
```

### Logging

```csharp
// Use structured logging
_logger.LogInformation("User {UserId} created successfully", user.Id);
_logger.LogWarning("Failed login attempt for {Email}", email);
_logger.LogError(ex, "Error processing order {OrderId}", orderId);
```

## Production Readiness

### Service Design Checklist

All services must complete the [Service Design Checklist](docs/service-design-checklist.md) before production deployment. This AWS-aligned checklist covers:

1. **Operational Excellence**: Health checks, logging, observability
2. **Security**: Authentication, authorization, secrets management
3. **Reliability**: Timeouts, retries, circuit breakers, graceful degradation
4. **Performance**: Resource sizing, latency measurement, optimization
5. **Cost Optimization**: Right-sizing, auto-scaling, managed services
6. **Sustainability**: Clean architecture, technical debt management
7. **API Contracts**: Documentation, versioning, backward compatibility
8. **Testing**: Unit, integration, contract tests

### Pre-Production Requirements

Before deploying to production:

- [ ] Complete Service Design Checklist
- [ ] All checklist items marked as ✅, ⚠️ (with ADR), or N/A
- [ ] Health endpoint implemented and tested
- [ ] Structured logging with correlation IDs
- [ ] Authentication and authorization implemented
- [ ] All secrets in AWS Secrets Manager
- [ ] Timeouts and circuit breakers configured
- [ ] Auto-scaling policies defined
- [ ] API documentation up to date
- [ ] 80%+ test coverage
- [ ] CI/CD pipeline configured
- [ ] Architecture review completed
- [ ] Security review completed

### Production Deployment Checklist

- [ ] Service deployed to staging and tested
- [ ] Load testing completed
- [ ] Monitoring and alerts configured
- [ ] Runbook created for on-call team
- [ ] Rollback plan documented
- [ ] Database migrations tested
- [ ] Feature flags configured (if applicable)
- [ ] Blue/green or canary deployment strategy defined

## Getting Help

- **Questions**: Create a discussion in the repository
- **Bugs**: Create an issue with reproduction steps
- **Features**: Create an issue with use case description
- **Urgent**: Contact the development team directly

## Resources

- [.NET Documentation](https://docs.microsoft.com/en-us/dotnet/)
- [Domain-Driven Design](https://martinfowler.com/bliki/DomainDrivenDesign.html)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [PostgreSQL Documentation](https://www.postgresql.org/docs/)

---

Thank you for contributing to GSS Backend! 🚀
