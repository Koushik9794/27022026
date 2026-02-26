# Rule Service - UI Requirements

## Overview
This document outlines the user interfaces required to consume the rule-service for SPR design rules integration. It covers admin interfaces for rule management, configurator interfaces for real-time validation, and testing/debugging tools.

## 1. Admin Rule Management Interface

### Purpose
Allow administrators to manage design rules, load charts, and rule sets without code changes.

### User Roles
- **System Admin**: Full access to all rule management features
- **Engineering Lead**: Can create/edit rules, requires approval for activation
- **QA Engineer**: Read-only access, can run test scenarios

### Key Features
#### 1.1 Rule Set Management
**List View**

```text
┌─────────────────────────────────────────────────────────────┐
│ Rule Sets                                    [+ New RuleSet] │
├─────────────────────────────────────────────────────────────┤
│ Name                    │ Scope          │ Rules │ Status   │
├─────────────────────────┼────────────────┼───────┼──────────┤
│ SPR v2 Design Rules     │ PRODUCT_GROUP  │ 69    │ Active   │
│ SPR Stability           │ PRODUCT_GROUP  │ 29    │ Active   │
│ IS 15635 Compliance     │ COUNTRY        │ 15    │ Active   │
│ Building Constraints    │ WAREHOUSE      │ 8     │ Draft    │
│ Fire Safety (Global)    │ GLOBAL         │ 12    │ Active   │
└─────────────────────────┴────────────────┴───────┴──────────┘
```

**Detail View**

```text
┌─────────────────────────────────────────────────────────────┐
│ SPR v2 Design Rules                          [Edit] [Delete] │
├─────────────────────────────────────────────────────────────┤
│ Scope:          PRODUCT_GROUP                                │
│ Product Group:  Selective Pallet Racking (SPR)              │
│ Country:        All                                          │
│ Status:         Active                                       │
│ Created:        2026-01-15 by admin@godrej.com              │
│ Last Modified:  2026-01-20 by engineering@godrej.com        │
│                                                              │
│ Rules (69)                                    [+ Add Rule]   │
│ ┌──────────────────────────────────────────────────────┐    │
│ │ FD001 - Frame Depth Calculation              [Edit]  │    │
│ │ Formula: PalletDepth - 200                           │    │
│ │ Priority: 200 | Phase: CALCULATION                   │    │
│ ├──────────────────────────────────────────────────────┤    │
│ │ RH001a - Warehouse Clearance Constraint      [Edit]  │    │
│ │ Formula: WarehouseClearHeight - 200 - PalletHeight   │    │
│ │ Priority: 200 | Phase: CALCULATION                   │    │
│ └──────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

#### 1.2 Rule Editor
**Create/Edit Rule Form**

```text
┌─────────────────────────────────────────────────────────────┐
│ Edit Rule: FD001                                             │
├─────────────────────────────────────────────────────────────┤
│ Rule ID:        FD001                                        │
│ Name:           Frame Depth Calculation                      │
│ Description:    Calculates frame depth based on pallet depth │
│                 with standard overhang                       │
│                                                              │
│ Category:       ○ SPATIAL  ● STRUCTURAL  ○ COMPLIANCE       │
│ Rule Type:      ● FORMULA  ○ VALIDATION  ○ LOOKUP           │
│                                                              │
│ Formula:        [PalletDepth - 200                        ]  │
│                 [Test Formula]                               │
│                                                              │
│ Output Field:   [FrameDepth                               ]  │
│                                                              │
│ Parameters:     [PalletDepth                              ]  │
│                 [+ Add Parameter]                            │
│                                                              │
│ Validation:                                                  │
│   Condition:    [FrameDepth >= 500 AND FrameDepth <= 1500 ]  │
│   Severity:     ● ERROR  ○ WARNING  ○ INFO                  │
│   Message:      [Frame depth must be between 500-1500mm   ]  │
│                                                              │
│ Execution:                                                   │
│   Priority:     [200                                      ]  │
│   Phase:        [CALCULATION                              ▼] │
│                                                              │
│ Dependencies:   [None                                     ▼] │
│                 [+ Add Dependency]                           │
│                                                              │
│                           [Cancel]  [Save Draft]  [Activate] │
└─────────────────────────────────────────────────────────────┘
```

#### 1.3 Formula Tester
**Interactive Formula Testing**

```text
┌─────────────────────────────────────────────────────────────┐
│ Formula Tester                                               │
├─────────────────────────────────────────────────────────────┤
│ Formula:  [PalletDepth - 200                              ]  │
│                                                              │
│ Test Inputs:                                                 │
│   PalletDepth:  [1200                                     ]  │
│                 [+ Add Parameter]                            │
│                                                              │
│                                              [Evaluate]       │
│                                                              │
│ Result:                                                      │
│ ┌──────────────────────────────────────────────────────┐    │
│ │ ✓ Success                                            │    │
│ │ Output: FrameDepth = 1000                            │    │
│ │ Execution Time: 2ms                                  │    │
│ └──────────────────────────────────────────────────────┘    │
│                                                              │
│ Test Scenarios:                                              │
│ ┌──────────────────────────────────────────────────────┐    │
│ │ Scenario 1: Standard 1200mm pallet        [Run]      │    │
│ │ Scenario 2: Small 1000mm pallet           [Run]      │    │
│ │ Scenario 3: Edge case - 500mm depth       [Run]      │    │
│ └──────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

