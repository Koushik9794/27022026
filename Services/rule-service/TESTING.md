# Testing Seeded Rules

## Seeded Test Data

The database contains the following seeded data for testing:

### Product Group & Country
- **Product Group ID**: `11111111-1111-1111-1111-111111111111`
- **Country ID**: `22222222-2222-2222-2222-222222222222`

### RuleSet
- **RuleSet ID**: `33333333-3333-3333-3333-333333333333`
- **Name**: "Standard Warehouse Rules - US"
- **Status**: ACTIVE

### Rules

#### Rule 1: Spatial Constraint
- **ID**: `44444444-4444-4444-4444-444444444444`
- **Name**: "Spatial Constraint - Minimum Dimensions"
- **Category**: SPATIAL
- **Priority**: 1 (highest)
- **Severity**: ERROR
- **Conditions**:
  - `width > 100` (GT)
  - `height > 100` (GT)

#### Rule 2: Pricing Rule
- **ID**: `55555555-5555-5555-5555-555555555555`
- **Name**: "Pricing Rule - Bulk Discount"
- **Category**: PRICING
- **Priority**: 2
- **Severity**: INFO
- **Conditions**:
  - `quantity > 100` (GT)

## Verification Steps

### 1. Verify Data in Database

```bash
# Check RuleSets
docker exec rule_service_db psql -U postgres -d rule_service -c 'SELECT id, name, status FROM rule_sets;'

# Check Rules
docker exec rule_service_db psql -U postgres -d rule_service -c 'SELECT id, name, category, priority FROM rules;'

# Check Conditions
docker exec rule_service_db psql -U postgres -d rule_service -c 'SELECT rule_id, field, operator, value FROM rule_conditions;'
```

### 2. Test via API (Once Endpoints are Implemented)

Run the test scripts:

**PowerShell:**
```powershell
.\test-seeded-rules.ps1
```

**Bash (WSL):**
```bash
bash test-seeded-rules.sh
```

### 3. Manual API Testing

#### Get RuleSet
```bash
curl http://localhost:5001/api/v1/ruleset/33333333-3333-3333-3333-333333333333
```

#### Get Active Rules
```bash
curl http://localhost:5001/api/v1/rule-evaluation/active-rules/11111111-1111-1111-1111-111111111111/22222222-2222-2222-2222-222222222222
```

#### Evaluate Rules - PASS Scenario
All conditions met (width=150, height=150, quantity=200):
```bash
curl -X POST http://localhost:5001/api/v1/rule-evaluation/evaluate \
  -H "Content-Type: application/json" \
  -d '{
    "ruleSetId": "33333333-3333-3333-3333-333333333333",
    "productGroupId": "11111111-1111-1111-1111-111111111111",
    "countryId": "22222222-2222-2222-2222-222222222222",
    "configurationData": "{\"width\": 150, \"height\": 150, \"quantity\": 200}"
  }'
```

**Expected Result**: Both rules pass

#### Evaluate Rules - FAIL Scenario
Spatial rule fails (width=50, height=50):
```bash
curl -X POST http://localhost:5001/api/v1/rule-evaluation/evaluate \
  -H "Content-Type: application/json" \
  -d '{
    "ruleSetId": "33333333-3333-3333-3333-333333333333",
    "productGroupId": "11111111-1111-1111-1111-111111111111",
    "countryId": "22222222-2222-2222-2222-222222222222",
    "configurationData": "{\"width\": 50, \"height\": 50, \"quantity\": 200}"
  }'
```

**Expected Result**: Spatial rule fails (ERROR severity), pricing rule passes

## Current Status

✅ **Database**: Seeded data verified in database
⚠️ **Endpoints**: Currently return placeholder data - need to implement actual repository calls

## Next Steps to Enable Full Testing

The Wolverine HTTP endpoints in `RuleSetEndpoints.cs` and `RuleEvaluationEndpoints.cs` currently return sample data. To enable full testing:

1. Inject `IRuleRepository` and `IRuleEvaluationService` into endpoint methods
2. Replace placeholder returns with actual service calls
3. Re-run test scripts to verify end-to-end functionality
