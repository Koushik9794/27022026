# Component Taxonomy

> **Core Principle:** "Physical Truth First" — Any configuration that is **physically unsafe or impossible** must be **blocked by design**, even if it looks logically valid.

## What Is the Component Taxonomy?

The Component Taxonomy defines **physical component types**, independent of product group or SKU.

It answers the question: **"WHAT is it?"**

---

## Abstraction Levels

The system models warehouse structures using **four abstraction levels**:

```
Level 1: Component Taxonomy (WHAT it is)
    ↓
Level 2: Physical Interfaces (HOW it connects)
    ↓
Level 3: Assembly Context (HOW it behaves in structure)
    ↓
Level 4: System State & Context (WHEN rules apply)
```

---

## Level 1 — Structural Component Types

| Component Type | Category | Description |
|----------------|----------|-------------|
| Upright | Frame | Vertical load-bearing column |
| Beam | Horizontal | Pallet support member |
| Panel | Decking | Shelf or deck surface |
| Pallet Support Bar | Beam Accessory | Additional pallet support |
| Bracing | Stability | Frame stabilization |
| Base Plate | Foundation | Floor connection |
| Anchor Bolt | Foundation | Slab attachment |
| Safety Guard | Protection | Aisle protection |
| Cantilever Arm | Cantilever | Load support arm |
| Rail | ASRS | Shuttle/crane track |

### Key Rule

> If two items **behave differently under load**, they must not share the same taxonomy node.

---

## Taxonomy Hierarchy

```
Component Taxonomy
├── Frames
│   ├── Upright
│   ├── Base Plate
│   └── Bracing
├── Horizontal Members
│   ├── Beam
│   ├── Panel / Decking
│   └── Pallet Support Bar
├── Cantilever Components
│   ├── Cantilever Arm
│   └── Cantilever Column
├── ASRS Components
│   ├── Rail
│   ├── Shuttle Track
│   └── Bin Support
├── Safety & Accessories
│   ├── Safety Guard
│   ├── Row Spacer
│   └── Column Protector
└── Foundation
    ├── Anchor Bolt
    └── Shim
```

---

## Taxonomy vs. SKU

| Concept | Purpose | Example |
|---------|---------|---------|
| **Taxonomy** | What it IS (physical type) | `BEAM` |
| **SKU** | What product it IS (sellable item) | `GSS-BM-2700-1.6-SB` |

The taxonomy is **timeless** — SKUs change, taxonomy is stable.

---

## Taxonomy Schema

```sql
CREATE TABLE component_types (
    id UUID PRIMARY KEY,
    code VARCHAR(50) UNIQUE NOT NULL,
    name VARCHAR(255) NOT NULL,
    category VARCHAR(100) NOT NULL,
    parent_type_id UUID REFERENCES component_types(id),
    attributes JSONB,
    is_active BOOLEAN DEFAULT true,
    created_at TIMESTAMP NOT NULL
);
```

---

## Related Documentation

- [Physical Interfaces](./interfaces/README.md)
- [Assemblies](../05-assemblies/README.md)
- [Warehouse Spatial Taxonomy](../04-warehouse-spatial/README.md)
