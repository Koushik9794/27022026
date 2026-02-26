# Failure Case: Load Exceeded

Demonstrates rule failure when pallet weight exceeds beam capacity.

## Input Facts
```json
{
  "productGroup": "SPR",
  "facts": {
    "palletWeight": 3000,
    "palletsPerLevel": 3,
    "beamLength": 2700,
    "safetyFactor": 1.5
  }
}
```

## Evaluation Trace

### Step 1: Calculate Total Beam Load
```
Formula: beam-load
Input: palletWeight=3000, palletsPerLevel=3
Calculation: 3000 * 3 = 9000kg
Output: totalLoad = 9000
```

### Step 2: Apply Safety Factor
```
Formula: design-load
Input: totalLoad=9000, safetyFactor=1.5
Calculation: 9000 * 1.5 = 13500kg
Output: designLoad = 13500
```

### Step 3: Lookup Beam Capacity
```
Lookup: beam-capacity-matrix
Input: beamLength=2700, designLoad=13500
Match: NO BEAM FOUND - max capacity for 2700mm is 5000kg
Output: FAILURE
```

## Expected Response
```json
{
  "success": false,
  "results": [
    {
      "ruleId": "beam-selection-001",
      "ruleName": "Select Beam Type",
      "outcome": "fail",
      "error": {
        "code": "LOAD_EXCEEDED",
        "message": "No beam available for required capacity",
        "details": {
          "requiredCapacity": 13500,
          "maxAvailable": 5000
        }
      }
    }
  ],
  "recommendations": [
    "Reduce palletsPerLevel from 3 to 2",
    "Reduce palletWeight below 1666kg",
    "Consider double-deep racking"
  ]
}
```

## Resolution Options

1. **Reduce pallets per level** - Use 2 pallets instead of 3
2. **Reduce pallet weight** - Maximum 1666kg per pallet for 3-wide
3. **Use stronger beams** - Requires longer beam length
4. **Change racking type** - Consider drive-in or pushback
