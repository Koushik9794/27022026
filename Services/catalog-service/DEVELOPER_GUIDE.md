# Catalog Service - Developer Guide

## Quick Start

### Prerequisites
- .NET 10+
- PostgreSQL (via Docker)
- Visual Studio Code

### Installing .NET 10 (Linux/WSL)
```bash
wget https://dot.net/v1/dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0
export PATH="$HOME/.dotnet:$PATH"
```

### Running Locally
```bash
# 1. Start PostgreSQL
cd gss-backend
docker compose up postgres -d

# 2. Run the service
cd Services/catalog-service
dotnet run
```

**Swagger UI:**
| Environment | URL |
|-------------|-----|
| Local (`dotnet run`) | http://localhost:60188/swagger |
| Docker | http://localhost:5002/swagger |

---

## Step-by-Step: Adding a New API Endpoint

This guide walks you through adding a complete new feature: a `GET /api/v1/taxonomy/interfaces` endpoint.

### Step 1: Create the Domain Entity

Create `src/domain/aggregates/ComponentInterface.cs`:

```csharp
namespace CatalogService.Domain.Aggregates;

public class ComponentInterface
{
    public Guid Id { get; private set; }
    public string Code { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string? Description { get; private set; }
    public bool IsActive { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private ComponentInterface() { } // For ORM

    public static ComponentInterface Create(string code, string name, string? description = null)
    {
        return new ComponentInterface
        {
            Id = Guid.NewGuid(),
            Code = code,
            Name = name,
            Description = description,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };
    }
}
```

### Step 2: Create the Repository Interface

Create `src/infrastructure/persistence/repositories/IComponentInterfaceRepository.cs`:

```csharp
namespace CatalogService.Infrastructure.Persistence.Repositories;

public interface IComponentInterfaceRepository
{
    Task<IEnumerable<ComponentInterface>> GetAllAsync();
    Task<ComponentInterface?> GetByIdAsync(Guid id);
    Task<Guid> CreateAsync(ComponentInterface entity);
}
```

### Step 3: Implement the Repository

Create `src/infrastructure/persistence/repositories/ComponentInterfaceRepository.cs`:

```csharp
using Dapper;
using CatalogService.Domain.Aggregates;

namespace CatalogService.Infrastructure.Persistence.Repositories;

public class ComponentInterfaceRepository : IComponentInterfaceRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ComponentInterfaceRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ComponentInterface>> GetAllAsync()
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM component_interfaces WHERE is_active = true";
        return await connection.QueryAsync<ComponentInterface>(sql);
    }

    public async Task<ComponentInterface?> GetByIdAsync(Guid id)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = "SELECT * FROM component_interfaces WHERE id = @Id";
        return await connection.QueryFirstOrDefaultAsync<ComponentInterface>(sql, new { Id = id });
    }

    public async Task<Guid> CreateAsync(ComponentInterface entity)
    {
        using var connection = _connectionFactory.CreateConnection();
        const string sql = @"
            INSERT INTO component_interfaces (id, code, name, description, is_active, created_at)
            VALUES (@Id, @Code, @Name, @Description, @IsActive, @CreatedAt)";
        await connection.ExecuteAsync(sql, entity);
        return entity.Id;
    }
}
```

### Step 4: Create the DTO

Create `src/application/dtos/ComponentInterfaceDto.cs`:

```csharp
namespace CatalogService.Application.Dtos;

public record ComponentInterfaceDto(
    Guid Id,
    string Code,
    string Name,
    string? Description,
    bool IsActive,
    DateTime CreatedAt
);
```

### Step 5: Create the Query

Create `src/application/queries/interfaces/InterfaceQueries.cs`:

```csharp
namespace CatalogService.Application.Queries.Interfaces;

public record GetAllInterfacesQuery();
public record GetInterfaceByIdQuery(Guid Id);
```

### Step 6: Create the Query Handler

Create `src/application/handlers/interfaces/InterfaceQueryHandlers.cs`:

```csharp
using CatalogService.Application.Dtos;
using CatalogService.Application.Queries.Interfaces;
using CatalogService.Infrastructure.Persistence.Repositories;

namespace CatalogService.Application.Handlers.Interfaces;

public class InterfaceQueryHandlers
{
    private readonly IComponentInterfaceRepository _repository;

    public InterfaceQueryHandlers(IComponentInterfaceRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<ComponentInterfaceDto>> Handle(GetAllInterfacesQuery query)
    {
        var interfaces = await _repository.GetAllAsync();
        return interfaces.Select(i => new ComponentInterfaceDto(
            i.Id, i.Code, i.Name, i.Description, i.IsActive, i.CreatedAt
        )).ToList();
    }

    public async Task<ComponentInterfaceDto?> Handle(GetInterfaceByIdQuery query)
    {
        var entity = await _repository.GetByIdAsync(query.Id);
        if (entity == null) return null;
        return new ComponentInterfaceDto(
            entity.Id, entity.Code, entity.Name, entity.Description, 
            entity.IsActive, entity.CreatedAt
        );
    }
}
```

### Step 7: Create the Controller

Add to `src/api/controllers/TaxonomyController.cs` (or create new controller):

