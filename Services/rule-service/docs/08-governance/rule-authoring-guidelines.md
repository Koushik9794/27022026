# Rule Authoring Guidelines

Best practices for creating and maintaining rules in the Rule Service.

## General Principles

1. **Single Responsibility** - Each rule should do one thing well
2. **Explicit Naming** - Rule names should describe intent, not implementation
3. **Testable** - Every rule must have associated test cases
4. **Documented** - Include description and business rationale

## Naming Conventions

| Element | Format | Example |
|---------|--------|---------|
| Rule | `{domain}-{action}-{qualifier}` | `elevation-calculate-beam-height` |
| Ruleset | `{product-group}-{category}` | `SPR-elevation-rules` |
| Formula | `{calculation}-{output}` | `rack-height-calculation` |
| Lookup | `{entity}-{attribute}` | `beam-capacity-matrix` |

## Rule Structure

```yaml
rule:
  id: elevation-001
  name: Calculate Beam Height
  description: |
    Calculates the required beam height based on pallet dimensions
    and clearance requirements.
  businessRationale: |
    Ensures adequate clearance for MHE operation and pallet placement.
  type: calculation
  phase: calculation
  priority: 100
  condition: palletHeight > 0
  action: beamHeight = @formula('rack-height', palletHeight, clearance)
```

## Do's and Don'ts

### Do
- ✅ Use formulas for complex calculations
- ✅ Reference lookups for configurable values
- ✅ Include error messages for validation failures
- ✅ Document edge cases
- ✅ Test with boundary values

### Don't
- ❌ Hardcode values that may change
- ❌ Create circular dependencies
- ❌ Use overly complex conditions
- ❌ Skip validation rules
- ❌ Ignore performance implications

## Formula Best Practices

```
// Good: Clear, single-purpose
beamHeight = palletHeight + topClearance + beamFaceHeight

// Bad: Magic numbers, unclear intent
result = x + 150 + 50
```

## Testing Requirements

Each rule requires:
1. **Positive Test** - Expected behavior with valid input
2. **Negative Test** - Proper handling of invalid input
3. **Boundary Test** - Edge cases (min, max, zero)
4. **Integration Test** - Interaction with other rules
