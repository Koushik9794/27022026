# Cantilever Load Charts

Cantilever components for cantilever racking systems.

## Chart Types

| Chart | Description | Dimensions |
|-------|-------------|------------|
| `CANT_ARM_HD_V1` | Heavy-Duty Arm | armLength, section, thickness |
| `CANT_ARM_LD_V1` | Light-Duty Arm | armLength, section |
| `CANT_COLUMN_V1` | Column Capacity | height, section, baseType |

## Dimension Schema (Arms)

```json
{
  "dimensions": {
    "armLength": { "type": "number", "unit": "mm" },
    "section": { "type": "string" },
    "thickness": { "type": "number", "unit": "mm" }
  }
}
```

## Example Entries (Heavy-Duty Arm)

| Arm Length | Section | Thickness | Capacity (kg) |
|------------|---------|-----------|---------------|
| 600 | C150 | 3.15 | 1200 |
| 900 | C150 | 3.15 | 900 |
| 1200 | C200 | 3.15 | 1800 |
| 1500 | C200 | 4.0 | 2000 |

## Product Groups Using These Charts

- Cantilever Racking
- Lumber Storage
- Pipe Storage
