# Effective Dating

## Overview

Effective dating controls when ruleset versions become active and when they expire. This enables:
- Scheduled rule changes (e.g., new pricing rules on Jan 1)
- Time-bound promotions or temporary rules
- Planned transitions between ruleset versions

## Effective Date Fields

| Field | Description |
|-------|-------------|
| `effective_from` | Date/time when version becomes active |
| `effective_to` | Date/time when version expires (optional) |

## Evaluation Logic

When evaluating rules, the system selects the appropriate version:

```
SELECT * FROM rulesets
WHERE product_group = :productGroup
  AND status = 'active'
  AND effective_from <= :evaluationTime
  AND (effective_to IS NULL OR effective_to > :evaluationTime)
ORDER BY effective_from DESC
LIMIT 1
```

## Overlapping Versions

Multiple versions can be scheduled with different effective dates:

```
Timeline:
|----V1----|----V2----|----V3----|
Jan 1      Mar 1      Jun 1      ...

Evaluation at Feb 15 → Uses V1
Evaluation at Apr 10 → Uses V2
Evaluation at Jul 20 → Uses V3
```

## Future Scheduling

Schedule a version to activate in the future:

```json
{
  "rulesetId": "rs-001",
  "version": 4,
  "effectiveFrom": "2026-03-01T00:00:00Z",
  "effectiveTo": null
}
```

## Backdating Considerations

> [!CAUTION]
> Backdating effective dates can cause inconsistencies in historical evaluations. Use with extreme caution and only for correction scenarios.
