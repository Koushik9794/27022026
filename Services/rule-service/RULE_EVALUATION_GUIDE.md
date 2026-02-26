# Rule Evaluation Testing Guide

## Overview
This guide explains how to test the 97 seeded rules, verify their prioritization, and understand the required inputs for evaluation.

## Rule Prioritization

### Current Priority Scheme
- **Priority 1-2**: Original test rules (Spatial and Pricing)
- **Priority 3-69**: Design Rules from GSS_DesignRules_Extraction.csv
- **Priority 100-128**: Stability Guidelines from Stability_Guidelines.csv

### Recommended Priority Reorganization

For proper warehouse domain evaluation, rules should be prioritized as:

1. **Structural Safety (Priority 1-50)** - MUST pass first
   - Stability rules (Height-to-Depth ratio, base plates, bracing)
   - Frame capacity limits
   - Maximum heights and loads

2. **Spatial Constraints (Priority 51-100)** - Physical fit requirements
   - Level-to-level pitch
   - Rack dimensions (width, depth, height)
   - Pallet overhang and clearances
   - MHE working space

3. **Component Selection (Priority 101-150)** - Configuration rules
   - Beam selection based on load
   - Frame depth selection
   - Base plate selection
   - Row connector placement

4. **Compliance & Optimization (Priority 151-200)** - Nice-to-have
   - Pricing rules
   - Optimization suggestions
   - Best practices

## Required Input Parameters

### Core Warehouse Configuration
```json
{
  // Warehouse Constraints
  "WarehouseClearHeight": 8000,           // mm - building height
  "WarehouseWidth": 30000,                // mm
  "WarehouseDepth": 50000,                // mm
  
  // Pallet Specifications
  "PalletWidth": 1200,                    // mm (standard: 800, 1000, 1200)
  "PalletDepth": 1000,                    // mm (standard: 800, 1000, 1200)
  "PalletHeight": 150,                    // mm
  "PalletsPerLevel": 2,                   // number
  
  // SKU/Load Information
  "SKUWidth": 1150,                       // mm
  "SKUDepth": 950,                        // mm
  "SKUHeight": 1200,                      // mm
  "SKUWeight": 800,                       // kg
  "SKUsPerPallet": 1,                     // number
  "LoadPerPallet": 800,                   // kg
  
  // Rack Configuration
  "NumberOfLevels": 4,                    // number of loading levels
  "UnitsPerRow": 3,                       // number of units in a row
  "FrameDepth": 800,                      // mm (typically PalletDepth - 200)
  "BeamSpan": 2700,                       // mm
  "LevelPitch": 50,                       // mm - vertical spacing
  "FirstLevelHeight": 400,                // mm from floor
  
  // MHE (Material Handling Equipment)
  "MHEType": "Counterbalance",            // or "Reach Truck", "Articulated"
  "MHEMaxForkHeight": 6000,               // mm
  "MHEWorkingAisle": 3600,                // mm
  "MHELoadCapacity": 2000,                // kg at max height
  
  // Stability Parameters
  "UnitType": "DoubleSided",              // or "SingleSided"
  "RowConnectorLength": 100,              // mm
  "UnsupportedLength": 2500,              // mm (USL)
  
  // Clearances
  "VerticalClearance": 75,                // mm between SKU top and beam
  "HorizontalClearance": 100,             // mm between pallets
  "PalletOverhang": 100,                  // mm on depth sides
  "BeamToPillarClearance": 100            // mm
}
```

## Test Scenarios

### Scenario 1: Valid Standard Configuration
**Purpose**: Verify all rules pass for a typical warehouse setup

```json
{
  "WarehouseClearHeight": 8000,
  "PalletWidth": 1200,
  "PalletDepth": 1000,
  "PalletHeight": 150,
  "PalletsPerLevel": 2,
  "SKUHeight": 1200,
  "SKUWeight": 800,
  "LoadPerPallet": 800,
  "NumberOfLevels": 4,
  "UnitsPerRow": 3,
  "FrameDepth": 800,
  "BeamSpan": 2700,
  "MHEMaxForkHeight": 6000,
  "MHEWorkingAisle": 3600,
  "UnitType": "DoubleSided",
  "UnsupportedLength": 2500,
  "VerticalClearance": 75,
  "HorizontalClearance": 100
}
```

**Expected**: All rules pass ✅

### Scenario 2: Height-to-Depth Ratio Violation
**Purpose**: Test stability rule failure

