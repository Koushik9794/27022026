# Request/Response Samples

Complete examples for common API interactions.

## Evaluate Rules - SPR Example

### Request
```json
{
  "productGroup": "SPR",
  "facts": {
    "palletHeight": 1200,
    "palletWeight": 1000,
    "mheType": "reachTruck"
  }
}
```

### Response
```json
{
  "success": true,
  "results": [
    {
      "ruleId": "elevation-001",
      "ruleName": "Calculate Beam Height",
      "outcome": "pass",
      "computedValue": 1500
    }
  ],
  "errors": []
}
```

---

## Explain Rules - SPR Example

### Request
```json
{
  "productGroup": "SPR",
  "facts": {
    "palletHeight": 1200,
    "palletWeight": 1000,
    "mheType": "reachTruck"
  }
}
```

### Response
```json
{
  "success": true,
  "results": [],
  "explanation": {
    "rulesEvaluated": [
      {
        "ruleId": "elevation-001",
        "condition": "palletHeight > 0",
        "conditionMet": true
      }
    ],
    "decisionPath": [
      "Entered ruleset: SPR-Elevation",
      "Evaluated rule: elevation-001",
      "Applied formula: rack-height"
    ],
    "formulasApplied": [
      {
        "formulaId": "rack-height",
        "inputs": { "palletHeight": 1200 },
        "output": 1500
      }
    ]
  }
}
```

---

## Admin - Create Ruleset Example

### Request
```json
{
  "name": "SPR-Elevation-Rules",
  "description": "Rules for SPR elevation calculations",
  "productGroup": "SPR"
}
```

### Response
```json
{
  "id": "rs-001",
  "name": "SPR-Elevation-Rules",
  "description": "Rules for SPR elevation calculations",
  "productGroup": "SPR",
  "createdAt": "2026-01-09T00:00:00Z"
}
```
