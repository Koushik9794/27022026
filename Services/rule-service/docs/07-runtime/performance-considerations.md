# Performance Considerations

## Latency Targets

| Operation | P50 | P99 | Max |
|-----------|-----|-----|-----|
| Evaluate (cached) | <10ms | <50ms | <100ms |
| Evaluate (cold) | <100ms | <200ms | <500ms |
| Explain | <50ms | <100ms | <200ms |

## Optimization Techniques

### 1. Compiled Formulas
Formulas are parsed and compiled on load, not per-evaluation:
```csharp
// On cache load
var compiled = FormulaCompiler.Compile(formula.Expression);
cache.Set(key, compiled);

// On evaluation
var result = compiled.Evaluate(facts);
```

### 2. Parallel Rule Execution
Rules without dependencies execute in parallel:
```csharp
var independentRules = rules.Where(r => !r.HasDependencies);
await Task.WhenAll(independentRules.Select(r => ExecuteAsync(r, facts)));
```

### 3. Lazy Lookup Loading
Lookups load on first access, not upfront:
```csharp
var lookup = lazyLookups.GetOrAdd(lookupId, id => LoadLookup(id));
```

### 4. Short-Circuit Evaluation
Stop early when possible:
- Validation fails → Return immediately
- Required rule fails → Skip dependent rules
- Early termination configured → Stop on first match

## Memory Management

- **Pooled Buffers** - Reuse evaluation context objects
- **Span<T>** - Avoid allocations for string operations
- **Weak References** - For rarely-used cached items

## Load Testing

Run periodic load tests:
```bash
# 1000 concurrent evaluations
k6 run --vus 1000 --duration 60s evaluate-load-test.js
```

## Bottleneck Identification

1. **Database** - Check query plans, add indexes
2. **Cache** - Monitor hit ratios
3. **Formula** - Profile complex expressions
4. **Network** - Measure inter-service latency
