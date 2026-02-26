# Failure Case: Aisle Violation

Demonstrates rule failure when aisle width is insufficient for MHE.

## Input Facts
```json
{
  "productGroup": "SPR",
  "facts": {
    "mheType": "reachTruck",
    "aisleWidth": 2000,
    "palletDepth": 1200,
    "frameDepth": 1100
  }
}
```

## Evaluation Trace

### Step 1: Lookup MHE Requirements
```
Lookup: mhe-clearance-matrix
Input: mheType="reachTruck"
Match: { type: "reachTruck", minAisle: 2700, minTurnRadius: 1800 }
Output: requiredAisle = 2700
```

### Step 2: Validate Aisle Width
```
Rule: aisle-clearance-check
Condition: aisleWidth >= requiredAisle
Evaluation: 2000 >= 2700 → FALSE
Output: FAILURE
```

## Expected Response
```json
{
  "success": false,
  "results": [
    {
      "ruleId": "aisle-001",
      "ruleName": "Validate Aisle Clearance",
      "outcome": "fail",
      "error": {
        "code": "AISLE_VIOLATION",
        "message": "Aisle width insufficient for selected MHE",
        "details": {
          "providedAisle": 2000,
          "requiredAisle": 2700,
          "mheType": "reachTruck",
          "deficit": 700
        }
      }
    }
  ],
  "recommendations": [
    "Increase aisle width to 2700mm minimum",
    "Change MHE type to 'walkie' (requires 2000mm)",
    "Consider very narrow aisle (VNA) configuration"
  ]
}
```

## MHE Aisle Requirements Reference

| MHE Type | Min Aisle | Typical Usage |
|----------|-----------|---------------|
| walkie | 2000mm | Low throughput |
| counterbalance | 3500mm | Standard |
| reachTruck | 2700mm | High density |
| turretTruck | 1800mm | VNA |

## Resolution Options

1. **Widen aisles** - Requires layout change, reduces storage density
2. **Change MHE** - Use walkie stacker (lower lift height)
3. **VNA design** - Requires turret truck investment
