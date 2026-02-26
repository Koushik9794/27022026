# Rule Service

Business Rules Engine microservice for GSS Warehouse Configurator.

## Overview

The Rule Service implements a Domain-Driven Design (DDD) architecture for managing, validating, and evaluating business rules used in warehouse configuration.

### Key Features

- **RuleSet Management**: Create and manage rule sets by product group and country
- **Rule Evaluation**: Evaluate complex rule expressions against configuration data
- **Multi-category Rules**: Support for SPATIAL, STRUCTURAL, ACCESSORY, PRICING, COMPLIANCE categories
- **Version Tracking**: Full audit trail of rule versions
- **Real-time Validation**: WebSocket support for live validation feedback
- **Powerful Expression Engine**: Integrated `DynamicExpresso` for complex formula evaluation
- **Stateful Functions**: Built-in support for `ADD_BOM`, `LOOKUP` (via Catalog Service), and `VALIDATE`
- **Context-Aware**: Tracks BOM items and violations across rule set evaluation
- **Simplified API**: Dedicated `/api/rules/evaluate` endpoint for orchestrator integration

## Architecture

### Domain-Driven Design (DDD)

```
src/
├── api/                          # Controllers & REST API endpoints
├── application/                  # Commands (CQRS) & handlers
│   ├── commands/                # Write operations
│   ├── queries/                 # Read operations
│   └── handlers/                # CQRS handlers
├── domain/                       # Core business logic
│   ├── aggregates/              # Root entities (RuleSet)
│   ├── entities/                # Domain entities (Rule, RuleCondition, RuleVersion)
│   ├── valueobjects/            # Value objects (RuleExpression, EffectivePeriod, RuleOutcome)
│   ├── services/                # Domain services
│   └── events/                  # Domain events
├── infrastructure/              # Technical concerns
│   ├── persistence/             # Repository implementations
│   └── adapters/                # Expression engine adapters
└── bootstrap/                   # Application startup (Program.cs)
```

## Getting Started

### Prerequisites

- .NET 8+
- PostgreSQL 13+ (local or Docker)
- Visual Studio 2022 or VS Code
- Docker (optional, for containerized development)

### Environment Setup

1. **Copy environment template:**
```bash
cp .env.example .env
```

2. **Update `.env` for your environment:**
```env
DB_HOST=localhost
DB_PORT=5432
DB_NAME=rule_service
DB_USER=postgres
DB_PASSWORD=postgres
CONNECTION_STRING=Server=localhost;Database=rule_service;User Id=postgres;Password=postgres;Port=5432;
ASPNETCORE_ENVIRONMENT=Development
```

3. **Important:**
   - `.env` is in `.gitignore` - never commit it
   - Use `.env.example` as a template
   - For production (Fargate), use AWS Secrets Manager

### Local Development

#### Option 1: Local PostgreSQL + .NET CLI

```bash
# Start PostgreSQL
# Windows: Use PostgreSQL installer or WSL
# macOS: brew services start postgresql@15
# Linux: sudo systemctl start postgresql

# Create database
psql -U postgres -c "CREATE DATABASE rule_service;"

# Build
dotnet build

# Run (migrations run automatically)
dotnet run

# App available at http://localhost:5000
# Swagger at http://localhost:5000/swagger
```

#### Option 2: Docker Compose (Recommended)

```bash
# Start PostgreSQL and Rule Service (runs both automatically)
docker-compose up -d

# Verify both are running
docker-compose ps

# Check logs
docker-compose logs -f rule_service

# Access the API
# Swagger UI: http://localhost:5000/swagger
# Health check: http://localhost:5000/health

# Stop both services
docker-compose down
```

**Uses `.env` file for all configuration** - runs database + service + migrations automatically

#### Option 3: Container Development with Hot Reload

```bash
# Build image
docker build -t rule-service:dev -f Dockerfile .

# Run with local source mount
docker run -d \
  -p 5000:80 \
  -e ConnectionStrings__DefaultConnection="Server=host.docker.internal;Database=rule_service;User Id=postgres;Password=postgres;" \
  --name rule_service \
  rule-service:dev

# View logs
docker logs -f rule_service
```

### Build

```bash
dotnet build
```

### Run

```bash
dotnet run --project src/bootstrap/Program.cs
```

### Tests

```bash
# Unit tests
dotnet test tests/

# Domain tests
dotnet test tests/domain/

# Application tests
dotnet test tests/application/

# Contract tests
dotnet test tests/contract/
```

## API Endpoints

### Swagger UI Documentation

After starting the service, access interactive API documentation:

```
http://localhost:5000/swagger
```

**Features:**
- Browse all endpoints
- View request/response schemas
- Try-it-out feature to test endpoints
- See example payloads

### Health Check

```bash
curl http://localhost:5000/health
```

**Response**: `200 OK` if service is healthy

### RuleSet Management

- `POST /api/v1/ruleset` - Create a new RuleSet
- `GET /api/v1/ruleset/{id}` - Get RuleSet by ID
- `POST /api/v1/ruleset/{id}/activate` - Activate RuleSet
- `POST /api/v1/ruleset/{id}/validate` - Validate RuleSet

### Rule Evaluation

- `GET /api/v1/rule-evaluation/active-rules` - Get active rules
- `POST /api/v1/rule-evaluation/evaluate` - Evaluate rules (Legacy)
- `POST /api/rules/evaluate` - **New** Orchestrator-compatible evaluation endpoint

