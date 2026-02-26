# Rule Manifest API - Testing Guide

## Overview
This document provides comprehensive testing guidance for the Rule Manifest API, including both automated integration tests and manual Postman testing.

## Test Types

### Integration Tests (Recommended)
**Location**: `tests/RuleService.IntegrationTests/RuleManifestIntegrationTests.cs`

Integration tests are the **primary testing approach** because:
- ✅ Tests against real PostgreSQL database
- ✅ Validates full data aggregation pipeline
- ✅ Ensures JSONB operations work correctly
- ✅ Verifies rule condition loading
- ✅ Tests version generation logic
- ✅ Automated and repeatable

### Postman Collection (Manual/Exploratory)
**Location**: `tests/postman/RuleManifestAPI.postman_collection.json`

Use Postman for:
- 🔍 Manual API exploration
- 📊 Performance testing
- 🐛 Debugging specific scenarios
- 📝 API documentation
- 🤝 Sharing with frontend team

---

## Integration Test Scenarios

### Test Suite Coverage (8 Tests)

#### 1. **GetManifest_WithValidProductAndCountry_ReturnsManifestWithRules**
**Purpose**: Verify basic manifest retrieval with valid parameters

**Setup**:
- Seeds a test RuleSet with active status
- Links to specific product group and country

**Assertions**:
- Returns 200 OK
- Manifest contains correct product/country IDs
- Version string is populated
- Rules array is not empty

**Example**:
```csharp
var result = await RuleManifestEndpoints.GetManifest(productGroupId, countryId, _ruleRepo, _matrixRepo);
var manifest = ((Ok<RuleManifestResponse>)result).Value;
Assert.NotEmpty(manifest.Rules);
```

---

#### 2. **GetManifest_WithNoActiveRuleSet_ReturnsNotFound**
**Purpose**: Validate error handling for non-existent configurations

**Setup**:
- Uses random GUIDs that don't exist in database

**Assertions**:
- Returns 404 Not Found
- Error message indicates no active ruleset

**Business Logic**:
- Prevents frontend from receiving empty/invalid manifests
- Clear error messaging for troubleshooting

---

#### 3. **GetManifest_IncludesRuleConditions**
**Purpose**: Verify deep loading of rule conditions

**Setup**:
- Seeds RuleSet with rules containing conditions
- Conditions have Field/Operator/Value structure

**Assertions**:
- At least one rule has conditions
- Conditions array is properly populated
- Field names match expected values (e.g., "PalletWidth")

**Critical for**:
- Frontend rule evaluation
- Condition-based validation logic

---

#### 4. **GetManifest_IncludesMatrixMetadata**
**Purpose**: Ensure matrix catalog is included in manifest

**Setup**:
- Seeds both RuleSet and LookupMatrix
- Matrix has name, category, and version

**Assertions**:
- Matrices array is populated
- Each matrix has required metadata
- Category matches expected value (e.g., "LOAD_CHART")

**Use Case**:
- Frontend knows which matrices are available
- Can validate MATRIX_LOOKUP calls in rules

---

#### 5. **GetManifest_VersionChanges_WhenRuleSetUpdated**
**Purpose**: Validate version header synchronization

**Setup**:
- Gets initial manifest version
- Updates RuleSet timestamp
- Gets manifest again

**Assertions**:
- Version string changes after update
- Version format: `YYYYMMDD.HHmm`

**Critical for**:
- Cache invalidation
- Real-time sync between admin and designers

**Example Version Flow**:
```
Initial:  20260125.0610
Updated:  20260125.0611  ← Timestamp changed
```

---

#### 6. **GetManifest_RulesOrderedByPriority**
**Purpose**: Verify rule evaluation order

**Setup**:
- Seeds rules with different priorities (1, 50, 100)
- Adds them in random order

**Assertions**:
- Rules are sorted descending by priority
- High priority (1) comes before low priority (100)

**Business Logic**:
- Critical rules (structural) evaluated first
- Info rules (compliance) evaluated last

---

#### 7. **GetManifest_IncludesFormulaRules**
**Purpose**: Validate formula-based rules are included

**Setup**:
- Seeds rule with formula instead of conditions
- Formula uses MATRIX_LOOKUP function

**Assertions**:
- Formula field is populated
- Contains expected function calls