```csharp
/// <summary>
/// Get all component interfaces
/// </summary>
[HttpGet("interfaces")]
[ProducesResponseType(typeof(List<ComponentInterfaceDto>), StatusCodes.Status200OK)]
public async Task<IActionResult> GetAllInterfaces()
{
    var result = await _mediator.InvokeAsync<List<ComponentInterfaceDto>>(
        new GetAllInterfacesQuery());
    return Ok(result);
}

/// <summary>
/// Get interface by ID
/// </summary>
[HttpGet("interfaces/{id:guid}")]
[ProducesResponseType(typeof(ComponentInterfaceDto), StatusCodes.Status200OK)]
[ProducesResponseType(StatusCodes.Status404NotFound)]
public async Task<IActionResult> GetInterfaceById(Guid id)
{
    var result = await _mediator.InvokeAsync<ComponentInterfaceDto?>(
        new GetInterfaceByIdQuery(id));
    if (result == null) return NotFound();
    return Ok(result);
}
```

### Step 8: Register the Repository

Add to `Program.cs`:

```csharp
builder.Services.AddScoped<IComponentInterfaceRepository, ComponentInterfaceRepository>();
```

### Step 9: Add Database Migration

Create `src/infrastructure/migrations/M20260111001_CreateComponentInterfaces.cs`:

```csharp
using FluentMigrator;

namespace CatalogService.Infrastructure.Migrations;

[Migration(20260111001)]
public class M20260111001_CreateComponentInterfaces : Migration
{
    public override void Up()
    {
        Create.Table("component_interfaces")
            .WithColumn("id").AsGuid().PrimaryKey()
            .WithColumn("code").AsString(50).NotNullable().Unique()
            .WithColumn("name").AsString(255).NotNullable()
            .WithColumn("description").AsString(1000).Nullable()
            .WithColumn("is_active").AsBoolean().NotNullable().WithDefaultValue(true)
            .WithColumn("created_at").AsDateTime().NotNullable();
    }

    public override void Down()
    {
        Delete.Table("component_interfaces");
    }
}
```

### Step 10: Write Unit Tests

Create `tests/unit/InterfaceQueryHandlerTests.cs`:

```csharp
using Moq;
using Xunit;
using CatalogService.Application.Handlers.Interfaces;
using CatalogService.Application.Queries.Interfaces;
using CatalogService.Domain.Aggregates;
using CatalogService.Infrastructure.Persistence.Repositories;

namespace CatalogService.Tests.Unit;

public class InterfaceQueryHandlerTests
{
    private readonly Mock<IComponentInterfaceRepository> _mockRepository;
    private readonly InterfaceQueryHandlers _handler;

    public InterfaceQueryHandlerTests()
    {
        _mockRepository = new Mock<IComponentInterfaceRepository>();
        _handler = new InterfaceQueryHandlers(_mockRepository.Object);
    }

    [Fact]
    public async Task Handle_GetAllInterfaces_ReturnsAllInterfaces()
    {
        // Arrange
        var interfaces = new List<ComponentInterface>
        {
            ComponentInterface.Create("HOOK", "Hook Interface"),
            ComponentInterface.Create("SLOT", "Slot Interface")
        };
        _mockRepository.Setup(r => r.GetAllAsync()).ReturnsAsync(interfaces);

        // Act
        var result = await _handler.Handle(new GetAllInterfacesQuery());

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Contains(result, i => i.Code == "HOOK");
        Assert.Contains(result, i => i.Code == "SLOT");
    }

    [Fact]
    public async Task Handle_GetInterfaceById_ReturnsInterface_WhenExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        var entity = ComponentInterface.Create("HOOK", "Hook Interface");
        _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync(entity);

        // Act
        var result = await _handler.Handle(new GetInterfaceByIdQuery(id));

        // Assert
        Assert.NotNull(result);
        Assert.Equal("HOOK", result.Code);
    }

    [Fact]
    public async Task Handle_GetInterfaceById_ReturnsNull_WhenNotExists()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockRepository.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((ComponentInterface?)null);

        // Act
        var result = await _handler.Handle(new GetInterfaceByIdQuery(id));

        // Assert
        Assert.Null(result);
    }
}
```

### Step 11: Run Tests

```bash
dotnet test
```

### Step 12: Test the Endpoint

```bash
# Start the service
dotnet run

# Test the endpoint
curl http://localhost:60188/api/v1/taxonomy/interfaces
```

---

## Summary Checklist

When adding a new API endpoint:

- [ ] Domain entity (`src/domain/aggregates/`)
- [ ] Repository interface (`src/infrastructure/persistence/repositories/I*.cs`)
- [ ] Repository implementation (`src/infrastructure/persistence/repositories/*.cs`)
- [ ] DTO (`src/application/dtos/`)
- [ ] Query/Command records (`src/application/queries/` or `commands/`)
- [ ] Handler (`src/application/handlers/`)
- [ ] Controller endpoint (`src/api/controllers/`)
- [ ] Register in `Program.cs`
- [ ] Database migration (`src/infrastructure/migrations/`)
- [ ] Unit tests (`tests/unit/`)

---

## Common Commands

| Task | Command |
|------|---------|
| Build | `dotnet build` |
| Run | `dotnet run` |
| Test | `dotnet test` |
| Start DB | `docker compose up postgres -d` |