## Stateful Rule Engine

The service now supports specialized functions within rule formulas:

- `LOOKUP(componentType, attribute, value)`: Calls the Catalog Service to find the best-fit part code.
- `ADD_BOM(partCode, quantity, category)`: Adds a physical part to the evaluation result's BOM list.
- `VALIDATE(message)`: Adds a rule violation warning/error to the evaluation result.
- `GetNum(key)` / `GetBool(key)`: Multi-source variable resolution (Input vs. Calculated).

## Domain Models

### RuleSet (Aggregate Root)

```csharp
public class RuleSet
{
    public Guid Id { get; }
    public string Name { get; }
    public Guid ProductGroupId { get; }
    public Guid CountryId { get; }
    public DateTime EffectiveFrom { get; }
    public DateTime? EffectiveTo { get; }
    public string Status { get; } // DRAFT, ACTIVE, INACTIVE, ARCHIVED
    public List<Rule> Rules { get; }
}
```

### Rule (Entity)

```csharp
public class Rule
{
    public Guid Id { get; }
    public string Name { get; }
    public string Category { get; } // SPATIAL, STRUCTURAL, ACCESSORY, PRICING, COMPLIANCE
    public int Priority { get; }
    public string Severity { get; } // ERROR, WARNING, INFO
    public List<RuleCondition> Conditions { get; }
}
```

### RuleCondition (Entity)

```csharp
public class RuleCondition
{
    public Guid Id { get; }
    public string Type { get; } // AND, OR, NOT
    public string Field { get; }
    public string Operator { get; } // EQ, NE, LT, GT, CONTAINS
    public string Value { get; } // JSON serialized
}
```

### Value Objects

- **RuleExpression**: Represents the expression to be evaluated
- **EffectivePeriod**: Date range for rule applicability
- **RuleOutcome**: Result of rule evaluation

## Configuration

### Migrations

Migrations automatically run on app startup via FluentMigrator.

**Available Migrations:**
- `M20241218001_InitialMigration` - Creates all tables, indexes, and foreign keys
- `M20241218002_SeedTestData` - Adds test data (development only)

**Test Data Included:**
- 1 RuleSet: "Standard Warehouse Rules - US"
- 2 Rules: SPATIAL constraint + PRICING rule
- 3 RuleConditions: Width, height, and quantity checks
- 2 RuleVersions: Version history
- Audit logs: Track changes

**To Run Migrations Manually:**
```bash
# Apply all pending migrations
dotnet run

# Or explicit migration (from rule-service directory)
dotnet build
dotnet run
```

**View Migration Status:**
```bash
# Using docker-compose
docker-compose logs rule_service | grep -i migration
```

### Environment Variables

Configuration is managed through environment variables and can be provided via:

1. **`.env` file** (Local development - not committed)
2. **`appsettings.json`** (Defaults)
3. **`appsettings.{Environment}.json`** (Environment-specific)
4. **Environment variables** (Docker/Fargate override)
5. **AWS Secrets Manager** (Production)

### Local Development Setup

```bash
# Copy template
cp .env.example .env

# Edit .env with your values
nano .env
```

Example `.env`:
```env
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Server=localhost;Database=rule_service;User Id=postgres;Password=postgres;
ASPNETCORE_URLS=http://+:5000
LOG_LEVEL=Information
```

### Docker Compose Setup

```bash
# docker-compose.yml reads .env automatically
docker-compose up -d
```

### Production (AWS Fargate)

For production, use AWS Secrets Manager:

```bash
# Store connection string in Secrets Manager
aws secretsmanager create-secret \
  --name rule-service-db-connection \
  --secret-string "Server=rule-service-db.xxxxx.rds.amazonaws.com;Database=rule_service;User Id=postgres;Password=SecurePassword123!;SSL Mode=Require;"
```

Task definition references it:
```json
"secrets": [
  {
    "name": "ConnectionStrings__DefaultConnection",
    "valueFrom": "arn:aws:secretsmanager:us-east-1:ACCOUNT_ID:secret:rule-service-db-connection-XXXXX"
  }
]
```

### Configuration File Override

Set the environment variable before running:

```bash
# Windows (cmd)
set ASPNETCORE_ENVIRONMENT=Production
dotnet run

# Windows (PowerShell)
$env:ASPNETCORE_ENVIRONMENT = "Production"
dotnet run

# Linux/macOS
export ASPNETCORE_ENVIRONMENT=Production
dotnet run

# Docker
docker run -e ASPNETCORE_ENVIRONMENT=Production ...
```

### Set the connection string



### Adding a New Rule Category

1. Update `RuleCondition` enum
2. Add validation rules in domain service
3. Create endpoint tests
4. Update OpenAPI spec

### Adding a New Expression Engine

1. Implement `IExpressionEngineAdapter`
2. Register in DI container
3. Add adapter tests

## OpenAPI/Swagger

OpenAPI specification is located in `openapi/v1/`:

- `index.yaml` - Root spec
- `rules.yaml` - Rules endpoints
- `common/schemas.yaml` - Shared schemas
- `common/errors.yaml` - Error definitions

## Contributing

1. Follow DDD principles
2. Write domain tests first
3. Keep business logic in domain layer
4. Use CQRS for application layer
5. Keep infrastructure concerns separate

## License

Proprietary - GSS

