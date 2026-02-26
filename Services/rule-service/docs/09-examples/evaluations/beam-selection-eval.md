# Beam Selection Evaluation Example

Demonstrates how beam type is selected based on load requirements.

## Input Facts
```json
{
  "productGroup": "SPR",
  "facts": {
    "palletWeight": 1200,
    "palletsPerLevel": 2,
    "beamLength": 2700,
    "safetyFactor": 1.5
  }
}
```

## Evaluation Trace

### Step 1: Calculate Total Beam Load
```
Formula: beam-load
Input: palletWeight=1200, palletsPerLevel=2
Calculation: 1200 * 2 = 2400kg
Output: totalLoad = 2400
```

### Step 2: Apply Safety Factor
```
Formula: design-load
Input: totalLoad=2400, safetyFactor=1.5
Calculation: 2400 * 1.5 = 3600kg
Output: designLoad = 3600
```

### Step 3: Lookup Beam Capacity
```
Lookup: beam-capacity-matrix
Input: beamLength=2700, designLoad=3600
Match: { length: 2700, minCapacity: 3600 }
Output: beamType = "2700-4000"
```

## Expected Response
```json
{
  "success": true,
  "results": [
    {
      "ruleId": "beam-selection-001",
      "ruleName": "Select Beam Type",
      "outcome": "pass",
      "computedValues": {
        "totalLoad": 2400,
        "designLoad": 3600,
        "beamType": "2700-4000"
      }
    }
  ]
}
```

## Beam Capacity Reference

| Beam Type | Length | Capacity |
|-----------|--------|----------|
| 2700-2500 | 2700mm | 2500kg |
| 2700-3000 | 2700mm | 3000kg |
| 2700-4000 | 2700mm | 4000kg ← Selected |
| 2700-5000 | 2700mm | 5000kg |
