# Catalog Service Scope

## What the Catalog Service Manages

```mermaid
flowchart TD
    subgraph Catalog["Catalog Service"]
        SKU[SKU Aggregate]
        Pallet[Pallet Aggregate]
        MHE[MHE Aggregate]
        Palette[UI Palette]
    end
    
    subgraph Consumers
        BFF[BFF/Web API]
        Rules[Rule Service]
        Config[Configuration Service]
    end
    
    BFF --> Catalog
    Rules --> Catalog
    Config --> Catalog
```

## Aggregate Boundaries

| Aggregate | Responsibility | Key Attributes |
|-----------|----------------|----------------|
| **SKU** | Stock keeping unit definition | Code, Name, GLB File, Status |
| **Pallet** | Pallet type definition | Code, Name, Type, Dimensions |
| **MHE** | Material handling equipment | Code, Name, Equipment Type |
| **Palette** | UI component configuration | Groups, Categories, Roles |

## Integration Scope

### Provides Data To

- **BFF** — Palette configuration, component lookups
- **Rule Service** — Component attributes, load charts
- **Configuration Service** — Component references for configurations

### Receives Data From

- **Admin Portal** — CRUD operations on catalog items
- **Import Jobs** — Bulk catalog updates

## Feature Matrix

| Feature | SKU | Pallet | MHE | Palette |
|---------|-----|--------|-----|---------|
| CRUD Operations | ✅ | ⚠️ | ⚠️ | ❌ |
| Soft Delete | ✅ | ⚠️ | ⚠️ | N/A |
| 3D Model Reference | ✅ | ❌ | ❌ | ❌ |
| Status Toggle | ✅ | ⚠️ | ⚠️ | N/A |
| Role Filtering | ❌ | ❌ | ❌ | ✅ |

✅ Implemented | ⚠️ Domain Model Only | ❌ Not Applicable
