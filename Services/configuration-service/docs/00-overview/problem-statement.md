# Problem Statement

## The Challenge

Warehouse configurator projects require:
- **Enquiry tracking** linked to external CRM/sales systems
- **Customer-specific data** (their products, pallets, building constraints)
- **Version control** for configuration iterations
- **Snapshot management** for milestone preservation

Without a dedicated configuration service:
- Configuration data is scattered across systems
- No audit trail of design iterations
- Customer requirements are not properly captured
- Integration with external enquiry systems is ad-hoc

## What the Configuration Service Solves

The Configuration Service provides **project-specific data management** for warehouse designs.

| Problem | Solution |
|---------|----------|
| External enquiry scattered | `Enquiry` entity with external ID reference |
| Customer SKUs undefined | `CustomerSku` with specific dimensions/weights |
| Pallet specs unknown | `CustomerPallet` with capacity limits |
| Building constraints lost | `WarehouseConfig` with spatial data |
| Design versions untracked | `EnquiryVersion` with change history |

## Business Value

1. **Traceability** — Link configurations to sales enquiries
2. **Accuracy** — Capture exact customer requirements
3. **Iteration** — Track design changes over time
4. **Recovery** — Restore previous configurations via snapshots
5. **Integration** — Connect to external CRM/ERP systems
