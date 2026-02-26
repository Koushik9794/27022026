# Attributes & Units of Measurement

> Engineering consistency through **strict attribute definitions**.

## Attribute Definition Philosophy

Attributes are **defined once**, globally, with strict meaning.

| Attribute Name | Data Type | Unit | Example |
|----------------|-----------|------|---------|
| length | Number | mm | 2700 |
| height | Number | mm | 10500 |
| width | Number | mm | 100 |
| thickness | Number | mm | 2.0 |
| depth | Number | mm | 1100 |
| load_capacity | Number | kg | 2500 |
| yield_strength | Number | MPa | 355 |
| deflection_limit | Enum | — | L/200, L/180 |
| seismic_zone | Enum | — | Zone-1, Zone-2, Zone-3 |
| temperature_min | Number | °C | -30 |
| weight | Number | kg | 15.5 |

---

## Attribute Rules

| Rule | Description |
|------|-------------|
| Units are **never optional** | Every numeric attribute has a defined unit |
| No free-text numerics | All values are validated against data type |
| Same attribute = same meaning | `length` always means the same thing |
| Enums are closed | Only defined values are valid |

---

## Why This Matters

This ensures:
- No unit mismatch (mm vs cm)
- Reliable load calculations
- Safe rule evaluation
- Consistent BOM generation

---

## Attribute Categories

### Dimensional Attributes

| Attribute | Unit | Used For |
|-----------|------|----------|
| `length` | mm | Beams, rails |
| `height` | mm | Uprights, frames |
| `width` | mm | Pallets, bays |
| `depth` | mm | Frames, pallets |
| `thickness` | mm | Plates, panels |

### Structural Attributes

| Attribute | Unit | Used For |
|-----------|------|----------|
| `load_capacity` | kg | Beams, arms |
| `yield_strength` | MPa | Steel specification |
| `moment_of_inertia` | cm⁴ | Beam sizing |
| `section_modulus` | cm³ | Stress calculation |

### Environmental Attributes

| Attribute | Unit | Used For |
|-----------|------|----------|
| `temperature_min` | °C | Cold storage |
| `temperature_max` | °C | Environment |
| `corrosion_class` | Enum | Coating selection |

---

## Attribute Schema

```sql
CREATE TABLE attribute_definitions (
    id UUID PRIMARY KEY,
    code VARCHAR(50) UNIQUE NOT NULL,
    name VARCHAR(255) NOT NULL,
    data_type VARCHAR(20) NOT NULL, -- NUMBER, ENUM, TEXT, BOOLEAN
    unit VARCHAR(20),
    min_value DECIMAL,
    max_value DECIMAL,
    enum_values JSONB,
    is_required BOOLEAN DEFAULT false,
    created_at TIMESTAMP NOT NULL
);
```
