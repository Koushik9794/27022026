# ADR-001: Domain-Driven Design Architecture

## Status

Accepted

## Context

The Catalog Service needs a clear architectural approach that:
- Encapsulates business logic
- Separates concerns
- Supports future growth
- Integrates with other GSS services

## Decision

Adopt **Domain-Driven Design (DDD)** with:
- Rich domain model (aggregates: SKU, Pallet, MHE)
- CQRS pattern using WolverineFx
- Repository pattern with Dapper
- Clear layer separation (API, Application, Domain, Infrastructure)

## Consequences

### Positive
- Business logic is encapsulated in domain
- Clear separation of read/write operations
- Testable in isolation
- Consistent with rule-service architecture

### Negative
- More initial setup complexity
- Learning curve for WolverineFx
- Requires discipline to maintain layer boundaries
