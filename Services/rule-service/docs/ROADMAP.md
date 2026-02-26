# Rule Service Roadmap

## Mission
Build a scalable, product-agnostic rule evaluation engine that supports complex formulas, dependency resolution, and multi-product group design rules with complete audit trail and data lineage tracking.

---

## Current Status
**Phase**: Planning Complete ✅  
**Next**: Implementation Phase 1  
**Target**: SPR v2 Design Rules Integration (98 rules)

---

## Phases Overview

### Phase 1: Foundation & SPR Rules (Q1 2026) 🎯
**Goal**: Establish formula support, dependency resolution, and seed SPR design rules

- ✅ Planning & Architecture Design
- 🔄 Database Schema Extensions
- 🔄 Domain Model Enhancements
- 🔄 Formula Evaluation Engine
- 🔄 SPR Rules Seeding (98 rules)

### Phase 2: Rule Execution & Integration (Q1-Q2 2026)
**Goal**: Build robust rule evaluation service with real-time validation

- Rule execution engine with topological sort
- Configuration service integration
- Preview/simulation mode
- Error handling & performance optimization

### Phase 3: Load Charts & Lookup Integration (Q2 2026)
**Goal**: Integrate load chart management for structural validation

- Load chart types & versioning
- Catalog service integration
- Admin UI for chart management
- API for capacity lookups

### Phase 4: Multi-Product Expansion (Q2-Q3 2026)
**Goal**: Extend to other product groups beyond SPR

- Cantilever rules
- ASRS rules
- Warehouse-level rules
- Country-specific compliance rules

---

## Phase 1 Details: Foundation & SPR Rules

### Milestone 1.1: Database Schema ✅ Designed
**Status**: Ready for implementation

**Deliverables**:
- [x] Migration: AddFormulaSupport (formula, output_field, rule_type, priority, execution_phase, parameters)
- [x] Migration: SeedSPRv2Rules (69 design rules)
- [x] Migration: SeedStabilityGuidelines (29 stability rules)
- [ ] Execute migrations
- [ ] Verify data integrity

**Schema Enhancements**:
```sql
-- Rules table extensions
ALTER TABLE rules ADD COLUMN formula TEXT;
ALTER TABLE rules ADD COLUMN output_field VARCHAR(255);
ALTER TABLE rules ADD COLUMN rule_type VARCHAR(50) DEFAULT 'CONDITION';
ALTER TABLE rules ADD COLUMN priority INT DEFAULT 500;
ALTER TABLE rules ADD COLUMN execution_phase VARCHAR(50) DEFAULT 'CALCULATION';
ALTER TABLE rules ADD COLUMN parameters JSONB;

-- Rule dependencies table
CREATE TABLE rule_dependencies (
    rule_id UUID REFERENCES rules(id),
    depends_on_rule_id UUID REFERENCES rules(id),
    PRIMARY KEY (rule_id, depends_on_rule_id)
);
```

---

### Milestone 1.2: Domain Model ✅ Designed
**Status**: Ready for implementation

**Deliverables**:
- [ ] Extend `Rule` entity with formula properties
- [ ] Create `ParameterDefinition` value object
- [ ] Create `RuleEvaluationContext` class
- [ ] Create enums: `RuleType`, `ExecutionPhase`, `ParameterSource`

**Key Classes**:
```csharp
public class Rule {
    public string Formula { get; set; }
    public string OutputField { get; set; }
    public RuleType RuleType { get; set; }
    public int Priority { get; set; }
    public ExecutionPhase ExecutionPhase { get; set; }
    public List<ParameterDefinition> Parameters { get; set; }
    public List<RuleDependency> Dependencies { get; set; }
}

public enum ParameterSource {
    STATIC, PARAMETRIC, CALCULATED, LOOKUP
}
```

---

### Milestone 1.3: Formula Evaluation Engine ✅ Designed
**Status**: Ready for implementation

**Deliverables**:
- [ ] Implement `FormulaEvaluator` with NCalc integration
- [ ] Support mathematical operations (+, -, *, /, MIN, MAX, IF, ROUND)
- [ ] Support custom functions (SelectBeam, LookupCapacity, BestFitCalculation)
- [ ] Implement expression caching for performance
- [ ] Error handling (division by zero, null values, missing fields)
- [ ] Unit tests for formula evaluation

**Capabilities**:
- Mathematical: `PalletDepth - 200`
- Aggregation: `MIN(MaxLevelWarehouse, MaxLevelMHE)`
- Conditional: `IF(HeightToDepthRatio > 6, TRUE, FALSE)`
- Lookup: `LookupCapacity('BEAM', BeamType, BeamSpan)`

---

### Milestone 1.4: SPR Rules Seeding ✅ Designed
**Status**: Ready for implementation

