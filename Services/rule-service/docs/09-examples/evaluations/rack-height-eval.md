# Rack Height Evaluation Example

Demonstrates how rack height is calculated based on input facts.

## Input Facts
```json
{
  "productGroup": "SPR",
  "facts": {
    "palletHeight": 1500,
    "palletsPerLevel": 2,
    "numberOfLevels": 4,
    "topClearance": 150,
    "beamFaceHeight": 100
  }
}
```

## Evaluation Trace

### Step 1: Calculate Level Height
```
Formula: level-height
Input: palletHeight=1500, topClearance=150, beamFaceHeight=100
Calculation: 1500 + 150 + 100 = 1750mm
Output: levelHeight = 1750
```

### Step 2: Calculate Total Rack Height
```
Formula: rack-height
Input: levelHeight=1750, numberOfLevels=4
Calculation: 1750 * 4 + 200 (base plate) = 7200mm
Output: rackHeight = 7200
```

## Expected Response
```json
{
  "success": true,
  "results": [
    {
      "ruleId": "rack-height-001",
      "ruleName": "Calculate Rack Height",
      "outcome": "pass",
      "computedValues": {
        "levelHeight": 1750,
        "rackHeight": 7200
      }
    }
  ]
}
```

## Validation
- Total height within building constraint ✅
- Level heights uniform ✅
- Base plate accounted for ✅
