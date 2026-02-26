# ASRS Load Charts

Automated Storage and Retrieval System structural components.

## Chart Types

| Chart | Description | Dimensions |
|-------|-------------|------------|
| `ASRS_RAIL_V1` | Rail Load Capacity | span, axleLoad, speed |
| `ASRS_SHUTTLE_V1` | Shuttle Support | trackWidth, binWeight |
| `ASRS_BIN_V1` | Bin Support Capacity | binWidth, binDepth |

## Dimension Schema (Rail)

```json
{
  "dimensions": {
    "span": { "type": "number", "unit": "mm" },
    "axleLoad": { "type": "number", "unit": "kg" },
    "speed": { "type": "number", "unit": "m/s" }
  }
}
```

## Example Entries (Rail)

| Span | Axle Load | Speed | Capacity (kg) |
|------|-----------|-------|---------------|
| 2400 | 500 | 2.0 | 1500 |
| 2400 | 500 | 4.0 | 1200 |
| 3000 | 500 | 2.0 | 1200 |

## Product Groups Using These Charts

- Mini-Load ASRS
- Pallet ASRS
- Shuttle Systems