#### 1.4 Load Chart Management
**Load Chart Upload**

```text
┌─────────────────────────────────────────────────────────────┐
│ Import Load Chart                                            │
├─────────────────────────────────────────────────────────────┤
│ Chart Code:       [BEAM_STEP_2024                         ]  │
│ Component Type:   [Step Beam                              ▼] │
│ Version:          [2.0                                    ]  │
│ Effective From:   [2024-01-01                             ]  │
│                                                              │
│ Upload File:      [Choose File] beam_load_chart.xlsx        │
│                   ✓ Validated: 156 entries                   │
│                                                              │
│ Preview:                                                     │
│ ┌──────────────────────────────────────────────────────┐    │
│ │ Span  │ Section    │ Capacity │ Conditions          │    │
│ ├───────┼────────────┼──────────┼─────────────────────┤    │
│ │ 2700  │ 100x50x1.5 │ 2200 kg  │ UDL, L/200          │    │
│ │ 2700  │ 100x50x2.0 │ 2500 kg  │ UDL, L/200          │    │
│ │ 3000  │ 120x50x2.0 │ 3200 kg  │ UDL, L/200          │    │
│ └───────┴────────────┴──────────┴─────────────────────┘    │
│                                                              │
│                                    [Cancel]  [Import]        │
└─────────────────────────────────────────────────────────────┘
```

## 2. Configurator Integration Interface

### Purpose
Provide real-time rule validation during warehouse configuration with clear feedback.

### User Roles
- **Sales Engineer**: Creates warehouse configurations
- **Customer**: Reviews configuration (read-only)

### Key Features
#### 2.1 Real-Time Validation Panel
**Integrated in Configuration UI**

