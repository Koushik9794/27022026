# Warehouse Spatial Taxonomy

> **Core Principle:** Component Taxonomy defines **WHAT exists**. Warehouse Spatial Taxonomy defines **WHERE it exists**.

## Why This Exists

A real warehouse is **not a single homogeneous space**.

In practice:
- Different areas serve different functions
- Different floors have different civil capacities
- Different zones impose different safety and regulatory constraints
- Multiple storage systems coexist within the same facility

---

## Separation of Concerns (Non-Negotiable)

| Taxonomy | Owns |
|----------|------|
| Component Taxonomy | Physical components and assemblies |
| Warehouse Spatial Taxonomy | Physical environment and civil context |

> **Invariant:** A component must never redefine location. A warehouse zone must never redefine component behavior.

Safety emerges **only when both are evaluated together**.

---

## Hierarchical Model

```
Warehouse
├── Area / Zone
│   ├── Floor / Level
│   │   ├── Storage System Instance
│   │   │   ├── Assemblies
│   │   │   │   ├── Components
```

This hierarchy mirrors how **real engineering decisions are made**.

---

## Level Definitions

### Warehouse (Facility-Level Context)

Represents the **overall facility**.

| Attribute | Description | Example |
|-----------|-------------|---------|
| Location | Geo-location | Mumbai, India |
| Design Standards | Applicable codes | IS / EN / FEM / OSHA |
| Safety Policy | Default policy | Standard |
| Currency | For pricing | INR, USD |
| Temperature Range | Environment | -5°C to 45°C |

---

### Area / Zone (Functional Partition)

Represents **functional or regulatory subdivisions**.

| Example Zones | Purpose |
|---------------|---------|
| Bulk Storage Zone | Pallet racking |
| Picking Zone | Order picking |
| Dispatch Zone | Loading/unloading |
| Fire Compartment | Fire safety |
| Cold Storage Zone | Refrigerated |

| Attribute | Description |
|-----------|-------------|
| Fire zone classification | Safety rating |
| Permitted MHE types | Allowed equipment |
| Minimum aisle widths | Operational constraint |
| Mandatory safety accessories | Required items |

> **Invariant:** Safety requirements are zone-specific, not warehouse-wide.

---

### Floor / Level (Civil Truth)

Represents **vertical structural reality**.

| Attribute | Description | Example |
|-----------|-------------|---------|
| Floor number | Level | 0 (Ground), 1 (Mezzanine) |
| Clear height | Available height | 11,000 mm |
| Slab rating | Load capacity | 50 kN/m² |
| Point load limit | Anchor capacity | 35 kN |
| Flatness tolerance | Slab quality | FM2 |
| Seismic zone | Ground motion | Zone-3 |

> **Invariant:** No structural component is validated without floor context.

---

### Storage System Instance (Product Group Deployment)

A **specific deployment** of a product group in a specific spatial context.

| Example | Description |
|---------|-------------|
| SPR in Area A, Floor 1 | Selective Pallet Racking |
| Cantilever in Area B, Floor 1 | Long goods storage |
| Shelving on Mezzanine | Pick module |

| Attribute | Description |
|-----------|-------------|
| Product group | SPR, Cantilever, ASRS |
| Associated area and floor | Location |
| Applicable rule set | Rules version |
| MHE type | Reach truck, Turret |
| Aisle width | Operational |

> This is the **unit at which rules are evaluated and BOM is generated**.

---

## Rule Evaluation Scope

> **Invariant:** Rules are **never evaluated globally**. They are evaluated at the level of Storage System Instance using Area and Floor context.

### Example
Same Upright + Beam:
- Floor 1 → Standard anchor
- Floor 2 (weaker slab) → Heavy-duty anchor
- Mezzanine → Height restriction violation

---

## Data Model

```sql
CREATE TABLE warehouses (
    id UUID PRIMARY KEY,
    name VARCHAR(255) NOT NULL,
    location VARCHAR(255),
    design_standard VARCHAR(50),
    currency VARCHAR(3),
    created_at TIMESTAMP NOT NULL
);

CREATE TABLE warehouse_areas (
    id UUID PRIMARY KEY,
    warehouse_id UUID NOT NULL REFERENCES warehouses(id),
    name VARCHAR(255) NOT NULL,
    zone_type VARCHAR(100),
    fire_zone VARCHAR(50),
    permitted_mhe_types JSONB,
    created_at TIMESTAMP NOT NULL
);

CREATE TABLE warehouse_floors (
    id UUID PRIMARY KEY,
    area_id UUID NOT NULL REFERENCES warehouse_areas(id),
    floor_number INT NOT NULL,
    clear_height_mm DECIMAL NOT NULL,
    slab_rating_kn_m2 DECIMAL,
    seismic_zone VARCHAR(20),
    created_at TIMESTAMP NOT NULL
);

CREATE TABLE storage_system_instances (
    id UUID PRIMARY KEY,
    floor_id UUID NOT NULL REFERENCES warehouse_floors(id),
    product_group VARCHAR(50) NOT NULL,
    ruleset_version VARCHAR(20),
    mhe_type VARCHAR(50),
    aisle_width_mm DECIMAL,
    created_at TIMESTAMP NOT NULL
);
```

---

## Related Documentation

- [Component Taxonomy](../03-component-taxonomy/README.md)
- [Assemblies](../05-assemblies/README.md)
- [BOM Explosion](../07-bom/README.md)
