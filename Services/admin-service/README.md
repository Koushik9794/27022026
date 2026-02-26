# Admin Service

User and Admin Management microservice for GSS Warehouse Configurator.

## Overview

The Admin Service implements a Domain-Driven Design (DDD) architecture for managing users, roles, and authentication in the warehouse configuration system.

### Key Features

- **User Management**: Register, activate, update, and deactivate users
- **Role-Based Access**: SUPER_ADMIN, ADMIN, DEALER, DESIGNER, VIEWER
- **Rich Domain Model**: Value objects, aggregates, domain events
- **CQRS Pattern**: Separate commands and queries
- **Validation**: FluentValidation with MediatR pipeline
- **Database Migrations**: FluentMigrator for schema versioning
- **API Documentation**: Swagger/OpenAPI with detailed annotations

## Architecture

### Domain-Driven Design (DDD)

```
src/
├── api/                          # Controllers & REST API endpoints
├── application/                  # Commands (CQRS) & handlers
│   ├── commands/                # Write operations
│   ├── queries/                 # Read operations
│   ├── handlers/                # CQRS handlers
│   └── validators/              # FluentValidation validators
├── domain/                       # Core business logic
│   ├── aggregates/              # Root entities (User)
│   ├── entities/                # Domain entities
│   ├── valueobjects/            # Value objects (Email, DisplayName, UserRole)
│   ├── services/                # Domain services
│   └── events/                  # Domain events
├── infrastructure/              # Technical concerns
│   ├── persistence/             # Repository implementations (Dapper)
│   ├── migrations/              # FluentMigrator migrations
│   └── dapper/                  # DB Connection Factory
└── bootstrap/                   # Application startup (Program.cs)
```

## Getting Started

### Prerequisites

- .NET 10+
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
DB_NAME=admin_service
DB_USER=postgres
DB_PASSWORD=postgres
CONNECTION_STRING=Server=localhost;Database=admin_service;User Id=postgres;Password=postgres;Port=5432;
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5001
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
psql -U postgres -c "CREATE DATABASE admin_service;"

# Build
dotnet build

# Run (migrations run automatically)
dotnet run --project src/bootstrap/Program.cs

# App available at http://localhost:5001
# Swagger at http://localhost:5001/swagger
```

#### Option 2: Docker Compose (Recommended)

```bash
# Start PostgreSQL and Admin Service (runs both automatically)
docker-compose up -d

# Verify both are running
docker-compose ps

# Check logs
docker-compose logs -f admin_service

# Access the API
# Swagger UI: http://localhost:5001/swagger
# Health check: http://localhost:5001/health

# Stop both services
docker-compose down
```

**Uses `.env` file for all configuration** - runs database + service + migrations automatically

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
http://localhost:5001/swagger
```

**Features:**
- Browse all endpoints
- View request/response schemas
- Try-it-out feature to test endpoints
- See example payloads
- XML documentation comments

### Health Check

```bash
curl http://localhost:5001/health
```

**Response**: `200 OK` with service status

### User Management

- `POST /api/v1/users` - Register a new user
- `GET /api/v1/users/{id}` - Get user by ID
- `GET /api/v1/users` - Get all users
- `POST /api/v1/users/{id}/activate` - Activate user
- `PUT /api/v1/users/{id}` - Update user profile
- `DELETE /api/v1/users/{id}` - Deactivate user

## Domain Models

### User (Aggregate Root)

```csharp
public class User
{
    public Guid Id { get; }
    public Email Email { get; }              // Value Object
    public DisplayName DisplayName { get; }  // Value Object
    public UserRole Role { get; }            // Value Object
    public UserStatus Status { get; }        // Enum
    public DateTime CreatedAt { get; }
    public DateTime? LastLoginAt { get; }
}
```

### Value Objects

- **Email**: Email validation with regex
- **DisplayName**: Length validation (2-100 chars)
- **UserRole**: Predefined roles (SUPER_ADMIN, ADMIN, DEALER, DESIGNER, VIEWER)

## Configuration

### Migrations

Migrations automatically run on app startup via FluentMigrator.

**Available Migrations:**
- `M20260107001_InitialMigration` - Creates users table with indexes

**To Run Migrations Manually:**
```bash
# Apply all pending migrations
dotnet run

# View migration status
docker-compose logs admin_service | grep -i migration
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
  --name admin-service-db-connection \
  --secret-string "Server=admin-service-db.xxxxx.rds.amazonaws.com;Database=admin_service;User Id=postgres;Password=SecurePassword123!;SSL Mode=Require;"
```

Task definition references it:
```json
"secrets": [
  {
    "name": "ConnectionStrings__DefaultConnection",
    "valueFrom": "arn:aws:secretsmanager:us-east-1:ACCOUNT_ID:secret:admin-service-db-connection-XXXXX"
  }
]
```

## Swagger/OpenAPI Features

### Automatic Documentation

- **XML Comments**: All controllers and endpoints documented
- **Request/Response Schemas**: Auto-generated from C# models
- **Example Values**: Provided for all DTOs
- **HTTP Status Codes**: Documented with `[ProducesResponseType]`

### Swagger UI Features

- **Interactive Testing**: Try endpoints directly from browser
- **Authentication**: JWT bearer token support (when implemented)
- **Schema Validation**: Real-time validation of request bodies
- **Export**: Download OpenAPI spec as JSON/YAML

### Accessing Swagger

```
Development: http://localhost:5001/swagger
Production: https://api.gss.com/admin/swagger (if enabled)
```

## Contributing

1. Follow DDD principles
2. Write domain tests first
3. Keep business logic in domain layer
4. Use CQRS for application layer
5. Keep infrastructure concerns separate

## License

Proprietary - GSS