```text
┌─────────────────────────────────────────────────────────────┐
│ Warehouse Configuration - SPR Layout                         │
├─────────────────────────────────────────────────────────────┤
│ [Layout Canvas]                                              │
│                                                              │
│ ┌──────────────────────────────────────────────────────┐    │
│ │ Validation Results                    ⚠ 2 Warnings   │    │
│ ├──────────────────────────────────────────────────────┤    │
│ │ ✓ All required parameters provided                   │    │
│ │ ✓ Frame depth: 800mm (valid)                         │    │
│ │ ✓ Beam capacity: 2500kg (sufficient)                 │    │
│ │ ⚠ Height-to-depth ratio: 12.25 exceeds 6:1          │    │
│ │   → Additional stability components required         │    │
│ │ ⚠ Last loading level: 9800mm                         │    │
│ │   → Limited by MHE working height (10000mm)          │    │
│ │                                                       │    │
│ │ [View Details] [Export Report]                       │    │
│ └──────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

#### 2.2 Validation Details Modal
**Detailed Rule Execution View**

```text
┌─────────────────────────────────────────────────────────────┐
│ Validation Details                                      [×]  │
├─────────────────────────────────────────────────────────────┤
│ Configuration: SPR-2024-001                                  │
│ Evaluated: 98 rules in 450ms                                │
│ Status: ⚠ Warning (2 issues)                                │
│                                                              │
│ Tabs: [Summary] [Calculations] [Violations] [Trace]         │
│                                                              │
│ ═══ Calculations ═══════════════════════════════════════    │
│                                                              │
│ FrameDepth = 800mm                                           │
│   ← FD001: PalletDepth - 200                                │
│   ← PalletDepth = 1000mm [INPUT]                            │
│                                                              │
│ LastLoadingLevel = 9800mm                                    │
│   ← RH001c: MIN(MaxLevelWarehouse, MaxLevelMHE)             │
│   ← MaxLevelWarehouse = 11650mm                             │
│     ← RH001a: WarehouseClearHeight - 200 - PalletHeight     │
│     ← WarehouseClearHeight = 12000mm [INPUT]                │
│     ← PalletHeight = 150mm [INPUT]                          │
│   ← MaxLevelMHE = 9800mm                                    │
│     ← RH001b: MHEWorkingHeight - 200                        │
│     ← MHEWorkingHeight = 10000mm [INPUT]                    │
│                                                              │
│ HeightToDepthRatio = 12.25                                   │
│   ← RH005: LastLoadingLevel ÷ FrameDepth                    │
│   ← LastLoadingLevel = 9800mm [CALCULATED]                  │
│   ← FrameDepth = 800mm [CALCULATED]                         │
│                                                              │
│                                              [Close]         │
└─────────────────────────────────────────────────────────────┘
```

#### 2.3 What-If Analysis Tool
**Interactive Scenario Testing**

```text
┌─────────────────────────────────────────────────────────────┐
│ What-If Analysis                                        [×]  │
├─────────────────────────────────────────────────────────────┤
│ Current Configuration                                        │
│   MHE Working Height: 10000mm                                │
│   Result: LastLoadingLevel = 9800mm ⚠                       │
│                                                              │
│ Test Scenario:                                               │
│   MHE Working Height: [12000                              ]  │
│                                              [Simulate]       │
│                                                              │
│ Simulated Result:                                            │
│ ┌──────────────────────────────────────────────────────┐    │
│ │ LastLoadingLevel = 11650mm ✓                         │    │
│ │ HeightToDepthRatio = 14.56 ⚠                         │    │
│ │                                                       │    │
│ │ Impact:                                               │    │
│ │ • Increased storage capacity by 18.9%                │    │
│ │ • Still requires stability components                │    │
│ │ • Consider wider MHE for better ratio                │    │
│ └──────────────────────────────────────────────────────┘    │
│                                                              │
│                                    [Apply] [Cancel]          │
└─────────────────────────────────────────────────────────────┘
```

## 3. Testing & Debugging Interface

### Purpose
Allow QA engineers and developers to test rules, debug issues, and validate rule sets.

### User Roles
- **QA Engineer**: Test rule sets with various scenarios
- **Developer**: Debug rule evaluation issues

### Key Features
#### 3.1 Rule Preview/Simulation
**Standalone Testing Tool**

```text
┌─────────────────────────────────────────────────────────────┐
│ Rule Simulation                                              │
├─────────────────────────────────────────────────────────────┤
│ Rule Set:  [SPR v2 Design Rules                           ▼] │
│ Country:   [India (IS 15635)                              ▼] │
│                                                              │
│ Sample Inputs:                                               │
│ ┌──────────────────────────────────────────────────────┐    │
│ │ PalletWidth:           [1200                         ]│    │
│ │ PalletDepth:           [1000                         ]│    │
│ │ PalletsPerLevel:       [3                            ]│    │
│ │ WarehouseClearHeight:  [12000                        ]│    │
│ │ MHEWorkingHeight:      [10000                        ]│    │
│ │ PalletHeight:          [150                          ]│    │
│ │                        [+ Add Parameter]              │    │
│ └──────────────────────────────────────────────────────┘    │
│                                                              │
│ Trace Level: ○ Summary  ● Detailed  ○ Verbose               │
│                                                              │
│                                              [Simulate]       │
│                                                              │
│ Results:                                                     │
│ ┌──────────────────────────────────────────────────────┐    │
│ │ Execution Summary:                                   │    │
│ │ • 98 rules evaluated in 450ms                        │    │
│ │ • 96 passed, 2 warnings                              │    │
│ │ • 45 calculated values generated                     │    │
│ │                                                       │    │
│ │ [View Execution Trace] [Export Results]             │    │
│ └──────────────────────────────────────────────────────┘    │
└─────────────────────────────────────────────────────────────┘
```

#### 3.2 Execution Trace Viewer
**Detailed Step-by-Step Execution**

```text
┌─────────────────────────────────────────────────────────────┐
│ Execution Trace - Simulation #12345                    [×]  │
├─────────────────────────────────────────────────────────────┤
│ Total Time: 450ms | Rules: 98 | Status: ⚠ Warning           │
│                                                              │
│ Filters: [All] [Errors] [Warnings] [Slow (>10ms)]           │
│                                                              │
│ ┌──────────────────────────────────────────────────────┐    │
│ │ #1  FD001 - Frame Depth Calculation          2ms ✓  │    │
│ │     Input:  PalletDepth = 1000                       │    │
│ │     Formula: PalletDepth - 200                       │    │
│ │     Output: FrameDepth = 800                         │    │
│ ├──────────────────────────────────────────────────────┤    │
│ │ #2  RH001a - Warehouse Clearance             1ms ✓  │    │
│ │     Input:  WarehouseClearHeight = 12000             │    │
│ │             PalletHeight = 150                       │    │
│ │     Formula: WarehouseClearHeight - 200 - PalletHeight│   │
│ │     Output: MaxLevelWarehouse = 11650                │    │
│ ├──────────────────────────────────────────────────────┤    │
│ │ #15 RH006 - Stability Component Check        3ms ⚠  │    │
│ │     Input:  HeightToDepthRatio = 12.25               │    │
│ │     Formula: IF(HeightToDepthRatio > 6, TRUE)        │    │
│ │     Output: RequireStabilityComponents = TRUE        │    │
│ │     Warning: Height-to-depth ratio exceeds 6:1       │    │
│ └──────────────────────────────────────────────────────┘    │
│                                                              │
│                                    [Export] [Close]          │
└─────────────────────────────────────────────────────────────┘
```

## 4. User Journeys

### Journey 1: Admin Creates New Rule
1. Navigate to Admin → Rule Management
2. Select "SPR v2 Design Rules" rule set
3. Click "+ Add Rule"
4. Fill rule details:
   - Rule ID, Name, Description
   - Select category and type
   - Enter formula
   - Define parameters
   - Set validation conditions
5. Test formula with sample inputs
6. Save as draft
7. Request approval from Engineering Lead
8. Activate after approval

### Journey 2: Sales Engineer Configures Warehouse
1. Open Configuration UI
2. Enter warehouse parameters (dimensions, MHE, pallets)
3. Place racks on layout canvas
4. View real-time validation panel
5. See warnings about stability requirements
6. Click "View Details" to understand issue
7. Adjust configuration (change MHE or frame depth)
8. Validate again until all rules pass
9. Save configuration
10. Export validated design

### Journey 3: QA Engineer Tests Rule Set
1. Navigate to Testing → Rule Simulation
2. Select "SPR v2 Design Rules" + "India"
3. Load test scenario from library
4. Click "Simulate"
5. Review execution trace
6. Verify all rules executed correctly
7. Check performance (< 500ms target)
8. Export results for documentation
9. Mark test case as passed

### Journey 4: Developer Debugs Rule Issue
1. Receive bug report: "Frame depth calculation incorrect"
2. Navigate to Rule Simulation
3. Enter exact inputs from bug report
4. Run simulation with "Verbose" trace
5. Review execution trace step-by-step
6. Identify issue: Missing parameter validation
7. Navigate to Rule Editor
8. Fix rule formula
9. Test again with same inputs
10. Verify fix works correctly

## 5. API Integration Points

### For Frontend Developers

#### 5.1 Get Active Rules
`GET /api/v1/rule-evaluation/active-rules?productGroupId={guid}&countryId={guid}`

**Response:**
```json
{
  "ruleSetId": "guid",
  "name": "SPR v2 Design Rules",
  "rules": [
    {
      "id": "FD001",
      "name": "Frame Depth Calculation",
      "requiredParameters": ["PalletDepth"]
    }
  ]
}
```

#### 5.2 Evaluate Configuration
`POST /api/v1/rule-evaluation/evaluate`

**Request:**
```json
{
  "productGroupId": "guid",
  "configurationData": {
    "PalletWidth": 1200,
    "PalletDepth": 1000
  }
}
```

**Response:**
```json
{
  "success": true,
  "calculatedValues": {
    "FrameDepth": 800,
    "HeightToDepthRatio": 12.25
  },
  "violations": [
    {
      "ruleId": "RH006",
      "severity": "WARNING",
      "message": "Height-to-depth ratio exceeds 6:1"
    }
  ]
}
```

#### 5.3 Preview/Simulate
`POST /api/v1/rule-evaluation/preview`

**Request:**
```json
{
  "productGroupId": "guid",
  "sampleInputs": {
    "PalletDepth": 1000
  },
  "traceLevel": "DETAILED"
}
```

**Response:**
```json
{
  "executionTrace": [
    {
      "ruleId": "FD001",
      "inputs": {"PalletDepth": 1000},
      "output": {"FrameDepth": 800},
      "executionTimeMs": 2
    }
  ],
  "totalExecutionTimeMs": 450
}
```

## 6. Implementation Priority

- **Phase 1 (MVP)**:
  - ✅ Rule Set list view (read-only)
  - ✅ Real-time validation panel in configurator
  - ✅ Basic validation details modal
- **Phase 2**:
  - ✅ Rule editor (create/edit)
  - ✅ Formula tester
  - ✅ Execution trace viewer
- **Phase 3**:
  - ✅ Load chart management
  - ✅ What-if analysis tool
  - ✅ Advanced debugging tools

## 7. Technical Requirements

### Frontend Stack
- **Framework**: React/Next.js
- **State Management**: Redux/Zustand
- **UI Library**: Material-UI or Ant Design
- **Code Editor**: Monaco Editor (for formula editing)
- **Visualization**: D3.js (for data lineage graphs)

### Performance Targets
- Rule list load: < 500ms
- Validation feedback: < 1s (real-time)
- Simulation execution: < 2s
- Trace viewer render: < 300ms

### Accessibility
- WCAG 2.1 Level AA compliance
- Keyboard navigation support
- Screen reader compatible
- High contrast mode

## Summary
This UI documentation covers:
- Admin interfaces for rule and load chart management
- Configurator integration for real-time validation
- Testing tools for QA and debugging
- User journeys for common workflows
- API integration points for frontend developers

The interfaces are designed to be intuitive, provide clear feedback, and support the complete rule lifecycle from creation to production use.
