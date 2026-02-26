# Validation Checklist

Mandatory checks before ruleset activation.

## Pre-Activation Checklist

### Syntax & Structure
- [ ] All rules parse without errors
- [ ] All formulas compile successfully
- [ ] All lookup references resolve
- [ ] No circular dependencies detected

### Testing
- [ ] Unit tests pass for all rules
- [ ] Integration tests pass
- [ ] Performance benchmarks within thresholds
- [ ] Edge cases covered

### Review
- [ ] Peer review completed
- [ ] Business logic verified by domain expert
- [ ] Documentation updated
- [ ] Change log entry added

### Environment Validation
- [ ] Tested in development environment
- [ ] Tested in staging environment
- [ ] Smoke tests pass
- [ ] No regression in existing functionality

### Approval
- [ ] Technical lead approval
- [ ] Product owner approval (for business rule changes)
- [ ] Security review (if applicable)

## Automated Validation

The system runs these checks automatically:

| Check | Description | Blocking |
|-------|-------------|----------|
| Syntax | Parse all rule expressions | Yes |
| References | Verify formula/lookup references | Yes |
| Cycles | Detect circular dependencies | Yes |
| Coverage | Minimum test coverage threshold | Yes |
| Performance | Evaluation time < threshold | Warning |
| Complexity | Cyclomatic complexity limit | Warning |

## Validation Report

```json
{
  "rulesetId": "rs-001",
  "version": 4,
  "validationStatus": "PASSED",
  "checks": [
    { "name": "syntax", "status": "PASSED" },
    { "name": "references", "status": "PASSED" },
    { "name": "cycles", "status": "PASSED" },
    { "name": "coverage", "status": "PASSED", "value": "95%" },
    { "name": "performance", "status": "WARNING", "message": "P99 latency 45ms (threshold 50ms)" }
  ],
  "readyForActivation": true
}
```
