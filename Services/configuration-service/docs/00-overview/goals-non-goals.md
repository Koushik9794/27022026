# Goals and Non-Goals

## Goals

### Primary Objectives

1. **Enquiry Configuration Management**
   - Store configurations linked to external enquiries
   - Version control for configuration iterations
   - Snapshot support for milestone preservation

2. **Customer-Specific Data Capture**
   - Customer's product specifications (CustomerSku)
   - Customer's pallet specifications (CustomerPallet)
   - Building constraints (WarehouseConfig)

3. **Design Output Storage**
   - Store racking configurations
   - Link to catalog-service master data
   - Support BOM generation downstream

4. **DDD Architecture**
   - Rich domain model with encapsulated logic
   - CQRS pattern using MediatR
   - Clean separation of concerns

## Non-Goals

### Explicitly Out of Scope

| Non-Goal | Reason |
|----------|--------|
| Master data management | Handled by catalog-service |
| Rule evaluation | Handled by rule-service |
| BOM generation | Handled by bom-service |
| Customer/user management | Handled by admin-service |
| File storage | Handled by file-service |
| Enquiry metadata | Stored in external CRM/ERP |

## Success Metrics

| Metric | Target |
|--------|--------|
| API Response Time | < 100ms for configuration queries |
| Version History Depth | 100+ versions per enquiry |
| Snapshot Limit | 50 per enquiry |
| Uptime | 99.9% |
