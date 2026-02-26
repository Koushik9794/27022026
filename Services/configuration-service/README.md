# Configuration Service

Warehouse configuration state management microservice for GSS Warehouse Configurator.

## Overview

The Configuration Service manages warehouse configuration state, versioning, and snapshots for the warehouse design system.

### Key Features

- **Enquiry Management**: Link external CRM enquiries to warehouse configurations
- **Configuration Versioning**: Track configuration changes over time with version history
- **Per-Floor Design Layers**: Store civil layout and storage placements per floor
- **Autosave Support**: Frequent saves as designer makes changes
- **State Validation**: Validate configuration state against business rules
- **Snapshot Management**: Create and restore configuration snapshots
- **Audit Trail**: Complete history of configuration changes
- **Service Orchestration**: Coordinates layout expansion, rule evaluation, and BOM generation across distributed services

## Architecture

### Domain-Driven Design (DDD)

```
configuration-service/
├── ConfigurationService.csproj  # Project file
├── Program.cs                   # Entry point
├── README.md                    # This file
├── Dockerfile                   # Container configuration
├── docker-compose.yml           # Standalone development
├── .env.example                 # Environment template
├── appsettings.json             # Configuration
├── src/
│   ├── api/                    # REST controllers
│   ├── application/            # CQRS layer
│   │   ├── commands/           # Configuration operations
│   │   ├── queries/            # Configuration retrieval
│   │   └── handlers/           # Command/Query handlers
│   ├── domain/                 # Core business logic
│   │   ├── aggregates/         # Configuration, Snapshot aggregates
│   │   ├── valueobjects/       # ConfigurationState, Version
│   │   └── repositories/       # Repository interfaces
│   └── infrastructure/         # Technical concerns
│       ├── persistence/        # Repository implementations
│       └── migrations/         # Database migrations
└── tests/                      # Unit and integration tests
```

## Getting Started

### Prerequisites

- .NET 10+
- PostgreSQL 13+
- Docker (optional)

### Environment Setup

1. **Copy environment template:**
```bash
cp .env.example .env
```

2. **Update `.env` for your environment:**
```env
DB_HOST=localhost
DB_PORT=5432
DB_NAME=configuration_service
DB_USER=postgres
DB_PASSWORD=postgres
CONNECTION_STRING=Server=localhost;Database=configuration_service;User Id=postgres;Password=postgres;Port=5432;
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5004
```

### Local Development

#### Option 1: Docker Compose (Recommended)

```bash
# Start PostgreSQL and Configuration Service
docker-compose up -d

# Verify services are running
docker-compose ps

# Access the API
# Swagger UI: http://localhost:5004/swagger

# Stop services
docker-compose down
```

#### Option 2: Local .NET CLI

```bash
# Create database
psql -U postgres -c "CREATE DATABASE configuration_service;"

# Build
dotnet build

# Run
dotnet run

# App available at http://localhost:5004
# Swagger at http://localhost:5004/swagger
```

## API Endpoints

### Swagger UI Documentation

```
http://localhost:5004/swagger
```

### Health Check

```bash
curl http://localhost:5004/health
```

### Enquiry Management

- `POST /api/v1/enquiries` - Create new enquiry
- `GET /api/v1/enquiries/{id}` - Get enquiry by ID
- `GET /api/v1/enquiries/external/{externalId}` - Get by external CRM ID
- `PUT /api/v1/enquiries/{id}` - Update enquiry
- `DELETE /api/v1/enquiries/{id}` - Delete enquiry

### Configuration Management

- `GET /api/v1/enquiries/{enquiryId}/configurations` - List configurations
- `POST /api/v1/enquiries/{enquiryId}/configurations` - Create configuration
- `POST /api/v1/enquiries/{enquiryId}/configurations/{id}/set-primary` - Set as primary

### Version Management

- `POST /api/v1/enquiries/{enquiryId}/configurations/{configId}/versions` - Create version
- `GET /api/v1/enquiries/{enquiryId}/configurations/{configId}/versions/current` - Get current version
- `POST /api/v1/enquiries/{enquiryId}/configurations/{configId}/versions/{v}/set-current` - Set current

### Storage Configuration & Autosave

- `POST /api/v1/storage-configurations` - Create storage configuration
- `PUT /api/v1/storage-configurations/{id}/design` - **Autosave** design data

## Service Orchestration

The configuration service acts as the orchestrator for the GSS Configurator flow:

1. **Layout Expansion**: Expands a front-end layout JSON into a hierarchy of components.
2. **Rule Coordination**: For each component, it calls the **Rule Service** for structural and business validation.
3. **Implicit Catalog Lookup**: Downstream rules trigger lookups in the **Catalog Service** via the Rule Service client.
4. **BOM Propagation**: Aggregates all BOM items from rules and pushes them to the **BOM Service** in a single batch.

## Domain Models

### Entity Hierarchy

```
Enquiry (Aggregate Root)
└── Configuration (design variant: "Option A", "Option B")
    └── ConfigurationVersion (v1, v2, v3...)
        ├── ConfigurationSku (customer SKUs)
        ├── ConfigurationPallet (customer pallets)
        ├── WarehouseConfig (building dimensions, constraints)
        ├── MheConfig (Material Handling Equipment)
        └── StorageConfiguration (per-floor design layer)
              ├── FloorId → links to warehouse floor
              ├── DesignData → JSON with civil layout + product groups
              └── LastSavedAt → autosave tracking
```

### StorageConfiguration (Key Entity)

```csharp
public class StorageConfiguration
{
    public Guid Id { get; }
    public Guid ConfigurationVersionId { get; }
    public Guid? FloorId { get; }           // Links to warehouse floor
    public string Name { get; }
    public string ProductGroup { get; }     // SPR, Cantilever, Shelving
    public JsonDocument? DesignData { get; } // Civil layout + storage placements
    public DateTime? LastSavedAt { get; }    // Autosave tracking
}
```

### Value Objects

- **DesignData**: JSON containing civil layout + constraints + product group placements
- **ConfigurationStatus**: DRAFT, ACTIVE, ARCHIVED

## Configuration

### Environment Variables

```env
# Database
ConnectionStrings__DefaultConnection=Server=localhost;Database=configuration_service;...

# Application
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5004

# Snapshot Settings
MAX_SNAPSHOTS_PER_CONFIGURATION=50
SNAPSHOT_RETENTION_DAYS=90
```

## Testing

```bash
# Run all tests
dotnet test

# Run unit tests
dotnet test --filter Category=Unit

# Run integration tests
dotnet test --filter Category=Integration
```

## Contributing

1. Follow DDD principles
2. Validate configuration state
3. Ensure version consistency
4. Write comprehensive tests
5. Document state transitions

## Production Readiness

Before deploying to production, complete the [Service Design Checklist](../../docs/service-design-checklist.md).

Key requirements:
- Health checks with database connectivity
- Idempotent snapshot operations
- Optimistic concurrency control for updates
- Audit trail for all configuration changes
- Backup and restore procedures
- Performance testing for large configurations

## Status

## Status

✅ **Implemented Orchestration** - Core flow from layout expansion to BOM generation is operational.

## License

Proprietary - GSS
