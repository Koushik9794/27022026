# Goals and Non-Goals

## Goals

### Primary Objectives

1. **Centralized Component Catalog**
   - Single source of truth for SKUs, Pallets, MHE
   - Managed lifecycle (create, update, soft-delete)
   - Rich metadata with 3D model references

2. **UI Palette Configuration**
   - Serve component palette for DesignDeck configurator
   - Support role-based visibility (Admin, Designer, Dealer)
   - Group and categorize components

3. **Integration with Rule Service**
   - Provide component attributes for rule evaluation
   - Serve load charts tied to SKUs
   - Support capacity lookups

4. **DDD Architecture**
   - Rich domain model with encapsulated logic
   - CQRS pattern using WolverineFx
   - Clean separation of concerns

## Non-Goals

### Explicitly Out of Scope

| Non-Goal | Reason |
|----------|--------|
| 3D Model Rendering | Handled by frontend/BFF |
| Authentication/Authorization | Handled by BFF layer |
| Pricing/Quoting | Separate pricing service |
| Inventory Management | ERP system responsibility |
| Order Processing | Order service scope |
| File Storage | Managed externally (CDN/blob) |

## Success Metrics

| Metric | Target |
|--------|--------|
| API Response Time | < 50ms for catalog queries |
| Palette Load Time | < 100ms |
| Uptime | 99.9% |
| Cache Hit Ratio | > 95% |
