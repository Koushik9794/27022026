# Error Codes

Standard error codes returned by the Rule Service APIs.

## HTTP Status Codes

| Code | Meaning | When Used |
|------|---------|-----------|
| 200 | OK | Successful request |
| 201 | Created | Resource successfully created |
| 400 | Bad Request | Invalid input or validation failure |
| 401 | Unauthorized | Missing or invalid authentication |
| 403 | Forbidden | Insufficient permissions |
| 404 | Not Found | Resource not found |
| 422 | Unprocessable Entity | Semantic validation error |
| 500 | Internal Server Error | Unexpected server error |

## Application Error Codes

| Error Code | Description | Resolution |
|------------|-------------|------------|
| RULE_001 | Rule not found | Verify rule ID exists |
| RULE_002 | Ruleset not found | Verify ruleset ID exists |
| RULE_003 | Invalid rule expression | Check rule syntax |
| FORMULA_001 | Formula not found | Verify formula ID exists |
| FORMULA_002 | Formula evaluation error | Check input facts |
| FORMULA_003 | Circular dependency detected | Review formula references |
| FACT_001 | Missing required fact | Provide all required facts |
| FACT_002 | Invalid fact type | Check fact data types |
| EVAL_001 | Evaluation timeout | Simplify rule complexity |
| EVAL_002 | Maximum depth exceeded | Review rule nesting |

## Error Response Format

```json
{
  "success": false,
  "error": {
    "code": "RULE_001",
    "message": "Rule not found",
    "details": {
      "ruleId": "unknown-rule-id"
    }
  }
}
```
