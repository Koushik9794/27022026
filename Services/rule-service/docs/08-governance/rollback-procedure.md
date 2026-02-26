# Rollback Procedure

Emergency and planned rollback procedures.

## When to Rollback

- ❗ Production evaluation errors increasing
- ❗ Incorrect business outcomes detected
- ❗ Performance degradation after activation
- ❗ Critical bug discovered post-activation

## Emergency Rollback

### Step 1: Identify Issue
```
Alert: rule_evaluation_errors_total spike
Dashboard: Check error rate and error messages
Logs: Search for specific rulesetId and version
```

### Step 2: Authorize Rollback
- Contact Release Manager or Technical Lead
- Document the issue and decision
- Identify target version (usually n-1)

### Step 3: Execute Rollback
```http
POST /admin/rulesets/{id}/rollback
Authorization: Bearer {admin-token}
{
  "targetVersion": 3,
  "reason": "Version 4 causing incorrect beam height calculations",
  "isEmergency": true
}
```

### Step 4: Verify
- Confirm active version changed
- Monitor error rates
- Verify evaluations returning expected results
- Notify stakeholders

### Step 5: Post-Mortem
- Document root cause
- Create fix for failed version
- Update validation checklist if needed

## Rollback Timeline

| Scenario | Target Time |
|----------|-------------|
| Emergency (production impact) | < 5 minutes |
| Urgent (incorrect results) | < 15 minutes |
| Planned | Scheduled window |

## Rollback Safeguards

> [!IMPORTANT]
> - Previous versions are never deleted
> - Rollback is atomic and instant
> - All cache layers are invalidated
> - Audit log captures every rollback

## Rollback Limitations

Cannot rollback if:
- Target version was never activated (no production data)
- Schema incompatibility between versions
- External system dependencies changed

## Testing Rollback

Regularly test rollback capability:
1. Schedule quarterly rollback drills
2. Practice in staging environment
3. Verify rollback completes within SLA
4. Document any issues
