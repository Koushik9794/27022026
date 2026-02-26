# Domain Aggregates

## Overview

The Configuration Service uses a hierarchical domain model following DDD principles.

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
```

### Aggregate Summary

| Entity | Purpose | Key Invariants |
|--------|---------|----------------|
| **Enquiry** | Root entity, links to CRM | ExternalEnquiryId unique |
| **Configuration** | Design variant | One can be primary per enquiry |
| **ConfigurationVersion** | Versioned snapshot | Version number unique per config |
| **ConfigurationSku** | Customer product specs | Code unique per version |
| **ConfigurationPallet** | Customer pallet specs | Code unique per version |
| **WarehouseConfig** | Building context | Required dimensions |
| **MheConfig** | MHE specification | References catalog MheType |
| **StorageConfiguration** | Per-floor design | FloorId + DesignData JSON |

---

## Enquiry (Aggregate Root)

```csharp
public class Enquiry
{
    public Guid Id { get; }
    public string ExternalEnquiryId { get; }  // From external CRM
    public string Name { get; }
    public string? Description { get; }
    public EnquiryStatus Status { get; }      // Draft, InProgress, Submitted, Converted, Archived
    public int Version { get; }
    public IReadOnlyCollection<Configuration> Configurations { get; }
}
```

### Status Transitions

```
Draft → InProgress → Submitted → Converted → Archived
                  ↓             ↓
              Archived      Archived
```

---

## Configuration

Represents a design variant within an enquiry (e.g., "Standard Layout", "High-Density Layout").

```csharp
public class Configuration
{
    public Guid Id { get; }
    public Guid EnquiryId { get; }
    public string Name { get; }
    public bool IsPrimary { get; }
    public IReadOnlyCollection<ConfigurationVersion> Versions { get; }
}
```

---

## ConfigurationVersion

A versioned snapshot containing all child entities for that version.

```csharp
public class ConfigurationVersion
{
    public Guid Id { get; }
    public Guid ConfigurationId { get; }
    public int VersionNumber { get; }
    public bool IsCurrent { get; }
    
    // Child collections
    public IReadOnlyCollection<ConfigurationSku> Skus { get; }
    public IReadOnlyCollection<ConfigurationPallet> Pallets { get; }
    public IReadOnlyCollection<WarehouseConfig> WarehouseConfigs { get; }
    public IReadOnlyCollection<MheConfig> MheConfigs { get; }
    public IReadOnlyCollection<StorageConfiguration> StorageConfigurations { get; }
}
```

---

## StorageConfiguration

Per-floor design layer containing civil layout and product group placements.

```csharp
public class StorageConfiguration
{
    public Guid Id { get; }
    public Guid ConfigurationVersionId { get; }
    public Guid? FloorId { get; }           // Links to warehouse floor spatial data
    public string Name { get; }
    public string ProductGroup { get; }     // SPR, Cantilever, Shelving, ASRS
    public JsonDocument? DesignData { get; } // Civil layout + constraints + storage placements
    public DateTime? LastSavedAt { get; }    // Autosave tracking
}
```

### DesignData JSON Structure

Contains merged civil layout and storage placements:

```json
{
  "shapes": [
    { "type": "rectangle", "x": 100, "y": 50, ... },
    { "type": "door", "x": 200, "y": 100, ... },
    { "type": "rack", "name": "SPR", "x": 150, "y": 70, ... }
  ],
  "scale": 0.01,
  "unit": "meters",
  "version": "1.0"
}
```

---

## MheConfig

Material Handling Equipment configuration.

```csharp
public class MheConfig
{
    public Guid Id { get; }
    public Guid ConfigurationVersionId { get; }
    public Guid? MheTypeId { get; }         // References catalog-service
    public string Name { get; }
    public decimal? AisleWidthMm { get; }
    public decimal? LiftHeightMm { get; }
    public decimal? LoadCapacityKg { get; }
    public JsonDocument? Attributes { get; }
}
```

---

## WarehouseConfig

Building context and constraints.

```csharp
public class WarehouseConfig
{
    public Guid Id { get; }
    public Guid ConfigurationVersionId { get; }
    public string Name { get; }
    public decimal LengthM { get; }
    public decimal WidthM { get; }
    public decimal ClearHeightM { get; }
    public string? FloorType { get; }
    public decimal? FloorLoadCapacityKnM2 { get; }
    public string? SeismicZone { get; }
}
```

---

## ConfigurationSku

Customer's specific product.

```csharp
public class ConfigurationSku
{
    public Guid Id { get; }
    public Guid ConfigurationVersionId { get; }
    public Guid? SkuTypeId { get; }  // References catalog-service
    public string Code { get; }
    public string Name { get; }
    public int? UnitsPerLayer { get; }
    public int? LayersPerPallet { get; }
    public JsonDocument? Attributes { get; }
}
```

---

## ConfigurationPallet

Customer's pallet specification.

```csharp
public class ConfigurationPallet
{
    public Guid Id { get; }
    public Guid ConfigurationVersionId { get; }
    public Guid? PalletTypeId { get; }  // References catalog-service
    public string Code { get; }
    public string Name { get; }
    public JsonDocument? Attributes { get; }
}
```