**Example Formula**:
```csharp
"MATRIX_LOOKUP('BeamChart', upright, span, profile) > load"
```

---

#### 8. **Cleanup (IDisposable)**
**Purpose**: Ensure test isolation

**Implementation**:
- Deletes all test data after each run
- Prevents test pollution
- Maintains database cleanliness

---

## Running Integration Tests

### Prerequisites
1. **Docker Database Running**:
   ```bash
   docker-compose up -d postgres
   ```

2. **Database Migrated**:
   ```bash
   dotnet run --project Services/rule-service/RuleService.csproj
   ```

### Execute Tests
```bash
# Run all manifest tests
dotnet test Services/rule-service/tests/RuleService.IntegrationTests/RuleService.IntegrationTests.csproj --filter "FullyQualifiedName~RuleManifestIntegrationTests"

# Run specific test
dotnet test --filter "GetManifest_WithValidProductAndCountry_ReturnsManifestWithRules"

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"
```

### Expected Output
```
Test Run Successful.
Total tests: 8
     Passed: 8
     Failed: 0
   Skipped: 0
 Total time: 2.5 Seconds
```

---

## Postman Collection Usage

### Import Collection
1. Open Postman
2. Click **Import**
3. Select `tests/postman/RuleManifestAPI.postman_collection.json`
4. Collection appears in sidebar

### Configure Environment
Create environment with variables:
```json
{
  "baseUrl": "http://localhost:5001",
  "productGroupId": "11111111-1111-1111-1111-111111111111",
  "countryId": "22222222-2222-2222-2222-222222222222",
  "matrixId": "<set after creating matrix>"
}
```

### Test Scenarios

#### Scenario 1: Happy Path - Get Manifest
**Request**: `GET /api/v1/rules/manifest`
**Params**: `productGroupId`, `countryId`

**Expected Response** (200 OK):
```json
{
  "version": "20260125.0610",
  "productGroupId": "11111111-1111-1111-1111-111111111111",
  "countryId": "22222222-2222-2222-2222-222222222222",
  "rules": [
    {
      "id": "...",
      "name": "Beam Safety Check",
      "category": "STRUCTURAL",
      "severity": "ERROR",
      "priority": 1,
      "formula": "MATRIX_LOOKUP('BeamChart', upright, span, profile) > load",
      "conditions": []
    }
  ],
  "matrices": [
    {
      "name": "BeamChart",
      "category": "LOAD_CHART",
      "version": 1
    }
  ],
  "generatedAt": "2026-01-25T06:10:00Z"
}
```

**Postman Tests**:
- ✅ Status code is 200
- ✅ Version format matches regex `^\d{8}\.\d{4}$`
- ✅ Rules array has at least 1 item
- ✅ Each rule has id, name, category, severity

---

#### Scenario 2: Error Handling - Non-existent Product
**Request**: `GET /api/v1/rules/manifest`
**Params**: `productGroupId=99999999-9999-9999-9999-999999999999`

**Expected Response** (404 Not Found):
```json
{
  "message": "No active ruleset found for this configuration."
}
```

**Postman Tests**:
- ✅ Status code is 404
- ✅ Error message is present

---

#### Scenario 3: Matrix Choices - Interpolation
**Request**: `GET /api/v1/matrices/BeamChart/choices`
**Params**: `uprightId=ST20&span=2750&load=1500`

**Expected Response** (200 OK):
```json
[
  {
    "choiceId": "HEM_100",
    "capacity": 2900,
    "utilization": 51.72,
    "isSafe": true
  },
  {
    "choiceId": "HEM_80",
    "capacity": 1900,
    "utilization": 78.95,
    "isSafe": true
  }
]
```

**Postman Tests**:
- ✅ Choices sorted by utilization (ascending)
- ✅ Each choice has capacity, utilization, isSafe
- ✅ Response time < 200ms

---

#### Scenario 4: Matrix Cell Update
**Request**: `PATCH /api/v1/matrices/{id}/cell`
**Body**:
```json
{
  "path": ["uprights", "ST20", "2700", "HEM_80"],
  "value": 2100
}
```

**Expected Response** (204 No Content)

**Verification**:
1. Call GET choices again
2. Verify capacity changed to 2100
3. Verify utilization recalculated

---

### Performance Tests

#### Manifest Generation
**Target**: < 500ms for 100 rules + 10 matrices

