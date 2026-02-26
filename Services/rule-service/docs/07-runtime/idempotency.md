# Idempotency

## Evaluation Idempotency

Rule evaluations are **idempotent by design**:

```
Same Input + Same Ruleset Version = Same Output
```

### Guarantees

1. **Deterministic** - No random or time-based logic in rules
2. **Side-Effect Free** - Evaluations don't modify state
3. **Versioned** - Same version always behaves identically

### Request Deduplication

For duplicate requests within a time window:

```csharp
var cacheKey = $"eval:{Hash(request)}";
if (cache.TryGet(cacheKey, out var cached))
    return cached;

var result = await Evaluate(request);
cache.Set(cacheKey, result, TimeSpan.FromSeconds(30));
return result;
```

## Idempotency Keys

Clients can provide idempotency keys:

```http
POST /rules/evaluate
Idempotency-Key: client-request-12345
```

Server behavior:
1. Check if key exists in store
2. If exists, return stored response
3. If not, evaluate and store response with key
4. Key expires after 24 hours

## Retry Safety

All evaluation endpoints are safe to retry:
- `POST /rules/evaluate` - Idempotent
- `POST /rules/explain` - Idempotent

Admin endpoints require idempotency keys for safety:
- `POST /admin/rulesets` - Use idempotency key
- `PUT /admin/rulesets/{id}` - Naturally idempotent
