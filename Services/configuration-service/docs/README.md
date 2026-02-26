# Configuration Service Documentation

This directory contains comprehensive documentation for the Configuration Service.

## Quick Links

| Section | Purpose | Start Here |
|---------|---------|------------|
| [00-overview](./00-overview/) | Problem, goals, scope | [problem-statement.md](./00-overview/problem-statement.md) |
| [01-domain](./01-domain/) | Domain model, DDD concepts | [aggregates.md](./01-domain/aggregates.md) |
| [02-architecture](./02-architecture/) | System design | Coming soon |
| [03-api](./03-api/) | API endpoints | Coming soon |
| [04-data-model](./04-data-model/) | Database schema | Coming soon |

---

## Core Concepts

### What Configuration Service Manages

| Entity | Purpose |
|--------|---------|
| **Enquiry** | Links external CRM enquiry to warehouse configurations |
| **Configuration** | Design variant (e.g., "Option A", "Option B") |
| **ConfigurationVersion** | Versioned snapshot (v1, v2, v3) |
| **ConfigurationSku** | Customer's specific products with dimensions/weights |
| **ConfigurationPallet** | Customer's pallet specifications |
| **WarehouseConfig** | Building context (dimensions, constraints) |
| **MheConfig** | Material Handling Equipment configuration |
| **StorageConfiguration** | Per-floor design layer with civil layout + product groups |

### Entity Hierarchy

```
Enquiry (Aggregate Root)
└── Configuration (design variant)
    └── ConfigurationVersion
        ├── ConfigurationSku
        ├── ConfigurationPallet
        ├── WarehouseConfig
        ├── MheConfig
        └── StorageConfiguration
              ├── FloorId → links to warehouse floor
              ├── DesignData → JSON (civil layout + constraints + storage placements)
              └── LastSavedAt → autosave tracking
```

### Relationship with Catalog Service

```
catalog-service (Master Data)      configuration-service (Project Data)
─────────────────────────────      ───────────────────────────────────
SkuType (BOX, DRUM)         ←──── ConfigurationSku (references SkuType)
PalletType (EURO, US)       ←──── ConfigurationPallet (references PalletType)
ProductGroup (SPR, ASRS)    ←──── StorageConfiguration (uses ProductGroup)
MheType (Reach Truck, VNA)  ←──── MheConfig (references MheType)
```

### Key API Endpoints

| Endpoint | Purpose |
|----------|---------|
| `PUT /api/v1/storage-configurations/{id}/design` | **Autosave** - Updates design data |
| `POST /api/v1/storage-configurations` | Create storage configuration |
| `POST /api/v1/enquiries` | Create enquiry |
| `POST /api/v1/enquiries/{id}/configurations` | Create configuration |

---

## For New Team Members

1. Start with [00-overview/problem-statement.md](./00-overview/problem-statement.md)
2. Understand [01-domain/aggregates.md](./01-domain/aggregates.md)
3. Review the main [README.md](../README.md) for API details
