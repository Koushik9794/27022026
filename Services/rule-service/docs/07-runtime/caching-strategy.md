# Caching Strategy

## Cache Key Structure

Rulesets are cached by composite key:

```
CacheKey: rulesets:{rulesetId}:{version}
```

Examples:
```
rulesets:rs-001:3
rulesets:rs-002:1
```

## Caching by (rulesetId, version)

### Why This Key Structure

1. **Immutability** - Versions never change, cache entries never stale
2. **Parallel Versions** - Multiple versions can coexist in cache
3. **Instant Rollback** - Previous versions likely still cached
4. **Predictable Invalidation** - Only invalidate on activation changes

### What Gets Cached

| Item | Cache Key Pattern | TTL |
|------|-------------------|-----|
| Ruleset metadata | `rulesets:{id}:{version}:meta` | Infinite* |
| Rules | `rulesets:{id}:{version}:rules` | Infinite* |
| Formulas | `formulas:{id}:{version}` | Infinite* |
| Lookups | `lookups:{id}:{version}` | Infinite* |
| Active version pointer | `active:{productGroup}` | 60s |

*Infinite = Until explicit invalidation

### Cache Hierarchy

```
L1: In-Memory (per instance)
    ├── Hot rulesets
    ├── Compiled formulas
    └── Lookup indexes

L2: Distributed Cache (Redis)
    ├── All active rulesets
    ├── Recent versions
    └── Shared across instances
```

## Cache Invalidation

### On Version Activation
```csharp
// Invalidate active pointer
cache.Remove($"active:{productGroup}");

// Pre-warm new version
cache.Set($"rulesets:{id}:{newVersion}:rules", rules);
```

### On Rollback
```csharp
// Invalidate active pointer
cache.Remove($"active:{productGroup}");

// Target version likely already cached
```

## Cache Warming

On service startup:
1. Load all active rulesets
2. Compile formulas
3. Index lookups
4. Populate L1 and L2 caches

## Cache Metrics

- `cache_hit_ratio` - Target: >95%
- `cache_miss_latency_ms` - Time to load on miss
- `cache_size_bytes` - Memory utilization