**Deliverables**:
- [ ] Seed 69 SPR design rules from GSS_DesignRules_Extraction.csv
- [ ] Seed 29 stability guidelines from Stability_Guidelines.csv
- [ ] Set up rule dependencies based on CSV
- [ ] Validate all 98 rules seed correctly

**Rule Categories**:
- Level to Level (LL)
- First Level (FL)
- Rack Width & Depth (RW, RD)
- Overhang (PO)
- Rack Height (RH) - 6 rules
- Number of Loading Levels (NL)
- Pallet Selection (PS) - 5 rules
- Frame Depth (FD) - 3 rules
- Beam Selection (BS) - 4 rules
- Rack Placement (RP) - 11 rules
- MHE Rules (MHE) - 11 rules
- Stability (from Stability_Guidelines.csv)

---

## Phase 2 Details: Rule Execution & Integration

### Milestone 2.1: Rule Evaluation Service
**Target**: Q1 2026

**Deliverables**:
- [ ] Implement topological sort (Kahn's algorithm)
- [ ] Circular dependency detection
- [ ] Pre-evaluation parameter validation
- [ ] Priority-based execution within dependency levels
- [ ] Phase-based grouping (INPUT_VALIDATION → CALCULATION → STRUCTURAL → COMPLIANCE → OPTIMIZATION → OUTPUT_GENERATION)
- [ ] Context management with parameter source tracking
- [ ] Execution trace and data lineage
- [ ] Structured logging (Serilog)

**Architecture Highlights**:
```
Input Parameters
  ↓
Pre-Validation (fail-fast)
  ↓
Dependency Resolution (topological sort)
  ↓
Execution by Phase & Priority
  ↓
Context Updates (track lineage)
  ↓
Result Aggregation
  ↓
Execution Trace Output
```

---

### Milestone 2.2: Preview/Simulation Mode
**Target**: Q1 2026

**Deliverables**:
- [ ] Implement `PreviewRuleSetAsync` method
- [ ] Detailed execution trace with timing
- [ ] What-if analysis support
- [ ] API endpoint: `POST /api/v1/rule-evaluation/preview`

**Use Cases**:
- Test formulas before deploying
- Debugging rule issues
- Training users on rule behavior
- What-if scenario analysis

---

### Milestone 2.3: Configuration Service Integration
**Target**: Q1-Q2 2026

**Deliverables**:
- [ ] Create `RuleServiceClient` in configuration-service
- [ ] Implement retry and circuit breaker
- [ ] Integrate with `StorageConfigurationHandler`
- [ ] Return validation results to frontend
- [ ] Integration tests

---

## Phase 3 Details: Load Charts & Lookup Integration

### Milestone 3.1: Load Chart Schema (catalog-service)
**Target**: Q2 2026

**Deliverables**:
- [ ] Create `load_chart_types` table (abstraction layer)
- [ ] Create `load_charts` table (versioned, auditable)
- [ ] Create `load_chart_entries` table (JSONB dimensions)
- [ ] Create `load_chart_audit_log` table
- [ ] Seed beam load charts from GSS_Beam_Load_Charts.csv (612 entries)

**Architecture**:
```sql
load_chart_types (BEAM_CAPACITY, UPRIGHT_CAPACITY, ARM_CAPACITY)
  ↓
load_charts (versioned, DRAFT → APPROVED → ACTIVE → DEPRECATED)
  ↓
load_chart_entries (flexible JSONB dimensions)
  ↓
load_chart_audit_log (who, when, what, why, where)
```

---

### Milestone 3.2: Load Chart Management API
**Target**: Q2 2026

**Deliverables**:
- [ ] POST /api/v1/load-charts (create chart)
- [ ] POST /api/v1/load-charts/{id}/entries (add entry)
- [ ] POST /api/v1/load-charts/{id}/entries/bulk (CSV import)
- [ ] PUT /api/v1/load-charts/{id}/entries/{entryId} (update with versioning)
- [ ] POST /api/v1/load-charts/{id}/approve (approval workflow)
- [ ] POST /api/v1/load-charts/{id}/activate (activate version)
- [ ] GET /api/v1/load-charts/lookup (query capacity)
- [ ] GET /api/v1/load-charts/{id}/audit (audit trail)
- [ ] DELETE /api/v1/load-charts/{id}/entries/{entryId} (soft delete)

---

### Milestone 3.3: Admin UI for Load Charts
**Target**: Q2 2026

**Deliverables**:
- [ ] Load chart list view
- [ ] Load chart editor with version history
- [ ] CSV/Excel upload interface
- [ ] Approval workflow UI
- [ ] Audit trail viewer

---

## Phase 4 Details: Multi-Product Expansion

### Milestone 4.1: Rule Scope Hierarchy
**Target**: Q2 2026

**Deliverables**:
- [ ] Implement scope levels (GLOBAL, COUNTRY, WAREHOUSE, PRODUCT_GROUP, PRODUCT_COUNTRY)
- [ ] Rule inheritance for product group variants
- [ ] Scope-based rule merging in evaluation

**Scope Levels**:
```
GLOBAL → Universal rules (fire safety, seismic)
COUNTRY → IS 15635 (India), RMI (USA)
WAREHOUSE → Building clearance, floor capacity
PRODUCT_GROUP → SPR, Cantilever, ASRS specific rules
PRODUCT_COUNTRY → SPR + IS 15635 compliance
```

---

### Milestone 4.2: Additional Product Groups
**Target**: Q3 2026

**Deliverables**:
- [ ] Cantilever design rules
- [ ] ASRS design rules
- [ ] Double-Deep, Drive-In, Push-Back rules
- [ ] Warehouse-level global rules

---

## Technical Specifications

### Performance Targets
| Metric | Target | Rationale |
|--------|--------|-----------|
| Single rule evaluation | < 5ms | Fast feedback |
| Full rule set (100 rules) | < 500ms | Acceptable for autosave |
| Load chart lookup | < 10ms | Cached |
| Preview mode | < 1s | Interactive |
| Circular dependency detection | < 100ms | Fail fast |

### Data Lineage
Every calculated value tracks:
- Parameter name
- Value
- Source (STATIC, PARAMETRIC, CALCULATED, LOOKUP)
- Source rule (if calculated)
- Timestamp

### Error Handling
- **Pre-validation**: Check required parameters before evaluation
- **Graceful degradation**: One rule failure doesn't stop others
- **Clear errors**: Structured error responses with context
- **Logging**: Trace, Debug, Info, Warning, Error levels

---

## Dependencies

### External Services
- **catalog-service**: Load chart data, component types
- **configuration-service**: Configuration data for rule evaluation

### Libraries
- **NCalc**: Formula evaluation
- **Serilog**: Structured logging
- **FluentValidation**: Input validation
- **Polly**: Retry and circuit breaker

### Data Sources
- GSS_DesignRules_Extraction.csv (69 rules)
- Stability_Guidelines.csv (29 rules)
- GSS_Beam_Load_Charts.csv (612 capacity entries)

---

## Success Metrics

### Phase 1
- ✅ 98 rules seeded successfully
- ✅ Formula evaluator handles all test cases
- ✅ Database schema migration successful

### Phase 2
- ✅ All 98 rules evaluate correctly
- ✅ Performance targets met
- ✅ Zero circular dependency errors
- ✅ Configuration service integration working

### Phase 3
- ✅ 612 beam load chart entries imported
- ✅ Load chart lookup < 10ms
- ✅ Admin can manage charts via UI
- ✅ Full audit trail for all changes

### Phase 4
- ✅ 3+ product groups supported
- ✅ Scope hierarchy working
- ✅ Country-specific rules active

---

## Risks & Mitigations

| Risk | Impact | Mitigation |
|------|--------|------------|
| Complex formula parsing | High | Use proven NCalc library, extensive testing |
| Circular dependencies | High | Implement detection, fail fast with clear error |
| Performance degradation | Medium | Expression caching, parallel execution |
| Load chart data quality | Medium | Validation on import, approval workflow |
| Multi-product complexity | Medium | Abstract architecture, clear scope separation |

---

## Next Steps

### Immediate (This Week)
1. Execute database migrations
2. Implement `FormulaEvaluator` with NCalc
3. Extend `Rule` entity
4. Begin unit tests

### Short-Term (Next 2 Weeks)
1. Seed SPR rules
2. Implement rule evaluation service
3. Add topological sort
4. Integration with configuration-service

### Medium-Term (Next Month)
1. Load chart schema migration
2. Load chart API implementation
3. Preview mode endpoint
4. Admin UI for load charts

---

## References

- [Implementation Plan](file:///C:/Users/sk72/.gemini/antigravity/brain/b4e9a5c3-d77e-41bd-92c0-2f336083e42b/implementation_plan.md)
- [Task Breakdown](file:///C:/Users/sk72/.gemini/antigravity/brain/b4e9a5c3-d77e-41bd-92c0-2f336083e42b/task.md)
- [UI Requirements](file:///C:/Users/sk72/.gemini/antigravity/brain/b4e9a5c3-d77e-41bd-92c0-2f336083e42b/ui-requirements.md)
- [Load Chart API Spec](file:///C:/Users/sk72/.gemini/antigravity/brain/b4e9a5c3-d77e-41bd-92c0-2f336083e42b/load-chart-api-spec.md)
- [Load Charts Documentation](file:///c:/Users/sk72/Projects/gss/gss-backend/Services/rule-service/docs/03-rule-engine/load-charts.md)
- [SPR Rules Documentation](file:///c:/Users/sk72/Projects/gss/gss-backend/Services/rule-service/docs/04-product-groups/SPR)

---

**Last Updated**: 2026-01-21  
**Version**: 1.0  
**Owner**: Engineering Team  
**Status**: Planning Complete, Ready for Implementation
