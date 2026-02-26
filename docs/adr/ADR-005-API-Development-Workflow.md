# ADR-005: API Development Workflow

## Status
Accepted

---

## Context

GSS microservices require a consistent approach for developing and documenting REST APIs that balances productivity with maintainability.

---

## Decision

**Adopt a code-first API development approach:**

- Controllers are implemented using ASP.NET Core MVC
- OpenAPI documentation is auto-generated from controller code via Swashbuckle
- No separate specification files require manual maintenance

---

## Workflow

### Adding a New API Endpoint

| Step | Artifact | Location |
|------|----------|----------|
| 1 | Domain Entity | `src/domain/aggregates/` |
| 2 | Repository | `src/infrastructure/persistence/repositories/` |
| 3 | DTO | `src/application/dtos/` |
| 4 | Command/Query | `src/application/commands/` or `queries/` |
| 5 | Handler | `src/application/handlers/` |
| 6 | Controller Endpoint | `src/api/controllers/` |
| 7 | Dependency Injection | `Program.cs` |
| 8 | Database Migration | `src/infrastructure/migrations/` |
| 9 | Unit Tests | `tests/unit/` |

---

## API Documentation

OpenAPI/Swagger documentation is automatically generated at runtime:

| Environment | Swagger UI |
|-------------|------------|
| Development | http://localhost:{port}/swagger |
| Docker | http://localhost:5002/swagger |

---

## Consequences

### Positive
- Reduced maintenance overhead (no separate spec files)
- Documentation always matches implementation
- Standard .NET tooling with no additional dependencies

### Negative
- API design is finalized during implementation

---

## Related Decisions

- [ADR-002: API Contract Governance](ADR-002-API%20Contract%20Governance%20Using%20OpenAPI.md)
- [ADR-004: REST API Design Standards](ADR-004-REST-api-design-standards.md)
