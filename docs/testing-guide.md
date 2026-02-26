# Testing Guide

Comprehensive testing strategy for GSS Backend services.

## Testing Philosophy

We follow the **Test Pyramid** approach:

```
        /\
       /  \
      / E2E \          10% - End-to-End Tests
     /______\
    /        \
   / Integration\     20% - Integration Tests
  /______________\
 /                \
/   Unit Tests     \   70% - Unit Tests
/____________________\
```

- **Unit Tests**: Fast, isolated, test business logic
- **Integration Tests**: Test with real dependencies (database, external services)
- **Contract Tests**: Verify API contracts
- **E2E Tests**: Full user workflows (minimal, expensive)

## Test Categories

### 1. Unit Tests

**Purpose**: Test domain logic and application handlers in isolation

**Characteristics**:
- ⚡ Fast (< 100ms per test)
- 🔒 Isolated (no external dependencies)
- 🎯 Focused (one concept per test)
- 📊 High coverage (80%+ for domain layer)

**What to Test**:
- Domain models (aggregates, entities, value objects)
- Business rules and validations
- Command/Query handlers
- Domain services

**Example**:

```csharp
public class UserTests
{
    [Fact]
    public void Create_WithValidEmail_CreatesUser()
    {
        // Arrange
        var email = Email.Create("test@example.com");
        var displayName = DisplayName.Create("Test User");
        
        // Act
        var user = User.Create(email, displayName);
        
        // Assert
        user.Should().NotBeNull();
        user.Email.Value.Should().Be("test@example.com");
        user.Status.Should().Be(UserStatus.Pending);
    }
    
    [Fact]
    public void Activate_WhenPending_ActivatesUser()
    {
        // Arrange
        var user = CreatePendingUser();
        
        // Act
        user.Activate();
        
        // Assert
        user.Status.Should().Be(UserStatus.Active);
    }
    
    [Fact]
    public void Activate_WhenAlreadyActive_ThrowsException()
    {
        // Arrange
        var user = CreateActiveUser();
        
        // Act
        var act = () => user.Activate();
        
        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("User is already active");
    }
}
```

**Value Object Tests**:

```csharp
public class EmailTests
{
    [Theory]
    [InlineData("test@example.com")]
    [InlineData("user.name@company.co.uk")]
    [InlineData("admin+tag@domain.com")]
    public void Create_WithValidEmail_ReturnsEmail(string validEmail)
    {
        // Act
        var email = Email.Create(validEmail);
        
        // Assert
        email.Value.Should().Be(validEmail);
    }
    
    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("invalid")]
    [InlineData("@example.com")]
    [InlineData("user@")]
    public void Create_WithInvalidEmail_ThrowsException(string invalidEmail)
    {
        // Act
        var act = () => Email.Create(invalidEmail);
        
        // Assert
        act.Should().Throw<ArgumentException>();
    }
}
```

### 2. Integration Tests

**Purpose**: Test with real dependencies (database, message queues, etc.)

**Characteristics**:
- 🐢 Slower (database I/O)
- 🔗 Tests real integrations
- 🧹 Requires setup/teardown
- 📦 Tests repository implementations

**Setup**:

```csharp
public class DatabaseFixture : IDisposable
{
    public IDbConnectionFactory ConnectionFactory { get; }
    
    public DatabaseFixture()
    {
        var connectionString = "Server=localhost;Database=test_db;...";
        ConnectionFactory = new DbConnectionFactory(connectionString);
        
        // Run migrations
        RunMigrations(connectionString);
    }
    
    public void Dispose()
    {
        // Clean up test database
        DropDatabase();
    }
}

[CollectionDefinition("Database")]
public class DatabaseCollection : ICollectionFixture<DatabaseFixture> { }
```

**Example**:

```csharp
[Collection("Database")]
public class UserRepositoryTests
{
    private readonly IDbConnectionFactory _connectionFactory;
    
    public UserRepositoryTests(DatabaseFixture fixture)
    {
        _connectionFactory = fixture.ConnectionFactory;
    }
    
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
        retrieved!.Email.Value.Should().Be(user.Email.Value);
    }
    
    [Fact]
    public async Task GetByEmailAsync_WhenExists_ReturnsUser()
    {
        // Arrange
        var repository = new UserRepository(_connectionFactory);
        var user = await SeedUser(repository);
        
        // Act
        var result = await repository.GetByEmailAsync(user.Email);
        
        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(user.Id);
    }
}
```

### 3. Contract Tests

**Purpose**: Verify API contracts (request/response schemas)

**Example**:

```csharp
public class UserApiContractTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;
    
    public UserApiContractTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }
    
    [Fact]
    public async Task CreateUser_WithValidRequest_Returns201()
    {
        // Arrange
        var request = new
        {
            email = "test@example.com",
            displayName = "Test User"
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/users", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.Created);
        
        var userId = await response.Content.ReadFromJsonAsync<Guid>();
        userId.Should().NotBeEmpty();
    }
    
    [Fact]
    public async Task CreateUser_WithInvalidEmail_Returns400()
    {
        // Arrange
        var request = new
        {
            email = "invalid-email",
            displayName = "Test User"
        };
        
        // Act
        var response = await _client.PostAsJsonAsync("/api/v1/users", request);
        
        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
```

## Test Organization

### Directory Structure

```
tests/
├── unit/
│   ├── domain/
│   │   ├── UserTests.cs
│   │   ├── EmailTests.cs
│   │   └── DisplayNameTests.cs
│   └── application/
│       ├── CreateUserCommandHandlerTests.cs
│       └── GetUserByIdQueryHandlerTests.cs
├── integration/
│   ├── repositories/
│   │   └── UserRepositoryTests.cs
│   └── fixtures/
│       └── DatabaseFixture.cs
└── contract/
    └── UserApiContractTests.cs
```

