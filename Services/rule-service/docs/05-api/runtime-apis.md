# Runtime APIs

Contract-first documentation for rule evaluation endpoints.

## `/rules/evaluate`

### POST /rules/evaluate
Evaluate rules against provided facts.

#### Request
```http
POST /rules/evaluate
Content-Type: application/json
```

```json
{
  "productGroup": "SPR",
  "facts": {
    
  }
}
```

#### Response
```json
{
  "success": true,
  "results": [],
  "errors": []
}
```

---

## `/rules/explain`

### POST /rules/explain
Evaluate rules and return detailed explanation of the decision path.

#### Request
```http
POST /rules/explain
Content-Type: application/json
```

```json
{
  "productGroup": "SPR",
  "facts": {
    
  }
}
```

#### Response
```json
{
  "success": true,
  "results": [],
  "explanation": {
    "rulesEvaluated": [],
    "decisionPath": [],
    "formulasApplied": []
  }
}
```