```json
{
  "LastLoadingLevelHeight": 7000,
  "FrameDepth": 800,
  "UnitType": "SingleSided"
}
```

**Expected**: 
- Rule `HD001` fails: HeightToDepthRatio = 7000/800 = 8.75 (exceeds safe limit of 6)
- Severity: ERROR
- Recommendation: Require additional stability components

### Scenario 3: Single Level Configuration
**Purpose**: Test back bracing requirements

```json
{
  "NumberOfLevels": 1,
  "UnitsPerRow": 2
}
```

**Expected**:
- Rule `LL002` triggers: Back bracing required
- Rule `LL003` specifies: "Tie rod Dia 9.5mm with turnbuckle"
- Severity: ERROR if not provided

### Scenario 4: Excessive Pallet Load
**Purpose**: Test MHE capacity rules

```json
{
  "LoadPerPallet": 2500,
  "MHELoadCapacity": 2000,
  "MHEMaxForkHeight": 6000
}
```

**Expected**:
- Rule `MHE001` fails: Pallet load (2500kg) exceeds MHE capacity (2000kg)
- Severity: ERROR
- Recommendation: Reduce load or select different MHE

### Scenario 5: Insufficient Aisle Width
**Purpose**: Test MHE working space

```json
{
  "AisleWidth": 3000,
  "MHEWorkingAisle": 3600,
  "MHEType": "Counterbalance"
}
```

**Expected**:
- Rule `MHE002` fails: Aisle too narrow
- Severity: ERROR

## How to Execute Tests

### Option 1: Via API (Once Implemented)
```bash
curl -X POST http://localhost:5001/api/v1/rule-evaluation/evaluate \
  -H "Content-Type: application/json" \
  -d '{
    "ruleSetId": "aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa",
    "productGroupId": "11111111-1111-1111-1111-111111111111",
    "countryId": "22222222-2222-2222-2222-222222222222",
    "configurationData": "{\"WarehouseClearHeight\": 8000, \"PalletWidth\": 1200, ...}"
  }'
```

### Option 2: Via Unit Tests
Create test cases in `RuleEvaluationServiceTests.cs`:

```csharp
[Fact]
public async Task EvaluateRules_ValidConfiguration_AllRulesPass()
{
    // Arrange
    var config = JsonSerializer.Serialize(new {
        WarehouseClearHeight = 8000,
        PalletWidth = 1200,
        // ... all parameters
    });
    
    // Act
    var result = await _service.EvaluateRuleSetAsync(ruleSet, config);
    
    // Assert
    Assert.True(result.Success);
    Assert.All(result.Outcomes, o => Assert.True(o.Passed));
}
```

### Option 3: Direct Database Query
```sql
-- Get rules in priority order
SELECT 
    r.priority,
    r.name,
    r.category,
    r.severity,
    string_agg(rc.field || ' ' || rc.operator || ' ' || rc.value, ' AND ') as conditions
FROM rules r
LEFT JOIN rule_conditions rc ON r.id = rc.rule_id
WHERE r.id IN (
    SELECT rule_id FROM ruleset_rules 
    WHERE ruleset_id = 'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa'
)
GROUP BY r.id, r.priority, r.name, r.category, r.severity
ORDER BY r.priority;
```

## Rule Evaluation Order

The service should evaluate rules in this sequence:

1. **Load all active rules** for the product group and country
2. **Sort by priority** (ascending - lower number = higher priority)
3. **For each rule in order**:
   - Parse conditions
   - Evaluate against input configuration
   - Record outcome (pass/fail)
   - If ERROR severity and fails → mark overall result as failed
   - If WARNING severity and fails → log warning but continue
   - If INFO severity → always continue

4. **Return aggregated results**:
   - Overall success/failure
   - List of all rule outcomes
   - Failed rules with severity ERROR
   - Warnings from severity WARNING
   - Informational messages

## Next Steps

1. **Implement full repository methods** in `DapperRuleRepository` to fetch rules with conditions
2. **Enhance expression engine** to handle complex formulas from CSV
3. **Create comprehensive test suite** with all scenarios above
4. **Build UI/API** to accept configuration and display results
5. **Add rule explanation** feature to show why a rule failed

## Domain Expert Validation Needed

As a warehouse domain expert, please review:
- [ ] Is the priority scheme correct?
- [ ] Are there missing input parameters?
- [ ] Should certain rules block evaluation of subsequent rules?
- [ ] Are the test scenarios realistic?
- [ ] What are the most critical failure modes to test?
