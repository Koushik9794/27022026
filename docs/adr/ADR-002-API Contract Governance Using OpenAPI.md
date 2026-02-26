# ADR-002: API Contract Governance Using OpenAPI

## Status
Draft

---

## Context
The Godrej Warehouse Configurator is designed as a **microservices-based system**, with independently evolving bounded contexts such as:

- Admin & Reference Data  
- Enquiry Management  
- Warehouse Configurator  
- BOM Generation  
- Audit & Traceability  

Each service exposes APIs consumed by:

- Web-based 2D configurator  
- Estimation and BOM engines  
- External enterprise systems (ERP / PLM)  
- Future AI-based coding and analysis agents  
---

## Decision

**OpenAPI (v3.1) specifications will be the single source of truth for all service APIs.**

- Each microservice will own and maintain its OpenAPI contract  
- OpenAPI files will be version-controlled within the service repository  
- Word or PDF documents, if required, will be generated from OpenAPI and treated as secondary artifacts  
- No API endpoint, request, or response is considered valid unless defined in OpenAPI  

---

## Rationale

An OpenAPI-first approach ensures:

- Contract clarity between services and consumers  
- Early validation of API changes through CI/CD  
- Backward compatibility control via semantic versioning  
- Automation readiness for client generation, testing, and AI-assisted development  
- Reduced ambiguity in rule-intensive domains such as configuration, automation, and BOM logic  

This governance model aligns with:

- Domain-Driven Design (bounded context ownership)  
- Microservices autonomy  
- Continuous delivery practices  

---

## Consequences

### Positive

- Clear, enforceable API contracts  
- Reduced integration defects  
- Faster onboarding for developers and agents  
- Consistent API behavior across environments  

### Negative

- Requires discipline to update OpenAPI alongside code  
- Teams must learn and adhere to OpenAPI standards  

These consequences are accepted as necessary for long-term maintainability and scalability.
