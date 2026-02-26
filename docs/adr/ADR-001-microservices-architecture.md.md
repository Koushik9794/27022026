# ADR-001: Adoption of Microservices Architecture
## Status: Draft

## Context
The Godrej Warehouse Configurator is a rule-intensive, engineering-driven system involving:
- Enquiry lifecycle with revisions
- 2D layout configuration and automation
- Structural rule validation
- BOM explosion and costing
- File handling (DWG, DXF, GLB, exports)
- Role-based administration and governance
- Audit and traceability

## Key characteristics of the domain:
- Different change rates (rules vs UI vs BOM logic)
- Compute-heavy workloads (automation, BOM, simulations)
- Strict domain ownership (Admin, Configurator, BOM, Audit)
- Future integrations (PLM, ERP, partner systems)
- Need for independent scalability (e.g., BOM vs Admin)

## A single monolithic service would:
- Increase coupling between unrelated domains
- Slow down independent evolution of rule engines
- Make scaling compute-heavy components inefficient
- Increase regression risk during frequent rule changes

## Decision
The system will be designed as a set of domain-aligned microservices, each owning its data, business rules, and API contracts.

Each microservice will:
- Represent a bounded context
- Expose a versioned OpenAPI contract
- Own its persistence model
- Communicate via APIs and domain events
- Be independently deployable and scalable

## Rationale
- Microservices are chosen not primarily for technical scalability, but for domain scalability and governance.

This architecture enables:

- Independent evolution of rule engines and BOM logic
- Clear ownership and accountability per domain
- Isolation of high-change and high-compute components
- Safer experimentation and optimization in configurator automation
- Cleaner integration with external engineering and enterprise systems
- The architecture aligns with Domain-Driven Design, where each bounded context maps to a deployable service.

### Consequences
## Positive
- Independent scaling of compute-heavy services (Configurator, BOM)
- Reduced blast radius of changes
- Clear API contracts and versioning discipline
- Easier long-term integration and extensibility

## Negative
- Increased operational complexity
- Need for strong API governance and versioning
- Higher initial development overhead
- Requires disciplined DevOps and observability

These consequences are accepted, as the long-term domain complexity outweighs the short-term simplicity of a monolith.