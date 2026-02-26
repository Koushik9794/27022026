# BOM Service

Bill of Materials generation microservice for GSS Warehouse Configurator.

## Overview

The BOM Service generates and manages Bills of Materials for warehouse configurations, aggregating components, calculating costs, and producing export-ready BOM documents.

### Key Features

- **BOM Generation**: Automatically generate BOMs from warehouse configurations
- **Component Aggregation**: Aggregate SKUs, pallets, and MHE into complete BOM
- **Cost Calculation**: Calculate total costs with pricing rules
- **BOM Export**: Export BOMs in multiple formats (Excel, CSV, PDF)
- **Version Tracking**: Track BOM versions alongside configurations
- **Pricing Integration**: Integration with pricing rules and catalogs

## Architecture

### Domain-Driven Design (DDD)

```
bom-service/
├── BomService.csproj           # Project file
├── Program.cs                  # Entry point
├── README.md                   # This file
├── Dockerfile                  # Container configuration
├── docker-compose.yml          # Standalone development
├── .env.example                # Environment template
├── appsettings.json            # Configuration
├── src/
│   ├── api/                   # REST controllers
│   ├── application/           # CQRS layer
│   │   ├── commands/          # BOM operations
│   │   ├── queries/           # BOM retrieval
│   │   └── handlers/          # Command/Query handlers
│   ├── domain/                # Core business logic
│   │   ├── aggregates/        # BOM, BOMLine aggregates
│   │   ├── valueobjects/      # Quantity, Price, TotalCost
│   │   └── repositories/      # Repository interfaces
│   └── infrastructure/        # Technical concerns
│       ├── persistence/       # Repository implementations
│       └── migrations/        # Database migrations
└── tests/                     # Unit and integration tests
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
DB_NAME=bom_service
DB_USER=postgres
DB_PASSWORD=postgres
CONNECTION_STRING=Server=localhost;Database=bom_service;User Id=postgres;Password=postgres;Port=5432;
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5005

# Service Integration
CATALOG_SERVICE_URL=http://localhost:5002
CONFIGURATION_SERVICE_URL=http://localhost:5004
RULE_SERVICE_URL=http://localhost:5000
```

### Local Development

#### Option 1: Docker Compose (Recommended)

```bash
# Start PostgreSQL and BOM Service
docker-compose up -d

# Verify services are running
docker-compose ps

# Access the API
# Swagger UI: http://localhost:5005/swagger

# Stop services
docker-compose down
```

#### Option 2: Local .NET CLI

```bash
# Create database
psql -U postgres -c "CREATE DATABASE bom_service;"

# Build
dotnet build

# Run
dotnet run

# App available at http://localhost:5005
# Swagger at http://localhost:5005/swagger
```

## API Endpoints

### Swagger UI Documentation

```
http://localhost:5005/swagger
```

### Health Check

```bash
curl http://localhost:5005/health
```

### BOM Management

- `POST /api/v1/bom/generate` - Generate BOM from configuration
- `GET /api/v1/bom/{id}` - Get BOM by ID
- `PUT /api/v1/bom/{id}` - Update BOM
- `DELETE /api/v1/bom/{id}` - Delete BOM
- `GET /api/v1/bom` - List BOMs
- `POST /api/bom/batch` - **New** Batch push BOM items for a configuration

### BOM Export

- `GET /api/v1/bom/{id}/export/excel` - Export as Excel
- `GET /api/v1/bom/{id}/export/csv` - Export as CSV
- `GET /api/v1/bom/{id}/export/pdf` - Export as PDF

### Cost Calculation

- `POST /api/v1/bom/{id}/calculate-cost` - Recalculate BOM cost
- `GET /api/v1/bom/{id}/cost-breakdown` - Get detailed cost breakdown

## Domain Models

### BOM (Aggregate Root)

```csharp
public class BOM
{
    public Guid Id { get; }
    public Guid ConfigurationId { get; }
    public string Name { get; }
    public List<BOMLine> Lines { get; }
    public TotalCost TotalCost { get; }
    public DateTime GeneratedAt { get; }
    public Guid GeneratedBy { get; }
    public BOMStatus Status { get; }
}
```

### BOMLine (Entity)

```csharp
public class BOMLine
{
    public Guid Id { get; }
    public Guid ItemId { get; }
    public string ItemCode { get; }
    public string ItemName { get; }
    public ItemType Type { get; } // SKU, PALLET, MHE
    public Quantity Quantity { get; }
    public UnitPrice UnitPrice { get; }
    public LineTotal LineTotal { get; }
}
```

### Value Objects

- **Quantity**: Validated quantity with unit of measure
- **UnitPrice**: Price per unit with currency
- **TotalCost**: Calculated total cost
- **ItemType**: SKU, PALLET, MHE, ACCESSORY

## Configuration

### Environment Variables

```env
# Database
ConnectionStrings__DefaultConnection=Server=localhost;Database=bom_service;...

# Application
ASPNETCORE_ENVIRONMENT=Development
ASPNETCORE_URLS=http://+:5005

# Service Integration
CATALOG_SERVICE_URL=http://localhost:5002
CONFIGURATION_SERVICE_URL=http://localhost:5004

# BOM Settings
DEFAULT_CURRENCY=USD
INCLUDE_TAX=true
TAX_RATE=0.08
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

## Integration

### Catalog Service

BOM Service queries Catalog Service for:
- SKU details and pricing
- Pallet information
- MHE specifications

### Configuration Service

BOM Service retrieves:
- Warehouse configuration state
- Component lists
- Layout specifications

### Rule Service

BOM Service applies:
- Pricing rules
- Discount calculations
- Tax calculations

## Contributing

1. Follow DDD principles
2. Validate quantities and prices
3. Handle service integration errors
4. Write comprehensive tests
5. Document BOM calculations

## Production Readiness

Before deploying to production, complete the [Service Design Checklist](../../docs/service-design-checklist.md).

Key requirements:
- Health checks with downstream service connectivity
- Circuit breakers for catalog/configuration/rule services
- Graceful degradation when pricing unavailable
- Async BOM generation for large configurations
- Caching strategy for pricing data
- Performance testing with realistic data volumes

## Status

✅ **Implemented** - Service supports batch BOM persistence and configuration mapping.

## License

Proprietary - GSS