**Postman Test**:
```javascript
pm.test("Response time is less than 500ms", function () {
    pm.expect(pm.response.responseTime).to.be.below(500);
});
```

#### Matrix Choices
**Target**: < 200ms for interpolation + ranking

**Optimization Tips**:
- JSONB indexing on `name` column
- Partial node retrieval (don't fetch full matrix)
- In-memory interpolation

---

## Test Data Setup

### Seed Test RuleSet
```sql
INSERT INTO rule_sets (id, name, product_group_id, country_id, status, effective_from, created_at, updated_at)
VALUES (
  '11111111-1111-1111-1111-111111111111',
  'Test Design Rules',
  '11111111-1111-1111-1111-111111111111',
  '22222222-2222-2222-2222-222222222222',
  'ACTIVE',
  NOW(),
  NOW(),
  NOW()
);
```

### Seed Test Matrix
```sql
INSERT INTO lookup_matrices (id, name, category, data, metadata, version, created_at, updated_at)
VALUES (
  gen_random_uuid(),
  'BeamChart',
  'LOAD_CHART',
  '{
    "uprights": {
      "ST20": {
        "HEM_80": [
          {"X": 2700, "Y": 2000},
          {"X": 2800, "Y": 1800}
        ],
        "HEM_100": [
          {"X": 2700, "Y": 3000},
          {"X": 2800, "Y": 2800}
        ]
      }
    }
  }'::jsonb,
  '{}'::jsonb,
  1,
  NOW(),
  NOW()
);
```

---

## Troubleshooting

### Test Failures

#### "No active ruleset found"
**Cause**: Database not seeded or wrong product/country IDs

**Fix**:
```bash
# Check if rulesets exist
psql -h localhost -p 5433 -U postgres -d rule_service -c "SELECT id, name, status FROM rule_sets;"

# Verify product_group_id and country_id match
```

#### "Matrix not found"
**Cause**: lookup_matrices table empty

**Fix**:
```bash
# Check matrices
psql -h localhost -p 5433 -U postgres -d rule_service -c "SELECT name, category, version FROM lookup_matrices;"
```

#### Integration tests hang
**Cause**: Database connection timeout

**Fix**:
```bash
# Verify Docker is running
docker ps | grep postgres

# Check connection string in test
Host=127.0.0.1;Port=5433;Database=rule_service;Username=postgres;Password=postgres
```

---

## CI/CD Integration

### GitHub Actions Example
```yaml
name: Integration Tests

on: [push, pull_request]

jobs:
  test:
    runs-on: ubuntu-latest
    
    services:
      postgres:
        image: postgres:15
        env:
          POSTGRES_PASSWORD: postgres
        ports:
          - 5433:5432
    
    steps:
      - uses: actions/checkout@v3
      
      - name: Setup .NET
        uses: actions/setup-dotnet@v3
        with:
          dotnet-version: '10.0.x'
      
      - name: Run Migrations
        run: dotnet run --project Services/rule-service/RuleService.csproj
      
      - name: Run Integration Tests
        run: dotnet test Services/rule-service/tests/RuleService.IntegrationTests/RuleService.IntegrationTests.csproj
```

---

## Best Practices

### Test Isolation
- ✅ Each test creates its own data
- ✅ Cleanup in `Dispose()`
- ✅ Use unique GUIDs per test
- ❌ Don't rely on shared state

### Assertions
- ✅ Test one concept per test
- ✅ Use descriptive test names
- ✅ Assert on specific values, not just "not null"
- ❌ Don't test framework behavior

### Performance
- ✅ Keep tests fast (< 5s each)
- ✅ Use transactions for cleanup
- ✅ Minimize database round-trips
- ❌ Don't test with production-size data

---

## Summary

| Test Type | Count | Purpose | Run Frequency |
|-----------|-------|---------|---------------|
| Integration Tests | 8 | Automated validation | Every commit |
| Postman Scenarios | 10 | Manual exploration | As needed |
| Performance Tests | 2 | Response time validation | Daily |

**Total Coverage**: 20 test scenarios across happy path, error handling, performance, and edge cases.

**Next Steps**:
1. Run integration tests: `dotnet test --filter RuleManifestIntegrationTests`
2. Import Postman collection
3. Execute happy path scenarios
4. Monitor performance metrics
