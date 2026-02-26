# Observability

## Trace IDs

Every request receives a trace ID for end-to-end tracking:

```http
X-Trace-Id: 550e8400-e29b-41d4-a716-446655440000
```

### Trace Propagation
```
Client Request
    └── X-Trace-Id: abc123
         ├── Rule Service
         │    └── traceId: abc123, spanId: span1
         ├── Database Query
         │    └── traceId: abc123, spanId: span2
         └── Cache Lookup
              └── traceId: abc123, spanId: span3
```

### Trace in Logs
```json
{
  "timestamp": "2026-01-09T12:00:00Z",
  "level": "INFO",
  "message": "Rule evaluation completed",
  "traceId": "abc123",
  "spanId": "span1",
  "rulesetId": "rs-001",
  "version": 3,
  "durationMs": 15
}
```

## Explainability Hooks

The `/rules/explain` endpoint provides detailed decision tracing:

### Explanation Structure
```json
{
  "traceId": "abc123",
  "rulesetId": "rs-001",
  "version": 3,
  "evaluationPath": [
    {
      "step": 1,
      "type": "RULE",
      "ruleId": "elevation-001",
      "condition": "palletHeight > 0",
      "conditionResult": true,
      "durationMs": 2
    },
    {
      "step": 2,
      "type": "FORMULA",
      "formulaId": "rack-height",
      "inputs": { "palletHeight": 1200 },
      "output": 1500,
      "durationMs": 1
    }
  ]
}
```

### Audit Logging
All evaluations are logged for audit:
```json
{
  "eventType": "RULE_EVALUATION",
  "traceId": "abc123",
  "timestamp": "2026-01-09T12:00:00Z",
  "input": { "productGroup": "SPR", "facts": {...} },
  "output": { "success": true, "results": [...] },
  "rulesetVersion": 3,
  "durationMs": 15
}
```

## Metrics

| Metric | Type | Description |
|--------|------|-------------|
| `rule_evaluations_total` | Counter | Total evaluations |
| `rule_evaluation_duration_ms` | Histogram | Evaluation latency |
| `rule_evaluation_errors_total` | Counter | Failed evaluations |
| `ruleset_cache_hits_total` | Counter | Cache hit count |
| `ruleset_cache_misses_total` | Counter | Cache miss count |
| `active_ruleset_version` | Gauge | Current active version |

## Dashboards

Recommended dashboard panels:
1. Evaluation throughput (req/s)
2. Latency percentiles (P50, P95, P99)
3. Error rate
4. Cache hit ratio
5. Active ruleset versions per product group
