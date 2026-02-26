# Audit and Compliance

Enterprise audit and compliance requirements.

## Audit Requirements

### What Gets Logged

| Event | Data Captured |
|-------|---------------|
| Ruleset Created | Who, when, initial content |
| Ruleset Modified | Who, when, diff of changes |
| Ruleset Approved | Approver, timestamp, comments |
| Ruleset Activated | Who, when, from-version, to-version |
| Ruleset Rolled Back | Who, when, reason, target version |
| Rule Evaluated | Trace ID, input facts, output, ruleset version |

### Retention Policy

| Log Type | Retention | Storage |
|----------|-----------|---------|
| Change Logs | 7 years | Long-term archive |
| Approval Logs | 7 years | Long-term archive |
| Evaluation Logs | 90 days | Hot storage |
| Evaluation Logs | 2 years | Cold archive |

## Compliance Controls

### SOC 2 Type II
- ✅ Access controls documented
- ✅ Change management process
- ✅ Audit trail for all changes
- ✅ Segregation of duties (author ≠ approver)

### GDPR
- ✅ No PII in rule definitions
- ✅ Evaluation logs anonymized
- ✅ Data retention policies enforced
- ✅ Right to explanation (via /explain endpoint)

## Access Control

```yaml
permissions:
  rule_author:
    - rulesets:create
    - rulesets:edit
    - rulesets:submit
  
  technical_lead:
    - rulesets:*
    - approvals:approve
    - rollback:execute
  
  auditor:
    - rulesets:read
    - audit_logs:read
    - evaluations:read
```

## Audit Reports

### Monthly Report
- Total ruleset changes
- Approval statistics
- Rollback incidents
- Evaluation volume trends

### On-Demand Queries
```http
GET /admin/audit/logs?rulesetId={id}&from={date}&to={date}
```

## Evidence Collection

For compliance audits, export:
1. Change history for specific rulesets
2. Approval chain for activated versions
3. Rollback incident reports
4. Access control configurations