### Naming Conventions

- **Test Class**: `{ClassUnderTest}Tests`
- **Test Method**: `{MethodUnderTest}_{Scenario}_{ExpectedResult}`

Examples:
- `User_Create_WithValidEmail_ReturnsUser`
- `User_Activate_WhenAlreadyActive_ThrowsException`
- `UserRepository_GetByIdAsync_WhenNotFound_ReturnsNull`

## Running Tests

### Command Line

```powershell
# Run all tests
dotnet test

# Run specific service tests
cd Services/admin-service
dotnet test

# Run only unit tests
dotnet test --filter Category=Unit

# Run only integration tests
dotnet test --filter Category=Integration

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run with coverage
dotnet test /p:CollectCoverage=true /p:CoverageReportsDirectory=./coverage
```

### Visual Studio Code

1. Install **C# Dev Kit** extension
2. Open Test Explorer (Testing icon in sidebar)
3. Click "Run All Tests" or run individual tests

### Continuous Integration

Tests run automatically on:
- Pull request creation
- Push to develop/main branches
- Nightly builds

## Test Data Management

### Test Builders

Use builder pattern for test data:

```csharp
public class UserBuilder
{
    private Email _email = Email.Create("test@example.com");
    private DisplayName _displayName = DisplayName.Create("Test User");
    private UserStatus _status = UserStatus.Pending;
    
    public UserBuilder WithEmail(string email)
    {
        _email = Email.Create(email);
        return this;
    }
    
    public UserBuilder WithStatus(UserStatus status)
    {
        _status = status;
        return this;
    }
    
    public User Build()
    {
        var user = User.Create(_email, _displayName);
        // Set status via reflection or internal method
        return user;
    }
}

// Usage
var user = new UserBuilder()
    .WithEmail("custom@example.com")
    .WithStatus(UserStatus.Active)
    .Build();
```

### Fixtures

Use fixtures for shared test data:

```csharp
public class UserFixtures
{
    public static User CreatePendingUser() =>
        new UserBuilder().WithStatus(UserStatus.Pending).Build();
        
    public static User CreateActiveUser() =>
        new UserBuilder().WithStatus(UserStatus.Active).Build();
}
```

## Mocking

Use **NSubstitute** for mocking:

```csharp
public class CreateUserCommandHandlerTests
{
    [Fact]
    public async Task Handle_WithValidCommand_AddsUserToRepository()
    {
        // Arrange
        var repository = Substitute.For<IUserRepository>();
        var handler = new CreateUserCommandHandler(repository);
        var command = new CreateUserCommand("test@example.com", "Test User");
        
        // Act
        var userId = await handler.Handle(command, CancellationToken.None);
        
        // Assert
        await repository.Received(1).AddAsync(Arg.Is<User>(u => 
            u.Email.Value == "test@example.com"));
    }
}
```

## Assertions

Use **FluentAssertions** for readable assertions:

```csharp
// Good: FluentAssertions
user.Should().NotBeNull();
user.Email.Value.Should().Be("test@example.com");
user.Status.Should().Be(UserStatus.Active);

// Avoid: xUnit assertions
Assert.NotNull(user);
Assert.Equal("test@example.com", user.Email.Value);
Assert.Equal(UserStatus.Active, user.Status);
```

## Code Coverage

### Targets

- **Domain Layer**: 100% coverage
- **Application Layer**: 90%+ coverage
- **Infrastructure Layer**: 70%+ coverage
- **API Layer**: 80%+ coverage
- **Overall**: 80%+ coverage

### Generating Reports

```powershell
# Generate coverage report
dotnet test /p:CollectCoverage=true /p:CoverageReportsDirectory=./coverage

# Generate HTML report (requires ReportGenerator)
dotnet tool install -g dotnet-reportgenerator-globaltool
reportgenerator -reports:./coverage/coverage.cobertura.xml -targetdir:./coverage/html
```

### Viewing Reports

Open `coverage/html/index.html` in browser to view detailed coverage report.

## Best Practices

### ✅ Do

- Write tests before or alongside code (TDD)
- Test one concept per test
- Use descriptive test names
- Follow AAA pattern (Arrange, Act, Assert)
- Keep tests simple and readable
- Use test builders for complex objects
- Clean up test data after tests
- Run tests frequently during development

### ❌ Don't

- Test implementation details
- Write tests that depend on each other
- Use production database for tests
- Commit commented-out tests
- Skip tests (fix or delete them)
- Test framework code (e.g., ASP.NET Core)
- Write slow unit tests

## Debugging Tests

### Visual Studio Code

1. Set breakpoint in test
2. Right-click test in Test Explorer
3. Select "Debug Test"
4. Step through code

### Command Line

```powershell
# Run specific test with debugging
dotnet test --filter "FullyQualifiedName~UserTests.Create_WithValidEmail"
```

## Performance Testing

For performance-critical code:

```csharp
[Fact]
public void PerformanceTest_ProcessLargeDataset()
{
    // Arrange
    var items = GenerateLargeDataset(10000);
    var stopwatch = Stopwatch.StartNew();
    
    // Act
    ProcessDataset(items);
    stopwatch.Stop();
    
    // Assert
    stopwatch.ElapsedMilliseconds.Should().BeLessThan(1000);
}
```

## Resources

- [xUnit Documentation](https://xunit.net/)
- [FluentAssertions Documentation](https://fluentassertions.com/)
- [NSubstitute Documentation](https://nsubstitute.github.io/)
- [Test-Driven Development](https://martinfowler.com/bliki/TestDrivenDevelopment.html)

---

**Remember**: Good tests are an investment in code quality and maintainability! 🧪
