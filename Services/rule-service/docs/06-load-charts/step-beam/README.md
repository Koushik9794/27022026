# Step Beam Load Charts

Step beams are horizontal load-bearing members in selective pallet racking.

## Chart Types

| Chart | Description | Dimensions |
|-------|-------------|------------|
| `STEP_BEAM_RF_V2` | RF Panel Step Beam | depth, width, thickness |
| `STEP_BEAM_6BEND_V1` | 6-Bend Panel | depth, width, bendType |
| `GUIDED_PSB_V1` | Guided Pallet Support | systemDepth, thickness |

## Dimension Schema

```json
{
  "dimensions": {
    "depth": { "type": "number", "unit": "mm" },
    "width": { "type": "number", "unit": "mm" },
    "thickness": { "type": "number", "unit": "mm" }
  }
}
```

## Example Entries

| Depth | Width | Thickness | Capacity (kg) |
|-------|-------|-----------|---------------|
| 100 | 50 | 1.5 | 2200 |
| 100 | 50 | 2.0 | 2500 |
| 120 | 50 | 2.0 | 3200 |

## Product Groups Using These Charts

- SPR (Selective Pallet Racking)
- Mobile Racking
- Push-Back Racking
