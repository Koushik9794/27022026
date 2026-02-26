---
description: Schema-driven dynamic attributes pattern with UoM support
---

# Schema-Driven Dynamic Attributes Pattern

## Problem
Enterprise applications need flexible attribute handling where:
- Different entity types have different attributes (e.g., Reach Truck vs VNA Turret)
- Units of Measure (UoM) must not be hardcoded (mm, kg, m, etc.)
- Attributes should be validated against a schema from catalog-service

## Pattern

### 1. Catalog Service (Master Data) - Define Schema

```csharp
// Catalog entity has AttributeSchema defining valid attributes + UoM
public JsonDocument? AttributeSchema { get; private set; }
```

**Example AttributeSchema:**
```json
{
  "aisleWidth": { 
    "type": "number", 
    "label": "Aisle Width",
    "unit": "mm", 
    "min": 1500, 
    "max": 5000,
    "required": true
  },
  "liftHeight": { 
    "type": "number", 
    "label": "Lift Height",
    "unit": "m", 
    "min": 1, 
    "max": 18,
    "required": true
  },
  "loadCapacity": { 
    "type": "number", 
    "label": "Load Capacity",
    "unit": "kg", 
    "min": 500, 
    "max": 3000
  }
}
```

### 2. Configuration Service (Project Data) - Store Values

```csharp
// Configuration entity stores ONLY:
// - Reference to catalog type (e.g., MheTypeId)
// - Dynamic Attributes JSON (values conforming to schema)

public Guid? MheTypeId { get; private set; }       // References catalog
public JsonDocument? Attributes { get; private set; } // Values only
```

**Example Attributes (stored values):**
```json
{
  "aisleWidth": { "value": 2500, "unit": "mm" },
  "liftHeight": { "value": 12, "unit": "m" },
  "loadCapacity": { "value": 1500, "unit": "kg" }
}
```

### 3. Anti-Patterns to Avoid

❌ **Do NOT hardcode typed fields with UoM in property name:**
```csharp
// BAD - hardcoded UoM, inflexible
public decimal? AisleWidthMm { get; private set; }
public decimal? LiftHeightMm { get; private set; }
```

✅ **DO use dynamic attributes:**
```csharp
// GOOD - schema-driven, flexible, supports UoM conversion
public JsonDocument? Attributes { get; private set; }
```

## Entities Using This Pattern

| Service | Entity | Schema Source |
|---------|--------|---------------|
| catalog-service | `Mhe` | `AttributeSchema` |
| catalog-service | `ComponentType` | `AttributeSchema` |
| catalog-service | `ProductGroup` | `AttributeSchema` (add if needed) |
| configuration-service | `MheConfig` | References `Mhe.AttributeSchema` |
| configuration-service | `StorageConfiguration` | References `ProductGroup.AttributeSchema` |

## UoM Conversion (Future)

For unit conversion, consider:
1. Store canonical unit in schema
2. Allow conversion on read (display in user's preferred unit)
3. Always persist in canonical unit

## Validation Flow

```
UI → BFF → configuration-service
              │
              ├── Fetch AttributeSchema from catalog-service
              ├── Validate Attributes against schema
              └── Store valid Attributes JSON
```
