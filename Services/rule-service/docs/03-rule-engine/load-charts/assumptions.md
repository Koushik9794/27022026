# Load Chart Assumptions

Every load chart has **engineering assumptions** that must be validated before lookup.

## Standard Assumption Fields

| Field | Description | Example |
|-------|-------------|---------|
| `load_type` | Loading pattern | UDL, POINT, COMBINED |
| `deflection_limit` | Max allowable deflection | L/200, L/180 |
| `material_spec` | Steel grade | S355, S275 |
| `temperature_min` | Min rated temperature | -20°C |
| `safety_factor` | Applied safety factor | 1.5 |
| `standard` | Engineering standard | EN15512, FEM, RMI |

## Assumption Validation

Before a chart is queried, the Rule Service validates:

```yaml
validation:
  - field: load_type
    condition: facts.loadType == chart.load_type
    error: "Load type mismatch"
    
  - field: temperature_min
    condition: facts.operatingTemp >= chart.temperature_min
    error: "Temperature below rated minimum"
    
  - field: material_spec
    condition: facts.material == chart.material_spec
    error: "Material specification mismatch"
```

## Why Assumptions Matter

| Without Assumptions | With Assumptions |
|---------------------|------------------|
| Beam rated for UDL used for point load | System rejects invalid lookup |
| Cold storage uses ambient-rated chart | System requires cold-rated chart |
| Mixed steel grades | System enforces material match |

## Example Chart with Assumptions

```json
{
  "chart_code": "STEP_BEAM_RF_COLD_V1",
  "component_type": "BEAM",
  "assumptions": {
    "load_type": "UDL",
    "deflection_limit": "L/200",
    "material_spec": "S355JO",
    "temperature_min": -30,
    "standard": "EN15512"
  }
}
```

> [!IMPORTANT]
> **Invariant:** A chart cannot be used if its assumptions are not met by the facts.
