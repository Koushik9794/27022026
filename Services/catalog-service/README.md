# Catalog Service

Catalog management microservice for GSS Warehouse Configurator.

## Overview

Provides master data management for warehouse racking components, customer SKU types, and equipment. Built with Domain-Driven Design (DDD) and CQRS patterns.

## API Endpoints

### Taxonomy (Component Classification)

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/v1/taxonomy/categories` | List all component categories |
| GET | `/api/v1/taxonomy/categories/{id}` | Get category by ID |
| POST | `/api/v1/taxonomy/categories` | Create category |
| PUT | `/api/v1/taxonomy/categories/{id}` | Update category |
| DELETE | `/api/v1/taxonomy/categories/{id}` | Delete category |
| GET | `/api/v1/taxonomy/types` | List all component types |
| GET | `/api/v1/taxonomy/types/{id}` | Get type by ID |
| POST | `/api/v1/taxonomy/types` | Create type |
| PUT | `/api/v1/taxonomy/types/{id}` | Update type |
| DELETE | `/api/v1/taxonomy/types/{id}` | Delete type |
| GET | `/api/v1/taxonomy/product-groups` | List all product groups |
| POST | `/api/v1/taxonomy/product-groups` | Create product group |

### SKU Types (Customer Goods)

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/v1/sku-types` | List all SKU types |
| GET | `/api/v1/sku-types/{id}` | Get SKU type by ID |
| POST | `/api/v1/sku-types` | Create SKU type |
| PUT | `/api/v1/sku-types/{id}` | Update SKU type |
| DELETE | `/api/v1/sku-types/{id}` | Delete SKU type |

### Pallet Types

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/v1/pallet-types` | List all pallet types |
| GET | `/api/v1/pallet-types/{id}` | Get pallet type by ID |
| GET | `/api/v1/pallet-types/code/{code}` | Get pallet type by code |
| POST | `/api/v1/pallet-types` | Create pallet type |
| PUT | `/api/v1/pallet-types/{id}` | Update pallet type |
| DELETE | `/api/v1/pallet-types/{id}` | Delete pallet type |

### Palette (UI Configuration)

| Method | Route | Description |
|--------|-------|-------------|
| GET | `/api/v1/palette` | Get DesignDeck UI component palette |

### Catalog Lookup (Orchestration)

| Method | Route | Description |
|--------|-------|-------------|
| POST | `/api/catalog-lookup/lookup` | Perform "Smart LOOKUP" for a specific component and attribute |

## Smart LOOKUP Heuristics

The catalog service implements a heuristic-based part lookup ported from the GSS Configurator POC:

- **Type Matching**: Maps logical types (e.g., "UPRIGHT") to physical part codes and descriptions.
- **Attribute Resolution**: Heuristically extracts attribute values (e.g., Height, Span) from part metadata or descriptions.
- **Best Fit Matching**: Returns the smallest available part that satisfies the target value (with a 0.1mm tolerance).
- **Specialized Logic**: Includes specialized checks for "HeavyDuty" (thickness >= 8.0mm) and other structural constraints.

## Domain Model

### Taxonomy Hierarchy

```
ComponentCategory → ComponentType → ProductGroup
     FRAMES       →    UPRIGHT    →    SPR, VNA
   HORIZONTAL     →     BEAM      →    SPR, VNA
```

### Entities

| Entity | Purpose | Key Fields |
|--------|---------|------------|
| **ComponentCategory** | Component classification | Code, Name, Description |
| **ComponentType** | Type within category | Code, Name, CategoryId, AttributeSchema |
| **ProductGroup** | Product family | Code, Name, Description |
| **SkuType** | Customer goods type | Code, Name, Description, AttributeSchema |
| **PalletType** | Pallet standard | Code, Name, Description, AttributeSchema |
| **MHE** | Material handling equipment | Code, Name, EquipmentType |

### AttributeSchema

Flexible JSON schema defining what attributes each type requires:

```json
{
  "width": { "type": "number", "unit": "mm", "required": true },
  "depth": { "type": "number", "unit": "mm", "required": true },
  "maxLoadCapacity": { "type": "number", "unit": "kg" }
}
```

## Quick Start

### Prerequisites

- .NET 10+
- PostgreSQL (via Docker or local)

### Port Reference

| Environment | Catalog Service | PostgreSQL |
|-------------|-----------------|------------|
| Local (`dotnet run`) | 60188 | 5432 |
| Docker | 5002 | 5432 |

### Run Locally

```bash
# Start PostgreSQL
cd gss-backend
docker compose up postgres -d

# Run service
cd Services/catalog-service
dotnet run

# Access Swagger
# http://localhost:60188/swagger
```

### Run with Docker

```bash
cd gss-backend

# Start catalog service and dependencies
docker compose up catalog-service -d

# Access Swagger
# http://localhost:5002/swagger
```

## Project Structure

```
catalog-service/
├── src/
│   ├── api/controllers/           # REST API controllers
│   ├── application/
│   │   ├── commands/              # Write operations
│   │   ├── queries/               # Read operations
│   │   ├── handlers/              # Command/Query handlers
│   │   └── dtos/                  # Data transfer objects
│   ├── domain/
│   │   ├── aggregates/            # Domain entities
│   │   ├── enums/                 # Domain enumerations
│   │   └── valueobjects/          # Value objects
│   └── infrastructure/
│       ├── persistence/           # Repositories (Dapper)
│       └── migrations/            # FluentMigrator
├── docs/                          # Service documentation
└── tests/                         # Unit tests
```

## Technology Stack

| Technology | Purpose |
|------------|---------|
| .NET 10 | Runtime |
| PostgreSQL | Database with JSONB support |
| Dapper | Micro-ORM |
| WolverineFx | CQRS messaging |
| FluentMigrator | Database migrations |
| Swashbuckle | OpenAPI/Swagger |

## BFF Integration

The catalog-service is accessed via `gss-web-api` BFF:

| BFF Route | Catalog Service Route |
|-----------|----------------------|
| `/api/admin/sku-types` | `/api/v1/sku-types` |
| `/api/admin/pallet-types` | `/api/v1/pallet-types` |
| `/api/admin/taxonomy/*` | `/api/v1/taxonomy/*` |

## Migrations

Migrations run automatically on startup. Available migrations:

| Migration | Description |
|-----------|-------------|
| M20260107001 | Initial schema (skus, pallets, mhes) |
| M20260110001 | Taxonomy tables (component_categories, component_types, product_groups) |
| M20260110002 | Seed taxonomy data |
| M20260111001 | Add SKU description and attribute_schema |
| M20260111002 | Add Pallet description and attribute_schema |

## Contributing

1. Follow DDD principles
2. Keep business logic in domain layer
3. Use CQRS for application layer
4. Write unit tests
5. Update this README when adding endpoints

See [DEVELOPER_GUIDE.md](DEVELOPER_GUIDE.md) for detailed development workflow.

## License

Proprietary - GSS
